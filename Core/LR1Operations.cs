using System;
using System.Collections.Generic;
using System.Linq;

namespace CanonicalLR1Parser
{
    /// <summary>
    /// Implements Closure and GOTO operations for Canonical LR(1) parsing
    /// </summary>
    public class LR1Operations
    {
        private Grammar grammar;
        private FirstSetComputer firstComputer;

        public LR1Operations(Grammar grammar, FirstSetComputer firstComputer)
        {
            this.grammar = grammar;
            this.firstComputer = firstComputer;
        }

        /// <summary>
        /// Computes the closure of a set of LR(1) items
        /// </summary>
        public HashSet<LR1Item> Closure(HashSet<LR1Item> items)
        {
            var closure = new HashSet<LR1Item>(items);
            var workQueue = new Queue<LR1Item>(items);

            while (workQueue.Count > 0)
            {
                var item = workQueue.Dequeue();
                string symbolAfterDot = item.GetSymbolAfterDot();

                // If dot is at the end or symbol after dot is a terminal, skip
                if (symbolAfterDot == null || grammar.IsTerminal(symbolAfterDot))
                {
                    continue;
                }

                // Symbol after dot is a non-terminal
                // For each production B → γ, add [B → • γ, b] for each b in FIRST(βa)
                // where the item is [A → α • B β, a]
                
                var symbolsAfter = item.GetSymbolsAfterDotSymbol();
                var lookaheadSequence = new List<string>(symbolsAfter);
                lookaheadSequence.Add(item.Lookahead);

                // Compute FIRST of the sequence (β followed by lookahead)
                HashSet<string> lookaheads = ComputeFirstOfSequence(lookaheadSequence);

                // Get all productions for the non-terminal
                var productions = grammar.GetProductionsFor(symbolAfterDot);

                foreach (var production in productions)
                {
                    foreach (var lookahead in lookaheads)
                    {
                        var newItem = new LR1Item(production, 0, lookahead);
                        if (closure.Add(newItem))
                        {
                            workQueue.Enqueue(newItem);
                        }
                    }
                }
            }

            return closure;
        }

        /// <summary>
        /// Computes FIRST of a sequence of symbols followed by a terminal lookahead
        /// For this grammar without epsilon productions, if the sequence is empty,
        /// we return the set containing just the final lookahead terminal.
        /// Otherwise, we return FIRST of the first symbol.
        /// </summary>
        private HashSet<string> ComputeFirstOfSequence(List<string> symbols)
        {
            var result = new HashSet<string>();

            if (symbols.Count == 0)
            {
                // This shouldn't happen in our usage, but return empty set
                return result;
            }

            // Check if the last symbol is a terminal (the lookahead)
            string lastSymbol = symbols[symbols.Count - 1];
            
            // If we only have the lookahead symbol
            if (symbols.Count == 1)
            {
                if (grammar.IsTerminal(lastSymbol))
                {
                    result.Add(lastSymbol);
                }
                else if (grammar.IsNonTerminal(lastSymbol))
                {
                    var firstSet = firstComputer.GetFirst(lastSymbol);
                    foreach (var terminal in firstSet)
                    {
                        result.Add(terminal);
                    }
                }
                return result;
            }

            // For sequences with more than one symbol
            // In a grammar without epsilon, FIRST(βa) = FIRST(β) if β is not empty
            // Otherwise FIRST(a)
            string firstSymbol = symbols[0];
            
            if (grammar.IsTerminal(firstSymbol))
            {
                result.Add(firstSymbol);
            }
            else if (grammar.IsNonTerminal(firstSymbol))
            {
                var firstSet = firstComputer.GetFirst(firstSymbol);
                foreach (var terminal in firstSet)
                {
                    result.Add(terminal);
                }
            }

            return result;
        }

        /// <summary>
        /// Computes GOTO(I, X) where I is a set of items and X is a symbol
        /// </summary>
        public HashSet<LR1Item> Goto(HashSet<LR1Item> items, string symbol)
        {
            var gotoItems = new HashSet<LR1Item>();

            // Find all items where the dot is before the given symbol
            foreach (var item in items)
            {
                string symbolAfterDot = item.GetSymbolAfterDot();
                if (symbolAfterDot == symbol)
                {
                    // Advance the dot and add to goto set
                    gotoItems.Add(item.AdvanceDot());
                }
            }

            // Return the closure of the goto set
            if (gotoItems.Count > 0)
            {
                return Closure(gotoItems);
            }

            return gotoItems;
        }

        /// <summary>
        /// Gets all symbols that can be shifted from a given state
        /// </summary>
        public HashSet<string> GetShiftableSymbols(HashSet<LR1Item> items)
        {
            var symbols = new HashSet<string>();
            foreach (var item in items)
            {
                string symbol = item.GetSymbolAfterDot();
                if (symbol != null)
                {
                    symbols.Add(symbol);
                }
            }
            return symbols;
        }
    }
}
