# Canonical LR(1) Parser Implementation in C#

## Overview
This project implements a **Canonical LR(1) parser** from scratch in C# to demonstrate that a specific grammar is conflict-free and can successfully parse pointer-assignment expressions.

## Grammar
The hardcoded grammar represents pointer operations in C-like languages:

```
S' → S
S  → L = R
S  → R
L  → * R    (dereference operator - UNARY PREFIX)
L  → id
R  → L
```

**Terminals:** `id`, `*`, `=`, `$`  
**Non-terminals:** `S'`, `S`, `L`, `R`  
**Start symbol:** `S'`

### Important Note
In this grammar, `*` is a **unary prefix operator** (dereference), NOT an infix multiplication operator. This means:
- ✓ **Valid:** `id = * id` (assign dereference-of-id to id)
- ✓ **Valid:** `* * id` (double dereference)
- ✗ **Invalid:** `id = id * id` (would require infix multiplication)

## Project Structure

### Core Modules

1. **Production.cs** - Represents grammar production rules (LHS → RHS)
2. **Grammar.cs** - Hardcoded grammar with terminals, non-terminals, and productions
3. **FirstSetComputer.cs** - Computes FIRST sets for all symbols
4. **LR1Item.cs** - Defines LR(1) items `[A → α • β, lookahead]` and states
5. **LR1Operations.cs** - Implements Closure and GOTO operations
6. **CanonicalCollection.cs** - Builds the canonical collection of LR(1) states (DFA)
7. **ParsingTable.cs** - Constructs ACTION and GOTO tables, detects conflicts
8. **LR1Parser.cs** - Stack-based LR(1) parsing engine with step-by-step execution
9. **Program.cs** - Main demonstration program

## Features

**Complete Canonical LR(1) Implementation**
- FIRST set computation
- LR(1) item construction with lookahead propagation
- Closure and GOTO functions
- Canonical state collection (14 states)
- ACTION/GOTO parsing table construction
- Conflict detection (no conflicts found!)

**Detailed Output**
- Grammar display
- FIRST sets
- All LR(1) states with items
- State transitions (DFA)
- Parsing tables
- Step-by-step parsing trace
- Clear explanations of grammar semantics

**Demonstration**
- Parses valid strings: `id = * id`, `* * id`
- Explains why `id = id * id` is invalid
- Proves the grammar is Canonical LR(1) conflict-free

## How to Run

### Prerequisites
- .NET 8.0 SDK or later

### Build and Run
```bash
cd "c:\Personal\Project\Compiler"
dotnet build
dotnet run
```

## Sample Output

```
Step   State Stack               Symbol Stack                   Input                Action
--------------------------------------------------------------------------------------------------------------
1      0                                                        id = * id $          Shift 5
2      0 5                       id                             = * id $             Reduce by 4: L → id
3      0 2                       L                              = * id $             Shift 6
4      0 2 6                     L =                            * id $               Shift 11
5      0 2 6 11                  L = *                          id $                 Shift 12
6      0 2 6 11 12               L = * id                       $                    Reduce by 4: L → id
7      0 2 6 11 10               L = * L                        $                    Reduce by 5: R → L
8      0 2 6 11 13               L = * R                        $                    Reduce by 3: L → * R
9      0 2 6 10                  L = L                          $                    Reduce by 5: R → L
10     0 2 6 9                   L = R                          $                    Reduce by 1: S → L = R
11     0 1                       S                              $                    Accept

✓ PARSING SUCCESSFUL - INPUT ACCEPTED!
```

## Results

- **Total States:** 14
- **Conflicts:** None (Grammar IS Canonical LR(1))
- **Test 1:** `id = * id` → ✓ ACCEPTED
- **Test 2:** `* * id` → ✓ ACCEPTED

## Educational Value

This implementation demonstrates:
1. How to build a complete LR(1) parser from first principles
2. The distinction between Canonical LR(1) and other parsing techniques
3. How lookahead symbols prevent conflicts
4. The importance of grammar semantics (unary vs. binary operators)
5. Step-by-step parsing with state machines

## Key Insights

- The Canonical LR(1) parser uses **lookahead** to make parsing decisions
- Each state contains items with specific lookaheads (more states than SLR/LALR)
- The grammar is **conflict-free** under Canonical LR(1)
- The `*` operator's semantics (unary prefix) affect what strings are valid
- Proper implementation requires careful handling of FIRST sets and closure operations

