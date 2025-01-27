using Board;
using UnityEngine;

namespace Core
{
    // Setup board
    public sealed class BoardManager : MonoBehaviour
    {
        [SerializeField] private BoardLoader boardLoader;
        public BoardInfo BoardInfo { get; private set; }
        
        public void SetupScene()
        {
            // Create board
            boardLoader.BuildBoard();
            
            // Create reference Object
            BoardInfo = new BoardInfo(boardLoader);
        }
    }
}