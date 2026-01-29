using UnityEngine;
using System.Collections.Generic;

namespace PolarityGrid.Core
{
    [System.Serializable]
    public class BlockSpawnData
    {
        public Vector2Int coordinate;
        public BlockType type;
    }

    [CreateAssetMenu(fileName = "NewLevel", menuName = "PolarityGrid/Level Data")]
    public class LevelData : ScriptableObject
    {
        public int width = 5;
        public int height = 5;
        public List<BlockSpawnData> initialBlocks = new List<BlockSpawnData>();
    }
}