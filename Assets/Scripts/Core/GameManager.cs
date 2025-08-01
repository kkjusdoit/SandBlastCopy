using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Board board;
    [SerializeField] private Transform spawnPoint;
    
    private Block currentBlock;
    private float dropTimer = 0f;
    private float dropInterval = 1f;
    
    void Start()
    {
        if (board == null)
        {
            board = FindObjectOfType<Board>();
        }
        
        board.OnSandCleared += OnSandCleared;
        SpawnNewBlock();
    }
    
    void Update()
    {
        HandleInput();
        HandleDrop();
    }
    
    private void HandleInput()
    {
        if (currentBlock == null) return;
        
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveBlock(-1, 0);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveBlock(1, 0);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveBlock(0, 1);
        }
    }
    
    private void HandleDrop()
    {
        if (currentBlock == null) return;
        
        dropTimer += Time.deltaTime;
        if (dropTimer >= dropInterval)
        {
            dropTimer = 0f;
            
            if (!MoveBlock(0, 1))
            {
                // 方块落地，开始沙化
                LandBlock();
            }
        }
    }
    
    private bool MoveBlock(int deltaX, int deltaY)
    {
        if (currentBlock == null) return false;
        
        // 获取当前方块的2D位置
        Vector2 currentPos = currentBlock.transform.position;
        Vector2 newPos = currentPos + new Vector2(deltaX * Constants.SAND_SIZE, deltaY * Constants.SAND_SIZE);
        
        // 将世界坐标转换为网格坐标
        Vector2Int currentGridPos = new Vector2Int(
            Mathf.RoundToInt(currentPos.x / Constants.SAND_SIZE),
            Mathf.RoundToInt(-currentPos.y / Constants.SAND_SIZE)
        );
        
        Vector2Int newGridPos = new Vector2Int(
            currentGridPos.x + deltaX,
            currentGridPos.y + deltaY
        );
        
        // 检查新位置是否与方块的形状发生碰撞
        if (CanPlaceBlockAt(newGridPos))
        {
            currentBlock.transform.position = newPos;
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 检查方块是否可以放置在指定位置
    /// </summary>
    private bool CanPlaceBlockAt(Vector2Int gridPosition)
    {
        if (currentBlock == null) return false;
        
        int[,] scaledMatrix = currentBlock.ScaledMatrix;
        
        for (int r = 0; r < scaledMatrix.GetLength(0); r++)
        {
            for (int c = 0; c < scaledMatrix.GetLength(1); c++)
            {
                if (scaledMatrix[r, c] == 1)
                {
                    int boardX = gridPosition.x + c;
                    int boardY = gridPosition.y + r;
                    
                    // 边界检查
                    if (boardX < 0 || boardX >= Constants.BOARD_WIDTH || 
                        boardY < 0 || boardY >= Constants.BOARD_HEIGHT)
                    {
                        return false;
                    }
                    
                    // 碰撞检查
                    if (board.GetCell(boardX, boardY) != Constants.BlockType.Empty)
                    {
                        return false;
                    }
                }
            }
        }
        
        return true;
    }
    
    private void LandBlock()
    {
        if (currentBlock == null) return;
        
        // 计算方块在游戏板上的2D位置
        Vector2 worldPos = currentBlock.transform.position;
        Vector2Int boardPos = new Vector2Int(
            Mathf.RoundToInt(worldPos.x / Constants.SAND_SIZE),
            Mathf.RoundToInt(-worldPos.y / Constants.SAND_SIZE)
        );
        
        // 沙化方块
        currentBlock.Sandify(board, boardPos);
        currentBlock = null;
        
        // 开始物理模拟
        board.StartPhysicsSimulation();
    }
    
    private void SpawnNewBlock()
    {
        GameObject blockPrefab = Resources.Load<GameObject>("Prefabs/Block");
        Vector2 spawnPos = spawnPoint.position; // 2D位置
        GameObject blockObj = Instantiate(blockPrefab, spawnPos, Quaternion.identity);
        
        currentBlock = blockObj.GetComponent<Block>();
        
        // 随机形状和颜色
        int shapeIndex = Random.Range(0, Constants.TETRIS_SHAPES.Length);
        Constants.BlockType blockType = (Constants.BlockType)Random.Range(1, 5);
        
        currentBlock.Initialize(shapeIndex, blockType);
    }
    
    private void OnSandCleared(System.Collections.Generic.List<Vector2Int> clearedCells)
    {
        Debug.Log($"Cleared {clearedCells.Count} sand particles!");
        
        // 等待物理模拟结束后生成新方块
        StartCoroutine(WaitAndSpawnNewBlock());
    }
    
    private IEnumerator WaitAndSpawnNewBlock()
    {
        yield return new WaitForSeconds(0.5f);
        SpawnNewBlock();
    }
}