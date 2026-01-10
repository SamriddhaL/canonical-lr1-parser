using System;
using System.Collections.Generic;
using System.Linq;

namespace CanonicalLR1Parser
{
    public enum ActionType
    {
        Shift,
        Reduce,
        Accept,
        Error
    }

    public class ParserAction
    {
        public ActionType Type { get; }
        public int Value { get; } // State number for shift, production number for reduce

        public ParserAction(ActionType type, int value = -1)
        {
            Type = type;
            Value = value;
        }

        public override string ToString()
        {
            return Type switch
            {
                ActionType.Shift => $"s{Value}",
                ActionType.Reduce => $"r{Value}",
                ActionType.Accept => "acc",
                ActionType.Error => "err",
                _ => "?"
            };
        }
    }

    /// <summary>
    /// Constructs and manages the LR(1) parsing tables
    /// </summary>
    public class ParsingTable
    {
        private Grammar grammar;
        private CanonicalCollection collection;
        
        // ACTION table: [state, terminal] -> action
        public Dictionary<(int, string), ParserAction> ActionTable { get; private set; }
        
        // GOTO table: [state, non-terminal] -> state
        public Dictionary<(int, string), int> GotoTable { get; private set; }
        
        public bool HasConflicts { get; private set; }
        private List<string> conflicts;

        public ParsingTable(Grammar grammar, CanonicalCollection collection)
        {
            this.grammar = grammar;
            this.collection = collection;
            ActionTable = new Dictionary<(int, string), ParserAction>();
            GotoTable = new Dictionary<(int, string), int>();
            HasConflicts = false;
            conflicts = new List<string>();
        }

        /// <summary>
        /// Constructs the ACTION and GOTO tables
        /// </summary>
        public void ConstructTables()
        {
            foreach (var state in collection.States)
            {
                foreach (var item in state.Items)
                {
                    if (!item.IsComplete())
                    {
                        // Shift items: [A → α • a β, b] where a is a terminal
                        string symbolAfterDot = item.GetSymbolAfterDot();
                        
                        if (grammar.IsTerminal(symbolAfterDot) && symbolAfterDot != "$")
                        {
                            // Find the target state for this shift
                            if (collection.Transitions.TryGetValue((state.StateId, symbolAfterDot), out int targetState))
                            {
                                AddAction(state.StateId, symbolAfterDot, new ParserAction(ActionType.Shift, targetState));
                            }
                        }
                    }
                    else
                    {
                        // Reduce items: [A → α •, a]
                        if (item.Production.LeftHandSide == grammar.AugmentedStart)
                        {
                            // Accept item: [S' → S •, $]
                            AddAction(state.StateId, "$", new ParserAction(ActionType.Accept));
                        }
                        else
                        {
                            // Reduce item
                            int productionIndex = grammar.GetProductionIndex(item.Production);
                            AddAction(state.StateId, item.Lookahead, new ParserAction(ActionType.Reduce, productionIndex));
                        }
                    }
                }

                // GOTO entries for non-terminals
                foreach (var nonTerminal in grammar.NonTerminals)
                {
                    if (collection.Transitions.TryGetValue((state.StateId, nonTerminal), out int targetState))
                    {
                        GotoTable[(state.StateId, nonTerminal)] = targetState;
                    }
                }
            }
        }

        /// <summary>
        /// Adds an action to the ACTION table and detects conflicts
        /// </summary>
        private void AddAction(int state, string terminal, ParserAction action)
        {
            var key = (state, terminal);
            
            if (ActionTable.ContainsKey(key))
            {
                var existing = ActionTable[key];
                if (!ActionEquals(existing, action))
                {
                    HasConflicts = true;
                    conflicts.Add($"Conflict in state {state} on '{terminal}': {existing} vs {action}");
                }
            }
            else
            {
                ActionTable[key] = action;
            }
        }

        private bool ActionEquals(ParserAction a1, ParserAction a2)
        {
            return a1.Type == a2.Type && a1.Value == a2.Value;
        }

        /// <summary>
        /// Prints the parsing tables
        /// </summary>
        public void PrintTables()
        {
            Console.WriteLine("=== PARSING TABLE ===");
            Console.WriteLine();

            // Get all terminals and non-terminals
            var terminals = grammar.Terminals.OrderBy(t => t).ToList();
            var nonTerminals = grammar.NonTerminals.Where(nt => nt != grammar.AugmentedStart).OrderBy(nt => nt).ToList();

            // Print header
            Console.Write("State".PadRight(8));
            Console.Write("| ACTION".PadRight(60));
            Console.Write("| GOTO".PadRight(30));
            Console.WriteLine();
            Console.WriteLine(new string('-', 100));

            // Print for each state
            foreach (var state in collection.States.OrderBy(s => s.StateId))
            {
                Console.Write($"{state.StateId}".PadRight(8));
                Console.Write("| ");

                // ACTION columns
                foreach (var terminal in terminals)
                {
                    if (ActionTable.TryGetValue((state.StateId, terminal), out var action))
                    {
                        Console.Write($"{terminal}:{action} ".PadRight(10));
                    }
                }

                Console.Write("| ");

                // GOTO columns
                foreach (var nonTerminal in nonTerminals)
                {
                    if (GotoTable.TryGetValue((state.StateId, nonTerminal), out int targetState))
                    {
                        Console.Write($"{nonTerminal}:{targetState} ".PadRight(8));
                    }
                }

                Console.WriteLine();
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Reports whether conflicts exist
        /// </summary>
        public void ReportConflicts()
        {
            if (HasConflicts)
            {
                Console.WriteLine("=== CONFLICTS DETECTED ===");
                foreach (var conflict in conflicts)
                {
                    Console.WriteLine(conflict);
                }
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("=== NO CONFLICTS ===");
                Console.WriteLine("The grammar is Canonical LR(1) and conflict-free!");
                Console.WriteLine();
            }
        }

        public ParserAction GetAction(int state, string terminal)
        {
            if (ActionTable.TryGetValue((state, terminal), out var action))
            {
                return action;
            }
            return new ParserAction(ActionType.Error);
        }

        public int GetGoto(int state, string nonTerminal)
        {
            if (GotoTable.TryGetValue((state, nonTerminal), out int targetState))
            {
                return targetState;
            }
            return -1;
        }
    }
}
