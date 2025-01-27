using System.Collections.Generic;

namespace Board
{
    // Class with references of BoardLoader
    // Getters Only
    public sealed class BoardInfo
    {
        public readonly byte ColumnsInt;
        public readonly byte RowsInt;
        public readonly ushort Capacity;
        public readonly List<Square> Squares;
        public readonly List<Square> Walls;
        public readonly BoardPosition StartPosition;
        public readonly BoardPosition GoalPosition;
        public readonly WallType[] WallsType;

        // Constructor
        public BoardInfo(BoardLoader boardLoader)
        {
            ColumnsInt = boardLoader.ColumnsInt;
            RowsInt = boardLoader.RowsInt;
            Capacity = boardLoader.Capacity;
            Squares = boardLoader.Square;
            Walls = boardLoader.Walls;
            WallsType = boardLoader.WallsType;
            
            StartPosition = boardLoader.StartPosition;
            GoalPosition = boardLoader.GoalPosition;
        }
        
        public Square GetSquare(int index) => Squares[index];

        public Square GetStartPositionSquare()
        {
            var position = StartPosition.column * RowsInt + StartPosition.row;
            return Squares[position];
        }

        public int GetIndexOfPosition(BoardPosition boardPosition)
        {
            return boardPosition.column * RowsInt + boardPosition.row;
        }
    }
}