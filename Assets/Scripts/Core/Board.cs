using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Board : MonoBehaviour
{
    private Constants.BlockType[,] board;
    private SandParticle[,] visualBoard;
    private bool isPhysicsRunning = false;
    
    public event System.Action<List<Vector2Int>> OnSandCleared;
    
    void Awake()
    {
        board = new Constants.BlockType[Constants.BOARD_WIDTH, Constants.BOARD_HEIGHT];
        visualBoard = new SandParticle[Constants.BOARD_WIDTH, Constants.BOARD_HEIGHT];
        
        // 初始化为空
        for (int x = 0; x < Constants.BOARD_WIDTH; x++)
        {
            for (int y = 0; y < Constants.BOARD_HEIGHT; y++)
            {
                board[x, y] = Constants.BlockType.Empty;
                visualBoard[x, y] = null;
            }
        }
    }
    
    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < Constants.BOARD_WIDTH && y >= 0 && y < Constants.BOARD_HEIGHT;
    }
    
    public Constants.BlockType GetCell(int x, int y)
    {
        if (!IsValidPosition(x, y)) return Constants.BlockType.Empty;
        return board[x, y];
    }
    
    public void SetCell(int x, int y, Constants.BlockType type)
    {
        if (!IsValidPosition(x, y)) return;
        
        board[x, y] = type;
        UpdateVisualCell(x, y, type);
    }
    
    private void UpdateVisualCell(int x, int y, Constants.BlockType type)
    {
        // 销毁原有沙粒
        if (visualBoard[x, y] != null)
        {
            Destroy(visualBoard[x, y].gameObject);
            visualBoard[x, y] = null;
        }
        
        // 创建新2D沙粒
        if (type != Constants.BlockType.Empty)
        {
            GameObject sandPrefab = Resources.Load<GameObject>("Prefabs/SandParticle");
            Vector2 worldPos = new Vector2(x * Constants.SAND_SIZE, -y * Constants.SAND_SIZE);
            GameObject sandObj = Instantiate(sandPrefab, worldPos, Quaternion.identity, transform);
            
            SandParticle sand = sandObj.GetComponent<SandParticle>();
            sand.ParticleType = type;
            visualBoard[x, y] = sand;
        }
    }
    
    /// <summary>
    /// 开始物理模拟
    /// </summary>
    public void StartPhysicsSimulation()
    {
        if (!isPhysicsRunning)
        {
            StartCoroutine(PhysicsSimulationCoroutine());
        }
    }
    
    private IEnumerator PhysicsSimulationCoroutine()
    {
        isPhysicsRunning = true;
        
        while (true)
        {
            bool hasMoved = UpdateStep();
            
            if (!hasMoved)
            {
                // 物理模拟结束，检查消除
                var clearedCells = ClearSand();
                if (clearedCells.Count > 0)
                {
                    OnSandCleared?.Invoke(clearedCells);
                    // 继续物理模拟，因为消除后可能有新的下落
                    continue;
                }
                else
                {
                    // 没有消除，结束物理模拟
                    break;
                }
            }
            
            yield return new WaitForSeconds(Constants.PHYSICS_UPDATE_RATE);
        }
        
        isPhysicsRunning = false;
    }
    
    /// <summary>
    /// 对整个沙盘进行一轮物理计算
    /// </summary>
    public bool UpdateStep()
    {
        bool hasFallen = false;
        
        // 自底向上，自左向右遍历
        for (int y = Constants.BOARD_HEIGHT - 2; y >= 0; y--)
        {
            for (int x = 0; x < Constants.BOARD_WIDTH; x++)
            {
                Constants.BlockType cell = GetCell(x, y);
                if (cell == Constants.BlockType.Empty) continue;
                
                // 决策1：垂直下落（最高优先级）
                if (y + 1 < Constants.BOARD_HEIGHT && GetCell(x, y + 1) == Constants.BlockType.Empty)
                {
                    SetCell(x, y + 1, cell);
                    SetCell(x, y, Constants.BlockType.Empty);
                    hasFallen = true;
                    continue;
                }
                
                // 决策2&3：斜向滑动 - 重要的边界检查
                bool canGoLeft = (x - 1 >= 0) && (y + 1 < Constants.BOARD_HEIGHT) && 
                                GetCell(x - 1, y + 1) == Constants.BlockType.Empty;
                                
                bool canGoRight = (x + 1 < Constants.BOARD_WIDTH) && (y + 1 < Constants.BOARD_HEIGHT) && 
                                 GetCell(x + 1, y + 1) == Constants.BlockType.Empty &&
                                 GetCell(x + 1, y) == Constants.BlockType.Empty;
                
                if (canGoLeft && canGoRight)
                {
                    // 随机选择方向
                    int direction = Random.Range(0, 2) == 0 ? -1 : 1;
                    SetCell(x + direction, y + 1, cell);
                    SetCell(x, y, Constants.BlockType.Empty);
                    hasFallen = true;
                }
                else if (canGoLeft)
                {
                    SetCell(x - 1, y + 1, cell);
                    SetCell(x, y, Constants.BlockType.Empty);
                    hasFallen = true;
                }
                else if (canGoRight)
                {
                    SetCell(x + 1, y + 1, cell);
                    SetCell(x, y, Constants.BlockType.Empty);
                    hasFallen = true;
                }
            }
        }
        
        return hasFallen;
    }
    
    /// <summary>
    /// 流沙消除逻辑
    /// </summary>
    public List<Vector2Int> ClearSand()
    {
        HashSet<string> visited = new HashSet<string>();
        List<Vector2Int> clearedCells = new List<Vector2Int>();
        
        for (int y = 0; y < Constants.BOARD_HEIGHT; y++)
        {
            for (int x = 0; x < Constants.BOARD_WIDTH; x++)
            {
                Constants.BlockType cellType = GetCell(x, y);
                string key = $"{x},{y}";
                
                if (cellType != Constants.BlockType.Empty && !visited.Contains(key))
                {
                    var group = FindConnectedGroup(x, y, cellType, visited);
                    
                    // 如果团块同时接触左右墙壁，则消除
                    if (group.touchesLeftWall && group.touchesRightWall)
                    {
                        foreach (var cell in group.cells)
                        {
                            SetCell(cell.x, cell.y, Constants.BlockType.Empty);
                            clearedCells.Add(cell);
                        }
                    }
                }
            }
        }
        
        return clearedCells;
    }
    
    private (List<Vector2Int> cells, bool touchesLeftWall, bool touchesRightWall) FindConnectedGroup(
        int startX, int startY, Constants.BlockType targetType, HashSet<string> visited)
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        bool touchesLeftWall = false;
        bool touchesRightWall = false;
        
        queue.Enqueue(new Vector2Int(startX, startY));
        visited.Add($"{startX},{startY}");
        
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            cells.Add(current);
            
            // 检查是否接触墙壁
            if (current.x == 0) touchesLeftWall = true;
            if (current.x == Constants.BOARD_WIDTH - 1) touchesRightWall = true;
            
            // 四方向扩展
            Vector2Int[] directions = {
                new Vector2Int(0, 1), new Vector2Int(0, -1),
                new Vector2Int(1, 0), new Vector2Int(-1, 0)
            };
            
            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;
                string nextKey = $"{next.x},{next.y}";
                
                if (IsValidPosition(next.x, next.y) && 
                    !visited.Contains(nextKey) && 
                    GetCell(next.x, next.y) == targetType)
                {
                    visited.Add(nextKey);
                    queue.Enqueue(next);
                }
            }
        }
        
        return (cells, touchesLeftWall, touchesRightWall);
    }
}