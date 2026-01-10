using System;
using System.Collections.Generic;
using System.Linq;

namespace CanonicalLR1Parser
{
    /// <summary>
    /// Builds the canonical collection of LR(1) states
    /// </summary>
    public class CanonicalCollection
    {
        private Grammar grammar;
        private LR1Operations operations;
        public List<LR1State> States { get; private set; }
        
        // Transitions: (stateId, symbol) -> targetStateId
        public Dictionary<(int, string), int> Transitions { get; private set; }

        public CanonicalCollection(Grammar grammar, LR1Operations operations)
        {
            this.grammar = grammar;
            this.operations = operations;
            States = new List<LR1State>();
            Transitions = new Dictionary<(int, string), int>();
        }

        /// <summary>
        /// Builds the canonical collection starting from the initial item
        /// </summary>
        public void Build()
        {
            // Create initial state: [S' → • S, $]
            var initialItem = new LR1Item(grammar.Productions[0], 0, "$");
            var initialItems = new HashSet<LR1Item> { initialItem };
            var initialState = new LR1State(operations.Closure(initialItems));
            initialState.StateId = 0;
            
            States.Add(initialState);
            
            // Use a queue to process states
            var stateQueue = new Queue<LR1State>();
            stateQueue.Enqueue(initialState);
            
            // Keep track of states we've seen (for duplicate detection)
            var stateMap = new Dictionary<string, int>();
            stateMap[GetStateKey(initialState)] = 0;

            while (stateQueue.Count > 0)
            {
                var currentState = stateQueue.Dequeue();
                
                // Get all symbols that can be shifted from this state
                var symbols = operations.GetShiftableSymbols(currentState.Items);

                foreach (var symbol in symbols)
                {
                    // Compute GOTO(currentState, symbol)
                    var gotoItems = operations.Goto(currentState.Items, symbol);
                    
                    if (gotoItems.Count == 0)
                    {
                        continue;
                    }

                    var gotoState = new LR1State(gotoItems);
                    string stateKey = GetStateKey(gotoState);

                    int targetStateId;
                    if (stateMap.ContainsKey(stateKey))
                    {
                        // State already exists
                        targetStateId = stateMap[stateKey];
                    }
                    else
                    {
                        // New state
                        targetStateId = States.Count;
                        gotoState.StateId = targetStateId;
                        States.Add(gotoState);
                        stateMap[stateKey] = targetStateId;
                        stateQueue.Enqueue(gotoState);
                    }

                    // Record the transition
                    Transitions[(currentState.StateId, symbol)] = targetStateId;
                }
            }
        }

        /// <summary>
        /// Creates a unique key for a state based on its items
        /// </summary>
        private string GetStateKey(LR1State state)
        {
            var sortedItems = state.Items.OrderBy(i => i.ToString()).ToList();
            return string.Join("|", sortedItems.Select(i => i.ToString()));
        }

        /// <summary>
        /// Prints all states in the canonical collection
        /// </summary>
        public void PrintStates()
        {
            Console.WriteLine("=== CANONICAL LR(1) STATES ===");
            Console.WriteLine();
            
            foreach (var state in States)
            {
                Console.WriteLine($"State {state.StateId}:");
                foreach (var item in state.Items.OrderBy(i => i.ToString()))
                {
                    Console.WriteLine($"  {item}");
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Prints all transitions in the DFA
        /// </summary>
        public void PrintTransitions()
        {
            Console.WriteLine("=== STATE TRANSITIONS ===");
            
            foreach (var kvp in Transitions.OrderBy(t => t.Key.Item1).ThenBy(t => t.Key.Item2))
            {
                var (stateId, symbol) = kvp.Key;
                var targetStateId = kvp.Value;
                Console.WriteLine($"State {stateId} --{symbol}--> State {targetStateId}");
            }
            
            Console.WriteLine();
        }
    }
}
