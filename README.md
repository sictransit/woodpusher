# woodpusher
(chess, informal, derogatory) A bad player; an amateur.

A more or less UCI-compatible chess engine. Plug it into your favourite GUI.

## Features
- UCI-compatible
- PGN and FEN parsing
- Perft test
- Opening book
- Zobrist hashing
- Transposition table
- Killer heuristic

## UCI Commands

### `uci`
Initializes the engine and provides engine information.

**Arguments:**
- None

**Example:**
```
uci
id name Woodpusher 1.4.0+d6be4b9c9fa56528613008cd6f05241629906b4f
id author Mikael Fredriksson <micke@sictransit.net>
option name OwnBook type check default true
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


## Win vs. *Stockfish 15* in ultra bullet

To be fair, I've let them play hundreds of games, and this is the only one where Woodpusher won. It's a very rare event and probably due to the phase of the moon or something.
```
[Event "?"]
[Site "?"]
[Date "2024.11.08"]
[Round "?"]
[White "Woodpusher (release)"]
[Black "stockfish_15_x64_avx2"]
[Result "1-0"]
[ECO "B29"]
[GameDuration "00:00:28"]
[GameEndTime "2024-11-08T22:11:39.397 W. Europe Standard Time"]
[GameStartTime "2024-11-08T22:11:10.518 W. Europe Standard Time"]
[Opening "Sicilian"]
[PlyCount "85"]
[TimeControl "40/15"]
[Variation "Nimzovich-Rubinstein Variation"]

1. e4 c5 {-0.26/21 1.8s} 2. Nf3 Nf6 {-0.21/14 0.22s} 3. e5 Nd5 {-0.49/17 0.93s}
4. Nc3 e6 {-0.46/15 0.45s} 5. Nxd5 exd5 {-0.39/16 0.55s} 6. d4
cxd4 {-0.28/13 0.24s} 7. Nxd4 {+0.16/2 0.084s} Qb6 {-0.87/16 1.1s}
8. c3 {-0.83/4 0.35s} Bc5 {-0.59/13 0.34s} 9. b4 {-0.59/4 0.36s}
Bxd4 {-0.16/14 0.47s} 10. Qxd4 {-0.15/4 0.36s} O-O {-0.46/15 0.49s}
11. Bd3 {-0.18/4 0.36s} Re8 {-0.40/13 0.23s} 12. Be3 {-0.12/4 0.37s}
Qe6 {-1.21/16 0.58s} 13. O-O {+0.02/2 0.37s} d6 {-2.82/19 1.2s}
14. Qh4 {+0.02/4 0.37s} h5 {-2.72/17 1.1s} 15. Qxh5 {+0.96/4 0.39s}
g6 {-4.02/16 0.35s} 16. Qg5 {+0.44/4 0.38s} d4 {-2.30/15 0.40s}
17. cxd4 {+1.59/4 0.39s} a6 {-5.62/17 0.84s} 18. Qh6 {+1.84/4 0.39s}
b5 {-4.85/14 0.33s} 19. Be4 {+2.04/4 0.39s} Ra7 {-8.18/16 0.65s}
20. Rfe1 {+2.31/4 0.41s} d5 {-8.19/15 0.53s} 21. Bf3 {+2.06/4 0.41s}
Rb7 {-8.80/14 0.43s} 22. a4 {+2.24/4 0.41s} Rb6 {-8.66/12 0.14s}
23. axb5 {+2.68/4 0.42s} Rxb5 {-8.71/14 0.33s} 24. Rac1 {+1.99/4 0.42s}
Bd7 {-7.12/14 0.26s} 25. Rc7 {+1.92/4 0.42s} Nc6 {-8.30/13 0.21s}
26. Be2 {+1.69/4 0.43s} Rbb8 {-5.54/12 0.096s} 27. Bxa6 {+1.96/4 0.44s}
Nxb4 {-3.52/12 0.14s} 28. Be2 {+1.95/4 0.44s} Rec8 {-8.17/13 0.12s}
29. Rec1 {+1.89/4 0.46s} Rxc7 {-5.15/11 0.051s} 30. Rxc7 {+1.75/4 0.46s}
Nc2 {-4.87/12 0.079s} 31. Rxc2 {+2.03/4 0.47s} Ba4 {-7.82/11 0.061s}
32. Rc1 {+4.46/4 0.48s} Qb6 {-9.05/10 0.048s} 33. Bg4 {+4.76/4 0.49s}
Qb1 {-10.37/9 0.035s} 34. Rxb1 {+6.50/4 0.51s} Rxb1+ {-10.67/9 0.028s}
35. Bc1 {+7.82/6 0.36s} Bb3 {-10.18/8 0.023s} 36. e6 {+9.40/4 0.14s}
Rxc1+ {-10.76/9 0.022s} 37. Qxc1 {+11.64/6 0.41s} f5 {-11.16/9 0.020s}
38. Bf3 {+12.36/4 0.27s} Bc2 {-24.82/9 0.020s} 39. Qxc2 {+16.07/6 0.54s}
Kf8 {-M12/16 0.017s} 40. Qc7 {+16.99/6 0.20s} Kg8 {-M6/81 0.011s}
41. Qf7+ {+M5/6 0.22s} Kh8 {-M4/245 0.004s} 42. e7 {+M3/4 0.022s}
g5 {-M2/245 0.003s} 43. e8=Q# {+M1/2 0.010s, White mates} 1-0
```