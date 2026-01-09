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
        /// Parses the input string and prints step-by-step execution
        /// </summary>
        public bool Parse(List<string> inputTokens)
        {
            Console.WriteLine("=== PARSING EXECUTION ===");
            Console.WriteLine($"Input: {string.Join(" ", inputTokens)}");
            Console.WriteLine();

            // Initialize stacks
            var stateStack = new Stack<int>();
            var symbolStack = new Stack<string>();
            
            stateStack.Push(0);  // Initial state
            
            // Add end-of-input marker
            var input = new List<string>(inputTokens);
            input.Add("$");
            
            int inputIndex = 0;
            int step = 0;

            Console.WriteLine($"{"Step",-6} {"State Stack",-25} {"Symbol Stack",-30} {"Input",-20} {"Action",-20}");
            Console.WriteLine(new string('-', 110));

            while (true)
            {
                step++;
                int currentState = stateStack.Peek();
                string currentToken = input[inputIndex];

                // Get action
                var action = table.GetAction(currentState, currentToken);

                // Print current configuration
                string stateStackStr = string.Join(" ", stateStack.Reverse());
                string symbolStackStr = string.Join(" ", symbolStack.Reverse());
                string inputStr = string.Join(" ", input.Skip(inputIndex));
                
                Console.Write($"{step,-6} {stateStackStr,-25} {symbolStackStr,-30} {inputStr,-20} ");

                if (action.Type == ActionType.Shift)
                {
                    Console.WriteLine($"Shift {action.Value}");
                    
                    // Shift: push token and state
                    symbolStack.Push(currentToken);
                    stateStack.Push(action.Value);
                    inputIndex++;
                }
                else if (action.Type == ActionType.Reduce)
                {
                    Production production = grammar.Productions[action.Value];
                    Console.WriteLine($"Reduce by {action.Value}: {production}");
                    
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
                        Console.WriteLine($"\nError: No GOTO entry for state {topState} and symbol {production.LeftHandSide}");
                        return false;
                    }
                    
                    stateStack.Push(gotoState);
                }
                else if (action.Type == ActionType.Accept)
                {
                    Console.WriteLine("Accept");
                    Console.WriteLine();
                    Console.WriteLine("✓ PARSING SUCCESSFUL - INPUT ACCEPTED!");
                    return true;
                }
                else
                {
                    Console.WriteLine("Error");
                    Console.WriteLine();
                    Console.WriteLine("✗ PARSING FAILED - SYNTAX ERROR!");
                    return false;
                }
            }
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
}
