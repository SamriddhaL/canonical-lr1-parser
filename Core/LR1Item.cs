using System;
using System.Collections.Generic;
using System.Linq;

namespace CanonicalLR1Parser
{
    /// <summary>
    /// Represents an LR(1) item: [A → α • β, lookahead]
    /// </summary>
    public class LR1Item
    {
        public Production Production { get; }
        public int DotPosition { get; }
        public string Lookahead { get; }

        public LR1Item(Production production, int dotPosition, string lookahead)
        {
            Production = production;
            DotPosition = dotPosition;
            Lookahead = lookahead;
        }

        /// <summary>
        /// Returns the symbol immediately after the dot, or null if dot is at the end
        /// </summary>
        public string GetSymbolAfterDot()
        {
            if (DotPosition < Production.RightHandSide.Count)
            {
                return Production.RightHandSide[DotPosition];
            }
            return null;
        }

        /// <summary>
        /// Returns symbols after the symbol immediately following the dot
        /// </summary>
        public List<string> GetSymbolsAfterDotSymbol()
        {
            var result = new List<string>();
            if (DotPosition + 1 < Production.RightHandSide.Count)
            {
                for (int i = DotPosition + 1; i < Production.RightHandSide.Count; i++)
                {
                    result.Add(Production.RightHandSide[i]);
                }
            }
            return result;
        }

        /// <summary>
        /// Creates a new item with the dot advanced by one position
        /// </summary>
        public LR1Item AdvanceDot()
        {
            return new LR1Item(Production, DotPosition + 1, Lookahead);
        }

        /// <summary>
        /// Checks if this is a complete item (dot at the end)
        /// </summary>
        public bool IsComplete()
        {
            return DotPosition >= Production.RightHandSide.Count;
        }

        public override string ToString()
        {
            var symbols = new List<string>(Production.RightHandSide);
            symbols.Insert(DotPosition, "•");
            return $"[{Production.LeftHandSide} → {string.Join(" ", symbols)}, {Lookahead}]";
        }

        public override bool Equals(object obj)
        {
            if (obj is LR1Item other)
            {
                return Production.Equals(other.Production) &&
                       DotPosition == other.DotPosition &&
                       Lookahead == other.Lookahead;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Production, DotPosition, Lookahead);
        }
    }

    /// <summary>
    /// Represents a set of LR(1) items (a state in the LR(1) automaton)
    /// </summary>
    public class LR1State
    {
        public int StateId { get; set; }
        public HashSet<LR1Item> Items { get; }

        public LR1State()
        {
            Items = new HashSet<LR1Item>();
        }

        public LR1State(HashSet<LR1Item> items)
        {
            Items = new HashSet<LR1Item>(items);
        }

        public void AddItem(LR1Item item)
        {
            Items.Add(item);
        }

        public override bool Equals(object obj)
        {
            if (obj is LR1State other)
            {
                return Items.SetEquals(other.Items);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            foreach (var item in Items.OrderBy(i => i.ToString()))
            {
                hash ^= item.GetHashCode();
            }
            return hash;
        }

        public override string ToString()
        {
            return $"State {StateId}:\n" + string.Join("\n", Items.Select(i => "  " + i.ToString()));
        }
    }
}
