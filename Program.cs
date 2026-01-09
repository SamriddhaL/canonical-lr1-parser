using System;
using System.Collections.Generic;

namespace CanonicalLR1Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         CANONICAL LR(1) PARSER DEMONSTRATION                       ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            // Step 1: Initialize Grammar
            Console.WriteLine("Step 1: Initializing Grammar...");
            var grammar = new Grammar();
            grammar.PrintGrammar();

            // Step 2: Compute FIRST Sets
            Console.WriteLine("Step 2: Computing FIRST sets...");
            var firstComputer = new FirstSetComputer(grammar);
            firstComputer.PrintFirstSets();

            // Step 3: Build LR(1) Items and Operations
            Console.WriteLine("Step 3: Initializing LR(1) operations...");
            var operations = new LR1Operations(grammar, firstComputer);
            Console.WriteLine("✓ LR(1) operations ready");
            Console.WriteLine();

            // Step 4: Build Canonical Collection
            Console.WriteLine("Step 4: Building Canonical LR(1) State Collection...");
            var collection = new CanonicalCollection(grammar, operations);
            collection.Build();
            Console.WriteLine($"✓ Built {collection.States.Count} states");
            Console.WriteLine();
            
            collection.PrintStates();
            collection.PrintTransitions();

            // Step 5: Construct Parsing Tables
            Console.WriteLine("Step 5: Constructing Parsing Tables...");
            var parsingTable = new ParsingTable(grammar, collection);
            parsingTable.ConstructTables();
            Console.WriteLine("✓ Parsing tables constructed");
            Console.WriteLine();
            
            parsingTable.PrintTables();
            parsingTable.ReportConflicts();

            // Step 6: Parse test strings
            Console.WriteLine("Step 6: Parsing test strings...");
            Console.WriteLine();
            
            var parser = new LR1Parser(grammar, parsingTable);
            
            // Test with a VALID string
            var validString = "id = * id";
            Console.WriteLine($"══════════════════════════════════════════════════════════════════");
            Console.WriteLine($"Test 1: Parsing VALID string '{validString}'");
            Console.WriteLine($"══════════════════════════════════════════════════════════════════");
            var tokens1 = LR1Parser.Tokenize(validString);
            bool result1 = parser.Parse(tokens1);
            Console.WriteLine();
            
            // Test with another valid string
            var validString2 = "* * id";
            Console.WriteLine($"══════════════════════════════════════════════════════════════════");
            Console.WriteLine($"Test 2: Parsing VALID string '{validString2}'");
            Console.WriteLine($"══════════════════════════════════════════════════════════════════");
            var tokens2 = LR1Parser.Tokenize(validString2);
            bool result2 = parser.Parse(tokens2);
            Console.WriteLine();
            
            // Explain why "id = id * id" is INVALID
            Console.WriteLine("══════════════════════════════════════════════════════════════════");
            Console.WriteLine("IMPORTANT GRAMMAR NOTE: Why 'id = id * id' is REJECTED");
            Console.WriteLine("══════════════════════════════════════════════════════════════════");
            Console.WriteLine("In this grammar, '*' is a UNARY PREFIX operator (dereference),");
            Console.WriteLine("NOT an infix multiplication operator.");
            Console.WriteLine();
            Console.WriteLine("Grammar productions:");
            Console.WriteLine("  L → * R    (L can be: dereference of R)");
            Console.WriteLine("  L → id     (L can be: an identifier)");
            Console.WriteLine();
            Console.WriteLine("Valid strings:");
            Console.WriteLine("  ✓ 'id = * id'    (id equals dereference-of-id)");
            Console.WriteLine("  ✓ '* id = id'    (dereference-of-id equals id)");
            Console.WriteLine("  ✓ 'id = * * id'  (id equals double-dereference-of-id)");
            Console.WriteLine();
            Console.WriteLine("Invalid string:");
            Console.WriteLine("  ✗ 'id = id * id' (CANNOT be derived - would need infix *)");
            Console.WriteLine();
            Console.WriteLine("This grammar represents pointer operations in C-like languages,");
            Console.WriteLine("where expressions like 'x = *ptr' are valid, but 'x = y * z' are not.");
            Console.WriteLine("══════════════════════════════════════════════════════════════════");
            Console.WriteLine();
            
            Console.WriteLine();
            Console.WriteLine("╔════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                         FINAL RESULTS                              ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine($"Grammar: S' → S | S → L=R | S → R | L → *R | L → id | R → L");
            Console.WriteLine($"Test String 1: {validString} → {(result1 ? "✓ ACCEPTED" : "✗ REJECTED")}");
            Console.WriteLine($"Test String 2: {validString2} → {(result2 ? "✓ ACCEPTED" : "✗ REJECTED")}");
            Console.WriteLine($"Total States: {collection.States.Count}");
            Console.WriteLine($"Conflicts: {(parsingTable.HasConflicts ? "YES (Grammar is NOT LR(1))" : "NO (Grammar IS LR(1))")}");
            Console.WriteLine();
            
            if (!parsingTable.HasConflicts && result1)
            {
                Console.WriteLine("═══════════════════════════════════════════════════════════════════");
                Console.WriteLine("  DEMONSTRATION SUCCESSFUL!");
                Console.WriteLine("  The grammar is Canonical LR(1) conflict-free");
                Console.WriteLine("  and successfully parses valid pointer-assignment strings");
                Console.WriteLine("═══════════════════════════════════════════════════════════════════");
            }
            
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
