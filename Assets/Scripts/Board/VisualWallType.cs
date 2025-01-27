using System;

namespace Board
{
    [Flags]
    public enum VisualWallType
    {
        Empty = 0,
        Up = 1,
        Right = 2
    }
    
    [Flags]
    public enum WallType
    {
        Empty = 0,
        Down = 2,
        Left = 4,
        Up = 8,
        Right = 16
    }
    
    [Serializable]
    public struct BoardPosition
    {
        public int column;
        public int row;

        public BoardPosition(int column, int row)
        {
            this.column = column;
            this.row = row;
        }

        public bool IsEqual(BoardPosition otherPosition)
        {
            return column == otherPosition.column && row == otherPosition.row;
        }
        
        public static bool IsEqual(BoardPosition firstPosition, BoardPosition secondPosition)
        {
            return firstPosition.column == secondPosition.column && firstPosition.row == secondPosition.row;
        }
    }
}