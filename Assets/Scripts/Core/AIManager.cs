using System.Collections;
using System.Collections.Generic;
using Board;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Core
{
    public class AIManager : MonoBehaviour
    {
        private int _currentEpisode;
        private float _waitTime;
        private QTable _table;
        private List<BoardPosition> _path;
        private BoardInfo _boardInfo;

        private readonly List<List<BoardPosition>> _paths;
        private bool _once;

        [SerializeField] private bool Sarsa;
        [SerializeField] private int episodes = 1000;

        private AIManager()
        {
            _path = new List<BoardPosition>();
        }

        #region Setters

        public void SetQTable(int columns, int rows, BoardPosition goal, WallType[] wallTypes)
        {
            _table = new QTable(rows, columns, goal, wallTypes);
        }

        public void SetPosition(Vector2 position)
        {
            transform.position = position;
        }
        
        public void SetWaitTime(float waitTime)
        {
            _waitTime = waitTime;
        }
        
        public void SetBoardInfo(BoardInfo boardInfo)
        {
            _boardInfo = boardInfo;
        }

        #endregion

        // ReSharper disable Unity.PerformanceAnalysis
        private IEnumerator Animation(Vector2 currentPosition, List<Vector2> visualPath)
        {
            if (visualPath.Count > 0)
            {
                Debug.Log($"Visualizing Position {visualPath.Count}");
                Vector2 newPosition = visualPath[0];
                float elapsedTime = 0;
            
                while (elapsedTime < _waitTime)
                {
                    transform.position = new Vector3
                    (
                        Mathf.Lerp(currentPosition.x, newPosition.x, elapsedTime/_waitTime), 
                        Mathf.Lerp(currentPosition.y, newPosition.y, elapsedTime/_waitTime)
                    );
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            
                transform.position = new Vector2(newPosition.x, newPosition.y);
                visualPath.RemoveAt(0);
                MoveTo(visualPath);
                yield return null;
            }
            else if(!_once)
            {
                TrainAgentVisualizing();
                yield return null;
            }
        }

        private void MoveTo(List<Vector2> visualPath) 
        {
            // Move AI
            StartCoroutine(Animation(transform.position, visualPath));
        }
        
        private void MoveTo(List<BoardPosition> visualPath)
        {
            List<Vector2> vector2VisualPath = new List<Vector2>();
            foreach (var boardPosition in visualPath)
            {
                int discIndex = _boardInfo.GetIndexOfPosition(boardPosition);
                Square square = _boardInfo.GetSquare(discIndex);
                vector2VisualPath.Add(square.transform.position);
            }
            MoveTo(vector2VisualPath);
        }

        public void TrainAgentVisualizing()
        {
            if (++_currentEpisode > episodes) return;

            List<Vector2> visualPath = new List<Vector2>();
            BoardPosition boardPosition = _table.GetStartingLocation();

            while (!_table.IsTerminalState(boardPosition))
            {
                _table.UpdateQTable(ref boardPosition);
                
                // add position
                int discIndex = _boardInfo.GetIndexOfPosition(boardPosition);
                Square square = _boardInfo.GetSquare(discIndex);
                visualPath.Add(square.transform.position);
            }
            
            // move to boardPosition.
            Debug.Log("Visualizing Training ...");
            MoveTo(visualPath);
        }
        
        public void PrintQValues()
        {
            Debug.Log("Paths taken by the agent:");

            string log = "";
            
            for (int i = 0; i < _paths.Count; i++)
            {
                log += $"\nEpisode {i + 1}: ";
                foreach (var step in _paths[i])
                {
                    log += $"({step.column}, {step.row}) -> ";
                }
                log +="Goal";
            }

            Debug.Log(log);
        }
        
        public void TrainAgentPrintPaths()
        {
            for (int episode = 0; episode < episodes; episode++)
            {
                BoardPosition boardPosition = _table.GetStartingLocation();
                List<BoardPosition> _currentPath = new List<BoardPosition>();

                while (!_table.IsTerminalState(boardPosition))
                {
                    _currentPath.Add(boardPosition);
                    _table.UpdateQTable(ref boardPosition);
                }
                
                _paths.Add(_currentPath);
            }
        }

        private void TrainSarsa()
        {
            for (int episode = 0; episode < episodes; episode++)
            {
                BoardPosition boardPosition = _table.GetStartingLocation();
                int actionIndex = _table.GetNextActionWithoutEpsilon(boardPosition);
                
                while (!_table.IsTerminalState(boardPosition))
                {
                    _table.UpdateSARSATable(ref boardPosition, ref actionIndex);
                }
            }
        }

        private void TrainQLearning()
        {
            for (int episode = 0; episode < episodes; episode++)
            {
                BoardPosition boardPosition = _table.GetStartingLocation();

                while (!_table.IsTerminalState(boardPosition))
                {
                    _table.UpdateQTable(ref boardPosition);
                }
            }
        }
        
        public void TrainAgentWithoutVisualizing()
        {
            if (Sarsa)
            {
                TrainSarsa();
            }
            else
            {
                TrainQLearning();
            }
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && !_once)
            {
                _once = true;
                _path = _table.GetShortestPath(_boardInfo.StartPosition);
                MoveTo(_path);
            }
        }
    }
}