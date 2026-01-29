using UnityEngine;
using PolarityGrid.Blocks;

namespace PolarityGrid.Grid
{
    public class Cell : MonoBehaviour
    {
        public Vector2Int Coordinates { get; private set; }
        public Block CurrentBlock { get; private set; }

        public void Initialize(Vector2Int coords)
        {
            Coordinates = coords;
        }

        public void SetBlock(Block block)
        {
            CurrentBlock = block;
        }

        public void ClearCell()
        {
            CurrentBlock = null;
        }

        public bool IsOccupied => CurrentBlock != null;
    }
}