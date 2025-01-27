using System;
using System.Collections.Generic;
using Random = System.Random;

namespace Board
{
    public class QTable
    {
        private readonly double _discountFactor;
        private readonly double _learningRate;
        private readonly float _epsilon;
        
        private int[,] _rewards;
        private double[,,] _qValues;
        
        private readonly int _columns;
        private readonly int _rows;
        private readonly BoardPosition _goalPosition;
        
        private readonly string[] _actions = { "up", "right", "down", "left" };
        
        // rewards
        private readonly int _goalReward;
        private readonly int _pathReward;

        private readonly WallType[] _wallType;
        
        public QTable(int columns, int rows, BoardPosition goalPosition, WallType[] wallType)
        {
            // Initial values
            _columns = columns;
            _rows = rows;
            _wallType = wallType;
            _goalPosition = goalPosition;
            
            // Reward values
            _goalReward = 10;
            _pathReward = -1;
            
            // others
            _epsilon = 0.9f;
            _discountFactor = 0.9f;
            _learningRate = 0.9f;
            
            // Set values
            Reset();
        }

        private void Reset()
        {
            _rewards = new int[_columns, _rows];
            _qValues = new double[_columns, _rows, _actions.Length];
            
            // default values
            for (int column = 0; column < _columns; ++column)
            {
                for (int row = 0; row < _rows; ++row)
                {
                    _rewards[column, row] = _pathReward;
                }
            }
            
            // set goal
            _rewards[_goalPosition.column, _goalPosition.row] = _goalReward;
        }

        public bool IsTerminalState(BoardPosition boardPosition)
        {
            return _rewards[boardPosition.column, boardPosition.row] != _pathReward;
        }

        private BoardPosition RandomPosition()
        {
            int column = new Random().Next(_columns);
            int row = new Random().Next(_rows);
            return new BoardPosition(column, row);
        }
        
        /// <summary>
        /// Get a starting location.
        /// </summary>
        /// <returns>non-terminal state</returns>
        public BoardPosition GetStartingLocation()
        {
            BoardPosition currentPosition = RandomPosition();
            while (IsTerminalState(currentPosition))
            {
                currentPosition = RandomPosition();
            }

            return currentPosition;
        }

        private int GetNextAction(BoardPosition boardPosition, float epsilon)
        {
            return new Random().NextDouble() < epsilon 
                ? GetMaxQValueIndex(boardPosition) 
                : new Random().Next(_actions.Length);
        }
        
        public int GetNextActionWithoutEpsilon(BoardPosition boardPosition)
        {
            return new Random().NextDouble() < _epsilon 
                ? GetMaxQValueIndex(boardPosition) 
                : new Random().Next(_actions.Length);
        }

        
        private int GetMaxQValueIndex(BoardPosition boardPosition)
        {
            double maxQValue = double.MinValue;
            int maxQValueIndex = -1;

            for (int i = 0; i < _actions.Length; i++)
            {
                double qValue = _qValues[boardPosition.column, boardPosition.row, i];
                if (qValue > maxQValue)
                {
                    maxQValue = qValue;
                    maxQValueIndex = i;
                }
            }

            return maxQValueIndex;
        }
        
        private double GetMaxQValue(BoardPosition boardPosition)
        {
            double maxQValue = double.MinValue;
            foreach (var action in _actions)
            {
                int actionIndex = Array.IndexOf(_actions, action);
                double qValue = _qValues[boardPosition.column, boardPosition.row, actionIndex];
                if (qValue > maxQValue)
                    maxQValue = qValue;
            }
            return maxQValue;
        }
        
        // function that will get the next location based on the chosen action
        // check walls
        private BoardPosition GetNextLocation(BoardPosition currentBoardPosition, int actionIndex)
        {
            BoardPosition newBoardPosition = currentBoardPosition;
            WallType wallType = _wallType[currentBoardPosition.column * _rows + currentBoardPosition.row];
            
            if (_actions[actionIndex] == "up" && currentBoardPosition.row < _rows - 1 && (wallType & WallType.Up) == 0)
                newBoardPosition.row += 1;
            else if (_actions[actionIndex] == "right" && currentBoardPosition.column < _columns - 1 && (wallType & WallType.Right) == 0)
                newBoardPosition.column += 1;
            else if (_actions[actionIndex] == "down" && currentBoardPosition.row > 0 && (wallType & WallType.Down) == 0)
                newBoardPosition.row -= 1;
            else if (_actions[actionIndex] == "left" && currentBoardPosition.column > 0 && (wallType & WallType.Left) == 0)
                newBoardPosition.column -= 1;
            return newBoardPosition;
        }

        public List<BoardPosition> GetShortestPath(BoardPosition startBoardPosition)
        {
            if (IsTerminalState(startBoardPosition))
                return new List<BoardPosition>();

            List<BoardPosition> shortestPath = new List<BoardPosition> { startBoardPosition };
            BoardPosition currentBoardPosition = startBoardPosition;

            int threshold = 100;            
            while (!IsTerminalState(currentBoardPosition))
            {
                int actionIndex = GetNextAction(currentBoardPosition, 1.0f);
                BoardPosition newBoardPosition = GetNextLocation(currentBoardPosition, actionIndex);
                shortestPath.Add(newBoardPosition);
                currentBoardPosition = newBoardPosition;
                
                if(--threshold < 0) break;
            }
            return shortestPath;
        }

        public void UpdateQTable(ref BoardPosition boardPosition)
        {
            int actionIndex = GetNextAction(boardPosition, _epsilon);
            BoardPosition oldBoardPosition = boardPosition;
            boardPosition = GetNextLocation(boardPosition, actionIndex);

            int reward = _rewards[boardPosition.column, boardPosition.row];
            double oldQValue = _qValues[oldBoardPosition.column, oldBoardPosition.row, actionIndex];
            double temporalDifference = reward + (_discountFactor * GetMaxQValue(boardPosition)) - oldQValue;
            double newQValue = oldQValue + (_learningRate * temporalDifference);
            _qValues[oldBoardPosition.column, oldBoardPosition.row, actionIndex] = newQValue;
        }
        
        public void UpdateSARSATable(ref BoardPosition boardPosition, ref int actionIndex)
        {
            BoardPosition oldBoardPosition = boardPosition;
            boardPosition = GetNextLocation(boardPosition, actionIndex);
            
            int reward = _rewards[boardPosition.column, boardPosition.row];
            int newActionIndex = GetNextAction(boardPosition, _epsilon);

            double oldQValue = _qValues[oldBoardPosition.column, oldBoardPosition.row, actionIndex];
            double newQValue = _qValues[boardPosition.column, boardPosition.row, actionIndex];
            
            double temporalDifference = reward + (_discountFactor * newQValue) - oldQValue;
            double updatedQValue = oldQValue + (_learningRate * temporalDifference);
            _qValues[oldBoardPosition.column, oldBoardPosition.row, actionIndex] = updatedQValue;
            
            actionIndex = newActionIndex;
        }
    }
}