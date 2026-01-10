using System;
using System.Collections.Generic;
using System.Linq;

namespace CanonicalLR1Parser
{
    /// <summary>
    /// Represents a production rule in the grammar: LHS → RHS
    /// </summary>
    public class Production
    {
        public string LeftHandSide { get; }
        public List<string> RightHandSide { get; }

        public Production(string lhs, params string[] rhs)
        {
            LeftHandSide = lhs;
            RightHandSide = new List<string>(rhs);
        }

        public override string ToString()
        {
            return $"{LeftHandSide} → {string.Join(" ", RightHandSide)}";
        }

        public override bool Equals(object obj)
        {
            if (obj is Production other)
            {
                return LeftHandSide == other.LeftHandSide &&
                       RightHandSide.SequenceEqual(other.RightHandSide);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = LeftHandSide.GetHashCode();
            foreach (var symbol in RightHandSide)
            {
                hash = hash * 31 + symbol.GetHashCode();
            }
            return hash;
        }
    }
}
