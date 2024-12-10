# woodpusher
(chess, informal, derogatory) A bad player; an amateur.

A more or less UCI-compatible chess engine. Plug it into your favourite GUI.

## Features
- UCI-compatible
- PGN and FEN parsing
- Perft test
- Opening book
- Zobrist hashing
- Negamax search with alpha-beta pruning
	- NegaScout (or PVS?) 
	- Quiescence 
	- Iterative deepening 
	- Null move pruning
    - Move ordering 
    	- MVV/LVA, history heuristic, killer heuristic, transposition table

## UCI Commands

### `uci`
Initializes the engine and provides engine information.

**Arguments:**
- None

**Example:**
```
uci
id name Woodpusher 1.5.0+e1061610be8d96db24890a658c1b57373402d6de
id author Mikael Fredriksson <micke@sictransit.net>
option name OwnBook type check default true
option name Ponder type check default false
uciok
```

### `setoption`

Sets an engine option.

**Arguments:**
- `name <option_name>`: The name of the option.
- `value <option_value>`: The value of the option.

**Example:**
```
setoption name OwnBook value false
```

*The engine will ignore the `ponder` option. It is not implemented fully implemented yet.*

### `isready`
Checks if the engine is ready.

**Arguments:**
- None

### `ucinewgame`
Initializes a new game.

**Arguments:**
- None

### `position`
Sets up the board position using FEN and optional moves.

**Arguments:**
- `fen <fen_string>`: Sets the board position using the provided FEN string.
- `moves <move1> <move2> ...`: A list of moves in algebraic notation to be played from the given position or the starting position if no FEN is provided.

**Example:**

`position fen rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1 moves e2e4 e7e5`

### `go`
Starts the engine to find the best move or perform a perft test.

**Arguments:**
- `wtime <milliseconds>`: White's remaining time in milliseconds.
- `btime <milliseconds>`: Black's remaining time in milliseconds.
- `movestogo <number>`: Number of moves to the next time control.
- `movetime <milliseconds>`: Time to search in milliseconds.
- `perft <depth>`: Perform a perft test to the given depth.

**Example:**

```
go wtime 300000 btime 300000 movestogo 40
info string debug playing opening book g1f3
bestmove g1f3
```

### `stop`
Stops the engine's current calculation.

**Arguments:**
- None

### `d`
Displays the current board position.

**Arguments:**
- None

**Example:**
```
d
8 r n b q k b n r
7 p p p p · p p p
6 · · · · · · · ·
5 · · · · p · · ·
4 · · · · P · · ·
3 · · · · · · · ·
2 P P P P · P P P
1 R N B Q K B N R
  a b c d e f g h
Hash: 6805972066694293741
FEN: rnbqkbnr/pppp1ppp/8/4p3/4P3/8/PPPP1PPP/RNBQKBNR w KQkq e6 0 2
```

### `quit`
Exits the engine.

**Arguments:**
- None

## Strength of play (ELO)

This is the result of a small tournament vs Stockfish 15 locked to ELO 2500. The current version is stronger than the previous. **On my machine**, it even a beats Stockfish 15, but it's not a fair comparison since Stockfish seems to blunder to lower its strength.

```
Rank Name                          Elo     +/-   Games   Score    Draw 
   1 Woodpusher (1.5.0)             89     131      20   62.5%   35.0% 
   2 stockfish-windows-x86-64-avx2  35     145      20   55.0%   20.0% 
   3 Woodpusher (1.4.2)           -127     149      20   32.5%   25.0% 

SPRT: llr 0 (0.0%), lbound -inf, ubound inf
30 of 30 games finished.

Player: stockfish-windows-x86-64-avx2
   "Draw by 3-fold repetition": 3
   "Draw by insufficient mating material": 1
   "Loss: Black mates": 1
   "Loss: White mates": 6
   "Win: Black mates": 4
   "Win: White mates": 5
Player: Woodpusher (1.5.0)
   "Draw by 3-fold repetition": 5
   "Draw by adjudication: user decision": 1 (both sides had rook + king)
   "Draw by insufficient mating material": 1
   "Loss: Black mates": 2
   "Loss: White mates": 2
   "Win: Black mates": 3
   "Win: White mates": 6
Player: Woodpusher (1.4.2)
   "Draw by 3-fold repetition": 4
   "Draw by adjudication: user decision": 1 (both sides had rook + king)
   "Loss: Black mates": 6
   "Loss: White mates": 5
   "Win: Black mates": 2
   "Win: White mates": 2
```

