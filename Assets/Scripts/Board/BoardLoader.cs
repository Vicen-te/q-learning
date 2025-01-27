using System;
using File = System.IO.File;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Board
{
    public class BoardLoader : MonoBehaviour
    {
        [Header("Board")]
        [SerializeField, Range(0, 10)] private byte columnsInt = 7;
        [SerializeField, Range(0, 10)] private byte rowsInt = 6;
        [Tooltip("Space between discs"), SerializeField, Range(0, 1)] private float spaceAddition = 0.25f;

        [SerializeField] private VisualWallType[] visualWallsType;
        [SerializeField] private WallType[] wallsType;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject squarePrefab;
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private GameObject backGroundPrefab;
        
        // Space radius in Unity units
        private float _spaceRadius;

        // Read txt files.
        [SerializeField] private bool readFile;
        private bool _visuals;
        private bool _reward;
        
        // Goal
        [Header("Position")]
        [SerializeField] private BoardPosition goalPosition = new BoardPosition(3,3);
        [SerializeField] private BoardPosition startPosition = new BoardPosition(0,0);
        [SerializeField] private Color goalColor = new Color(0.25f, 0.25f, 0.25f); 

        // Getters
        public ushort Capacity { get; private set; } //< columns * rows => 7 * 6 = 42
        public byte ColumnsInt => columnsInt;
        public byte RowsInt => rowsInt;
        public WallType[] WallsType => wallsType;
        public BoardPosition StartPosition => startPosition;
        public BoardPosition GoalPosition => goalPosition;
        public List<Square> Square { get; private set; }
        public List<Square> Walls  { get; private set; }


        private void CheckVariable(string name, string value)
        {
            switch (name)
            {
                case "columns":
                    columnsInt = (byte)Convert.ChangeType(value.Replace(" ", ""), typeof(byte));
                    Debug.Log($"columnsInt: {columnsInt}");
                    break;
                case "rows":
                    rowsInt = (byte)Convert.ChangeType(value.Replace(" ", ""), typeof(byte));
                    Debug.Log($"rowsInt: {rowsInt}");
                    break;
                case "goal":
                    goalPosition = SetPosition(value);
                    Debug.Log($"goal.column: {goalPosition.column}, goal.row: {goalPosition.row}");
                    break;
                case "start":
                    startPosition = SetPosition(value);
                    // TODO: spawn AI
                    Debug.Log($"startPosition.column: {startPosition.column}, startPosition.row: {startPosition.row}");
                    break;
                case "visuals":
                    visualWallsType = new VisualWallType[(columnsInt+1) * (rowsInt+1)];
                    _visuals = true;
                    Debug.Log($"visuals: {_visuals}");
                    break;
                case "reward":
                    _visuals = false;
                    _reward = true;
                    Debug.Log($"visuals: {_visuals}\n reward: {_reward}");
                    break;
            }

            if (_visuals && value.Length > 0)
            {
                StoreWalls(name, value);
            }
            
            if (_reward)
            {
                // TODO
            }
        }

        private BoardPosition SetPosition(string value)
        {
            int position = value.IndexOf(",", StringComparison.Ordinal);
            string numberValue = value[..position];
            int column = (int)Convert.ChangeType(numberValue.Replace(" ", ""), typeof(int));
            
            value = value.Substring(position+1, value.Length-(position+1));
            int row = (int)Convert.ChangeType(value.Replace(" ", ""), typeof(int));
            
            return new BoardPosition(column, row);
        }

        private void StoreWalls(string name, string value)
        {
            // name == column
            int column = (int)Convert.ChangeType(name.Replace(" ", ""), typeof(int)) - 1;
            int row = 0;
            
            if(column > columnsInt+1) return;
            
            int position = value.IndexOf(",", StringComparison.Ordinal);
            while (position != -1)
            {
                string numberValue = value[..position];
                position = value.IndexOf(",", StringComparison.Ordinal);
                value = value.Substring(position+1, value.Length-(position+1));
                
                int wallsPosition = column * (rowsInt + 1) + row;
                visualWallsType[wallsPosition] = (VisualWallType)Convert.ChangeType(numberValue.Replace(" ", ""), typeof(int));
                Debug.Log($"visualWallsType[{wallsPosition}]: {visualWallsType[wallsPosition]}");
                ++row;
                
                if(row > columnsInt+1) break;
            }
        }
        
        private void ReadFile()
        {
            string filePath = Application.dataPath + "/Txt/Board.txt";
            List<string> fileLines = File.ReadAllLines(filePath).ToList();

            foreach(var line in fileLines)
            {
                if(line.Length == 0) continue;

                int comment = line.IndexOf("#", StringComparison.Ordinal);
                if(comment != -1) continue;
                
                int checkThreshold = line.IndexOf(":", StringComparison.Ordinal);
                string varName = line[..checkThreshold];
                string varValue = line.Substring(checkThreshold+1, line.Length-1-checkThreshold);
                CheckVariable(varName, varValue);
            }
        }
        
        public void BuildBoard()
        {
            // set Visual walls
            if (readFile) ReadFile();
            
            Capacity = (ushort)(rowsInt * columnsInt);
            Square = new List<Square>(Capacity);
            wallsType = new WallType[Capacity];

            // Space Radius
            GameObject squareRadiusGo = Instantiate(squarePrefab);
            squareRadiusGo.TryGetComponent(out Square space);
            squareRadiusGo.name = "spaceRadius";
            _spaceRadius = space.Radius;

            // Check Scale
            Vector3 spaceScale = squareRadiusGo.transform.localScale;
            float multiplier = spaceScale.x;
            if (spaceScale.x < spaceScale.y) 
                multiplier = spaceScale.y;
                
            _spaceRadius *= multiplier;
            squareRadiusGo.SetActive(false);

            // Create Board
            SquaresCreation();
            WallCreation();
            BackGroundCreation();
        }

        private void BackGroundCreation()
        {
            GameObject backGroundGameObject = Instantiate(backGroundPrefab, Vector3.zero, Quaternion.identity);
            float boardBoxSize = 2 * (_spaceRadius + spaceAddition / 2);
            backGroundGameObject.transform.localScale = new Vector2(columnsInt * boardBoxSize + spaceAddition, rowsInt * boardBoxSize + spaceAddition);
        }
        
        /**
         * Generation Example
         *  3x3:
         *      2 5 8
         *      1 4 7
         *      0 3 6
         */
        private void SquaresCreation()
        {
            for (ushort i = 0; i < Capacity; ++i)
            {
                // Table position
                ushort row    = (ushort)(i % rowsInt);
                ushort column = (ushort)(i / rowsInt);
                    
                // Square Creation
                GameObject spaceGameObject = Instantiate(squarePrefab, Vector3.zero, Quaternion.identity);
                spaceGameObject.name = $"Square [{column },{column}]";
                spaceGameObject.TryGetComponent(out Square square);
                spaceGameObject.TryGetComponent(out SpriteRenderer spriteRenderer);
                
                // Set Goal Color
                if (row == goalPosition.row && column == goalPosition.column) spriteRenderer.color = goalColor;
                
                // Margin space
                Vector2 spacePosition = new Vector2(column * (_spaceRadius * 2 + spaceAddition), 
                                                    row    * (_spaceRadius * 2 + spaceAddition));
                spacePosition += Offset();    
                square.SetPosition(spacePosition);
                Square.Add(square);
            }
        }

        private void CreateWall(string name, int row, int column, Vector2 position, Vector2 scale)
        {
            // Square Creation
            GameObject spaceGameObject = Instantiate(wallPrefab, Vector3.zero, Quaternion.identity);
            spaceGameObject.name = $"Wall {name} [{row},{column}]";
            spaceGameObject.TryGetComponent(out Square square);
            
            square.SetPosition(position);
            square.SetScale(scale);
            Square.Add(square);
        }
        
        private void WallCreation()
        {
            var columnPoints = columnsInt + 1;
            var rowsPoints = rowsInt + 1;
            var capacityPoints = columnPoints * rowsPoints;
            
            for (var i = 0; i < capacityPoints && visualWallsType.Length > i; ++i)
            {
                // Table position
                ushort row   = (ushort)(i % rowsPoints);
                ushort column = (ushort)(i / rowsPoints);
                
                // Create visuals
                // Wall Side Position
                float side = _spaceRadius * 2 + spaceAddition;
                
                // Wall offset
                Vector2 offset = Offset();
                float sideAddition = -spaceAddition/2 - _spaceRadius;
                float scaleAddition = (_spaceRadius * 2 + spaceAddition * 2) / (_spaceRadius * 2);

                // create wall up
                if ((visualWallsType[i] & VisualWallType.Up) != VisualWallType.Empty)
                {
                    Vector2 position = offset + new Vector2(column * side + sideAddition, row * side);
                    Vector2 scale = new Vector2(spaceAddition, scaleAddition);
                    CreateWall("up", column, row, position, scale);
                }
                
                // create wall right
                if ((visualWallsType[i] & VisualWallType.Right) != VisualWallType.Empty)
                {
                    Vector2 position = offset + new Vector2(column * side, row * side + sideAddition);
                    Vector2 scale = new Vector2(scaleAddition, spaceAddition);
                    CreateWall("right",column, row, position, scale);
                }
                
                // skip extra point for wall visuals propuse 
                if(column == columnsInt || row == rowsInt) continue;

                // Store walls
                int wallPosition = i - column; // exclude extras 
                
                // check other points
                int rightPoint = i + rowsPoints;
                int upPoint = i + 1;

                if((visualWallsType[i] & VisualWallType.Up) != VisualWallType.Empty)
                    wallsType[wallPosition] |= WallType.Left;
                if((visualWallsType[i] & VisualWallType.Right) != VisualWallType.Empty) 
                    wallsType[wallPosition] |= WallType.Down;
                if((visualWallsType[rightPoint] & VisualWallType.Up) != VisualWallType.Empty)
                    wallsType[wallPosition] |= WallType.Right;
                if((visualWallsType[upPoint] & VisualWallType.Right) != VisualWallType.Empty)  
                    wallsType[wallPosition] |= WallType.Up;
            }
        }

        private Vector2 Offset()
        {
            // Middle Position
            Vector2 offset = Vector2.zero;
            offset.x -= (columnsInt-1) * (_spaceRadius + spaceAddition / 2);
            offset.y -=   (rowsInt-1)  * (_spaceRadius + spaceAddition / 2);

            return offset;
        }
    }
}