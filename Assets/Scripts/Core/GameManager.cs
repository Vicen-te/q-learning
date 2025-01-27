using System;
using System.Diagnostics;
using Board;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private BoardManager boardManager;

        private BoardInfo BoardInfo => boardManager.BoardInfo;

        [Header("AI")] 
        [SerializeField] private float waitSecondsInteraction = 0.2f;
        [SerializeField] private float waitSecondsPlays = 1f;

        [SerializeField] private AIManager aiManager;
        
        private void StartValues()
        {
            // set ai values
            aiManager.SetWaitTime(waitSecondsInteraction);
            aiManager.SetQTable(BoardInfo.RowsInt, BoardInfo.ColumnsInt, BoardInfo.GoalPosition, BoardInfo.WallsType);
            // aiManager.SetEpisodes(1000);
            aiManager.SetBoardInfo(BoardInfo);
            
            // AI start position.
            Square square = BoardInfo.GetStartPositionSquare();
            aiManager.SetPosition(square.transform.position);
        }
                
        private void TrainAI()
        {
            // Start StopWatch
            Stopwatch stopWatch = Stopwatch.StartNew();
            
            // Execute AI
            Debug.Log("Starting Training ...");
            
            aiManager.TrainAgentWithoutVisualizing();
            
            // Stop and debug elapsed time
            stopWatch.Stop();
            TimeSpan timeSpan = stopWatch.Elapsed;
                
            // Debug
            Debug.Log($"Training complete! Elapsed time: {timeSpan.TotalMilliseconds}");
        }
        
        private void Start()
        {
            boardManager.SetupScene();
            StartValues();
            TrainAI();
        }
    }
}
