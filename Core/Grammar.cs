using System;
using System.Collections.Generic;
using System.Linq;

namespace CanonicalLR1Parser
{
    /// <summary>
    /// Represents the grammar with productions, terminals, non-terminals, and start symbol
    /// </summary>
    public class Grammar
    {
        public List<Production> Productions { get; }
        public HashSet<string> Terminals { get; }
        public HashSet<string> NonTerminals { get; }
        public string StartSymbol { get; }
        public string AugmentedStart { get; }

        public Grammar()
        {
            Productions = new List<Production>();
            Terminals = new HashSet<string>();
            NonTerminals = new HashSet<string>();
            
            // Hardcoded grammar:
            // S' → S
            // S  → L = R
            // S  → R
            // L  → * R
            // L  → id
            // R  → L
            
            AugmentedStart = "S'";
            StartSymbol = "S";
            
            // Define terminals
            Terminals.Add("id");
            Terminals.Add("*");
            Terminals.Add("=");
            Terminals.Add("$");  // End-of-input marker
            
            // Define non-terminals
            NonTerminals.Add("S'");
            NonTerminals.Add("S");
            NonTerminals.Add("L");
            NonTerminals.Add("R");
            
            // Define productions
            Productions.Add(new Production("S'", "S"));        // Production 0
            Productions.Add(new Production("S", "L", "=", "R")); // Production 1
            Productions.Add(new Production("S", "R"));         // Production 2
            Productions.Add(new Production("L", "*", "R"));    // Production 3
            Productions.Add(new Production("L", "id"));        // Production 4
            Productions.Add(new Production("R", "L"));         // Production 5
        }

        public bool IsTerminal(string symbol)
        {
            return Terminals.Contains(symbol);
        }

        public bool IsNonTerminal(string symbol)
        {
            return NonTerminals.Contains(symbol);
        }

        public List<Production> GetProductionsFor(string nonTerminal)
        {
            return Productions.Where(p => p.LeftHandSide == nonTerminal).ToList();
        }

        public int GetProductionIndex(Production production)
        {
            return Productions.IndexOf(production);
        }

        public void PrintGrammar()
        {
            Console.WriteLine("=== GRAMMAR ===");
            for (int i = 0; i < Productions.Count; i++)
            {
                Console.WriteLine($"{i}: {Productions[i]}");
            }
            Console.WriteLine();
        }
    }
}
