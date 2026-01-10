using System;
using System.Collections.Generic;
using System.Linq;

namespace CanonicalLR1Parser
{
    /// <summary>
    /// Stack-based LR(1) parsing engine
    /// </summary>
    public class LR1Parser
    {
        private Grammar grammar;
        private ParsingTable table;

        public LR1Parser(Grammar grammar, ParsingTable table)
        {
            this.grammar = grammar;
            this.table = table;
        }

        /// <summary>
        /// Parses the input and returns parsing steps for UI display
        /// </summary>
        public List<ParseStep> ParseWithSteps(List<string> inputTokens)
        {
            var steps = new List<ParseStep>();

            // Initialize stacks
            var stateStack = new Stack<int>();
            var symbolStack = new Stack<string>();
            
            stateStack.Push(0);  // Initial state
            
            // Add end-of-input marker
            var input = new List<string>(inputTokens);
            input.Add("$");
            
            int inputIndex = 0;

            while (true)
            {
                int currentState = stateStack.Peek();
                string currentToken = input[inputIndex];

                // Get action
                var action = table.GetAction(currentState, currentToken);

                // Record step
                var step = new ParseStep
                {
                    StateStack = new Stack<int>(stateStack.Reverse()),
                    SymbolStack = new Stack<string>(symbolStack.Reverse()),
                    RemainingInput = input.Skip(inputIndex).ToList(),
                    Action = action,
                    ActionDescription = GetActionDescription(action, currentToken)
                };
                steps.Add(step);

                if (action.Type == ActionType.Shift)
                {
                    // Shift: push token and state
                    symbolStack.Push(currentToken);
                    stateStack.Push(action.Value);
                    inputIndex++;
                }
                else if (action.Type == ActionType.Reduce)
                {
                    Production production = grammar.Productions[action.Value];
                    
                    // Pop symbols and states
                    int popCount = production.RightHandSide.Count;
                    for (int i = 0; i < popCount; i++)
                    {
                        if (symbolStack.Count > 0)
                            symbolStack.Pop();
                        if (stateStack.Count > 0)
                            stateStack.Pop();
                    }
                    
                    // Push LHS
                    symbolStack.Push(production.LeftHandSide);
                    
                    // Get GOTO state
                    int topState = stateStack.Peek();
                    int gotoState = table.GetGoto(topState, production.LeftHandSide);
                    
                    if (gotoState == -1)
                    {
                        step.IsError = true;
                        return steps;
                    }
                    
                    stateStack.Push(gotoState);
                }
                else if (action.Type == ActionType.Accept)
                {
                    step.IsAccepted = true;
                    return steps;
                }
                else
                {
                    step.IsError = true;
                    return steps;
                }
            }
        }

        private string GetActionDescription(ParserAction action, string currentToken)
        {
            return action.Type switch
            {
                ActionType.Shift => $"Shift {action.Value}",
                ActionType.Reduce => $"Reduce by {action.Value}: {grammar.Productions[action.Value]}",
                ActionType.Accept => "Accept",
                ActionType.Error => $"Error on '{currentToken}'",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Tokenizes a simple input string (for demonstration)
        /// </summary>
        public static List<string> Tokenize(string input)
        {
            var tokens = new List<string>();
            input = input.Replace(" ", "");

            int i = 0;
            while (i < input.Length)
            {
                if (input[i] == '=' || input[i] == '*')
                {
                    tokens.Add(input[i].ToString());
                    i++;
                }
                else if (char.IsLetter(input[i]))
                {
                    // Read identifier
                    int start = i;
                    while (i < input.Length && (char.IsLetterOrDigit(input[i]) || input[i] == '_'))
                    {
                        i++;
                    }
                    tokens.Add("id");  // All identifiers become "id"
                }
                else
                {
                    i++;
                }
            }

            return tokens;
        }
    }

    /// <summary>
    /// Represents a single parsing step
    /// </summary>
    public class ParseStep
    {
        public Stack<int> StateStack { get; set; } = new();
        public Stack<string> SymbolStack { get; set; } = new();
        public List<string> RemainingInput { get; set; } = new();
        public ParserAction Action { get; set; } = new(ActionType.Error);
        public string ActionDescription { get; set; } = "";
        public bool IsAccepted { get; set; } = false;
        public bool IsError { get; set; } = false;
    }
}
