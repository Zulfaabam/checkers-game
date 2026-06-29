```code
// for testing

Cell[0, 1] = new Cell(
              new Position(0, 1),
              new Piece(PieceType.Man, players.First(p => !p.IsPlayerOne).Color));

Cell[1, 0] = new Cell(
          new Position(1, 0),
          new Piece(PieceType.Man, players.First(p => p.IsPlayerOne).Color));
Cell[2, 3] = new Cell(
          new Position(2, 3),
          new Piece(PieceType.Man, players.First(p => p.IsPlayerOne).Color));
Cell[2, 1] = new Cell(
          new Position(2, 1),
          new Piece(PieceType.Man, players.First(p => p.IsPlayerOne).Color));
```
