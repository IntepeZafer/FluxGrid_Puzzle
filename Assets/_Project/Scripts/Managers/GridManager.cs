using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolarityGrid.Grid;
using PolarityGrid.Core;
using PolarityGrid.Blocks;

namespace PolarityGrid.Managers
{
    public class GridManager : MonoBehaviour
    {
        [Header("Level Setup")]
        [SerializeField] private LevelData currentLevel;
        [SerializeField] private Block blockPrefab;
        [SerializeField] private Cell cellPrefab;
        [SerializeField] private float cellSize = 1.1f;

        [Header("UI References")]
        private int _moveCount = 0;

        private Dictionary<Vector2Int, Cell> _gridCells = new Dictionary<Vector2Int, Cell>();
        private bool _isProcessing = false;

        private void OnEnable() => InputManager.OnSwipe += HandleSwipe;
        private void OnDisable() => InputManager.OnSwipe -= HandleSwipe;

        private void Start()
        {
            currentLevel = LevelManager.Instance.GetCurrentLevelData();
            GenerateGrid(currentLevel.width, currentLevel.height);
            SpawnInitialBlocks();
        }

        private void GenerateGrid(int w, int h)
        {
            Vector3 offset = new Vector3((w - 1) * cellSize / 2f, (h - 1) * cellSize / 2f, 0);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    Vector3 worldPos = new Vector3(x * cellSize, y * cellSize, 0) - offset;
                    Cell newCell = Instantiate(cellPrefab, worldPos, Quaternion.identity, transform);
                    newCell.Initialize(pos);
                    _gridCells.Add(pos, newCell);
                }
            }
        }

        private void SpawnInitialBlocks()
        {
            foreach (var data in currentLevel.initialBlocks)
            {
                Cell targetCell = GetCellAt(data.coordinate);
                if (targetCell != null)
                {
                    Block newBlock = Instantiate(blockPrefab, targetCell.transform.position, Quaternion.identity);
                    newBlock.SetType(data.type);
                    targetCell.SetBlock(newBlock);
                }
            }
        }

        private void HandleSwipe(Direction dir)
        {
            if(_isProcessing || GameManager.Instance.CurrentState != GameState.Playing) return;
            if (_isProcessing) return;
            _moveCount++;
            // UI'ı güncelle
            if (UIManager.Instance != null)
                UIManager.Instance.UpdateMoveCount(_moveCount);

            StartCoroutine(SequenceMoveAndMagnetism(dir));
            if(SoundManager.Instance != null) SoundManager.Instance.PlaySwipe();
        }

        private IEnumerator SequenceMoveAndMagnetism(Direction dir)
        {
            _isProcessing = true;
            MoveAllBlocks(dir);
            yield return new WaitUntil(() => !AnyBlockMoving());

            bool physicsHappened = true;
            while (physicsHappened)
            {
                physicsHappened = ApplyPhysics();
                if (physicsHappened)
                {
                    yield return new WaitUntil(() => !AnyBlockMoving());
                    yield return new WaitForSeconds(0.1f);
                }
            }

            // KAZANMA KONTROLÜ
            GameManager.Instance.CheckWinCondition(CheckIfAllBlocksPaired());

            _isProcessing = false;
        }

        private void MoveAllBlocks(Direction dir)
        {
            Vector2Int moveVector = GetVectorFromDirection(dir);
            List<Cell> occupiedCells = new List<Cell>();
            foreach (var cell in _gridCells.Values) if (cell.IsOccupied) occupiedCells.Add(cell);
            SortCellsForMovement(occupiedCells, dir);
            foreach (var cell in occupiedCells) MoveBlockUntilBlocked(cell, moveVector);
        }

        private void MoveBlockUntilBlocked(Cell startCell, Vector2Int direction)
        {
            Block block = startCell.CurrentBlock;
            Vector2Int currentPos = startCell.Coordinates;
            Cell lastValidCell = startCell;

            while (true)
            {
                Vector2Int nextPos = currentPos + direction;
                Cell nextCell = GetCellAt(nextPos);
                if (nextCell != null && !nextCell.IsOccupied) { lastValidCell = nextCell; currentPos = nextPos; }
                else break;
            }

            if (lastValidCell != startCell)
            {
                startCell.ClearCell();
                lastValidCell.SetBlock(block);
                block.MoveTo(lastValidCell.transform.position);
            }
        }

        private bool ApplyPhysics()
        {
            bool anyMovement = false;
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var cell in _gridCells.Values)
            {
                if (!cell.IsOccupied) continue;
                foreach (var dir in dirs)
                {
                    if (CheckAndPull(cell, dir)) { anyMovement = true; break; }
                    if (CheckAndPush(cell, dir)) { anyMovement = true; break; }
                }
            }
            return anyMovement;
        }

        private bool CheckAndPull(Cell startCell, Vector2Int direction)
        {
            Block myBlock = startCell.CurrentBlock;
            Vector2Int scanPos = startCell.Coordinates + direction;
            while (true)
            {
                Cell nextCell = GetCellAt(scanPos);
                if (nextCell == null) break;
                if (nextCell.IsOccupied)
                {
                    if (IsOpposite(myBlock.Type, nextCell.CurrentBlock.Type))
                    {
                        Vector2Int targetPos = startCell.Coordinates + direction;
                        Cell targetCell = GetCellAt(targetPos);
                        if (targetCell != null && !targetCell.IsOccupied && targetCell != nextCell)
                        {
                            Block pulledBlock = nextCell.CurrentBlock;
                            nextCell.ClearCell();
                            targetCell.SetBlock(pulledBlock);
                            pulledBlock.MoveTo(targetCell.transform.position);
                            return true;
                        }
                    }
                    break;
                }
                scanPos += direction;
            }
            return false;
        }

        private bool CheckAndPush(Cell startCell, Vector2Int direction)
        {
            Block myBlock = startCell.CurrentBlock;
            Vector2Int neighborPos = startCell.Coordinates + direction;
            Cell neighborCell = GetCellAt(neighborPos);
            if (neighborCell != null && neighborCell.IsOccupied)
            {
                Block otherBlock = neighborCell.CurrentBlock;
                if (myBlock.Type == otherBlock.Type && myBlock.Type != BlockType.Obstacle)
                {
                    Vector2Int pushTargetPos = neighborCell.Coordinates + direction;
                    Cell pushTargetCell = GetCellAt(pushTargetPos);
                    if (pushTargetCell != null && !pushTargetCell.IsOccupied)
                    {
                        neighborCell.ClearCell();
                        pushTargetCell.SetBlock(otherBlock);
                        otherBlock.MoveTo(pushTargetCell.transform.position);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CheckIfAllBlocksPaired()
        {
            List<Block> activeBlocks = new List<Block>();
            foreach (var cell in _gridCells.Values)
                if (cell.IsOccupied && (cell.CurrentBlock.Type == BlockType.Positive || cell.CurrentBlock.Type == BlockType.Negative))
                    activeBlocks.Add(cell.CurrentBlock);

            if (activeBlocks.Count == 0) return false;
            foreach (var b in activeBlocks) if (!IsBlockPaired(b)) return false;
            return true;
        }

        private bool IsBlockPaired(Block block)
        {
            Cell c = null;
            foreach (var cell in _gridCells.Values) if (cell.CurrentBlock == block) { c = cell; break; }
            if (c == null) return false;

            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var d in dirs)
            {
                Cell n = GetCellAt(c.Coordinates + d);
                if (n != null && n.IsOccupied && IsOpposite(block.Type, n.CurrentBlock.Type)) return true;
            }
            return false;
        }

        private bool IsOpposite(BlockType a, BlockType b) => (a == BlockType.Positive && b == BlockType.Negative) || (a == BlockType.Negative && b == BlockType.Positive);
        private bool AnyBlockMoving() { foreach (var cell in _gridCells.Values) if (cell.IsOccupied && cell.CurrentBlock.IsMoving) return true; return false; }
        private void SortCellsForMovement(List<Cell> cells, Direction dir) { cells.Sort((a, b) => { if (dir == Direction.Right) return b.Coordinates.x.CompareTo(a.Coordinates.x); if (dir == Direction.Left) return a.Coordinates.x.CompareTo(b.Coordinates.x); if (dir == Direction.Up) return b.Coordinates.y.CompareTo(a.Coordinates.y); if (dir == Direction.Down) return a.Coordinates.y.CompareTo(b.Coordinates.y); return 0; }); }
        private Vector2Int GetVectorFromDirection(Direction dir) => dir switch { Direction.Up => Vector2Int.up, Direction.Down => Vector2Int.down, Direction.Left => Vector2Int.left, Direction.Right => Vector2Int.right, _ => Vector2Int.zero };
        public Cell GetCellAt(Vector2Int pos) => _gridCells.TryGetValue(pos, out Cell c) ? c : null;
    }
}