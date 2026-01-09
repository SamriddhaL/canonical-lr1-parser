using System;
using System.Collections.Generic;
using System.Linq;

namespace CanonicalLR1Parser
{
    /// <summary>
    /// Computes and manages FIRST sets for all symbols in the grammar
    /// </summary>
    public class FirstSetComputer
    {
        private Grammar grammar;
        private Dictionary<string, HashSet<string>> firstSets;

        public FirstSetComputer(Grammar grammar)
        {
            this.grammar = grammar;
            this.firstSets = new Dictionary<string, HashSet<string>>();
            ComputeFirstSets();
        }

        private void ComputeFirstSets()
        {
            // Initialize FIRST sets
            foreach (var terminal in grammar.Terminals)
            {
                firstSets[terminal] = new HashSet<string> { terminal };
            }

            foreach (var nonTerminal in grammar.NonTerminals)
            {
                firstSets[nonTerminal] = new HashSet<string>();
            }

            // Iterate until no changes occur
            bool changed = true;
            while (changed)
            {
                changed = false;

                foreach (var production in grammar.Productions)
                {
                    string lhs = production.LeftHandSide;
                    var rhs = production.RightHandSide;

                    if (rhs.Count == 0)
                    {
                        // Empty production (epsilon) - not applicable to this grammar
                        continue;
                    }

                    // Add FIRST of first symbol on RHS to FIRST of LHS
                    string firstSymbol = rhs[0];
                    
                    if (firstSets.ContainsKey(firstSymbol))
                    {
                        foreach (var terminal in firstSets[firstSymbol])
                        {
                            if (firstSets[lhs].Add(terminal))
                            {
                                changed = true;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get FIRST set for a single symbol
        /// </summary>
        public HashSet<string> GetFirst(string symbol)
        {
            if (firstSets.ContainsKey(symbol))
            {
                return new HashSet<string>(firstSets[symbol]);
            }
            return new HashSet<string>();
        }

        /// <summary>
        /// Get FIRST set for a sequence of symbols (for lookahead computation)
        /// </summary>
        public HashSet<string> GetFirst(List<string> symbols)
        {
            var result = new HashSet<string>();

            if (symbols.Count == 0)
            {
                return result;
            }

            // For this grammar (no epsilon productions), just return FIRST of first symbol
            string firstSymbol = symbols[0];
            
            if (firstSets.ContainsKey(firstSymbol))
            {
                foreach (var terminal in firstSets[firstSymbol])
                {
                    result.Add(terminal);
                }
            }

            return result;
        }

        public void PrintFirstSets()
        {
            Console.WriteLine("=== FIRST SETS ===");
            
            // Print non-terminals first
            foreach (var nonTerminal in grammar.NonTerminals.OrderBy(x => x))
            {
                Console.WriteLine($"FIRST({nonTerminal}) = {{ {string.Join(", ", firstSets[nonTerminal].OrderBy(x => x))} }}");
            }
            
            Console.WriteLine();
        }

        public Dictionary<string, HashSet<string>> GetAllFirstSets()
        {
            return new Dictionary<string, HashSet<string>>(firstSets);
        }
    }
}
