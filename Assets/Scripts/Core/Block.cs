using UnityEngine;
using System.Collections.Generic;

public class Block : MonoBehaviour
{
    [SerializeField] private Constants.BlockType blockType;
    [SerializeField] private int shapeIndex;
    private int[,] scaledMatrix;
    private List<SandParticle> sandParticles = new List<SandParticle>();
    
    public Constants.BlockType BlockType => blockType;
    public int[,] ScaledMatrix => scaledMatrix;
    public List<SandParticle> SandParticles => sandParticles;
    
    public void Initialize(int shapeIdx, Constants.BlockType type)
    {
        shapeIndex = shapeIdx;
        blockType = type;
        
        // 创建缩放后的矩阵
        int[,] baseMatrix = Constants.TETRIS_SHAPES[shapeIndex];
        scaledMatrix = CreateScaledMatrix(baseMatrix);
        
        // 生成沙粒
        CreateSandParticles();
    }
    
    /// <summary>
    /// 根据缩放因子，将基础矩阵放大成由小沙粒组成的大矩阵
    /// </summary>
    private int[,] CreateScaledMatrix(int[,] baseMatrix)
    {
        int baseRows = baseMatrix.GetLength(0);
        int baseCols = baseMatrix.GetLength(1);
        
        int scaledRows = baseRows * Constants.SCALE_FACTOR;
        int scaledCols = baseCols * Constants.SCALE_FACTOR;
        
        int[,] scaledMatrix = new int[scaledRows, scaledCols];
        
        for (int r = 0; r < scaledRows; r++)
        {
            for (int c = 0; c < scaledCols; c++)
            {
                int baseRow = r / Constants.SCALE_FACTOR;
                int baseCol = c / Constants.SCALE_FACTOR;
                scaledMatrix[r, c] = baseMatrix[baseRow, baseCol];
            }
        }
        
        return scaledMatrix;
    }
    
    /// <summary>
    /// 根据缩放矩阵创建沙粒GameObject
    /// </summary>
    private void CreateSandParticles()
    {
        GameObject sandPrefab = Resources.Load<GameObject>("Prefabs/SandParticle");
        
        for (int r = 0; r < scaledMatrix.GetLength(0); r++)
        {
            for (int c = 0; c < scaledMatrix.GetLength(1); c++)
            {
                if (scaledMatrix[r, c] == 1)
                {
                    Vector3 position = new Vector3(c * Constants.SAND_SIZE, -r * Constants.SAND_SIZE, 0);
                    GameObject sandObj = Instantiate(sandPrefab, transform.position + position, Quaternion.identity, transform);
                    
                    SandParticle sand = sandObj.GetComponent<SandParticle>();
                    sand.ParticleType = blockType;
                    sandParticles.Add(sand);
                }
            }
        }
    }
    
    /// <summary>
    /// 方块"沙化" - 将沙粒分离到游戏板上
    /// </summary>
    public void Sandify(Board board, Vector2Int position)
    {
        for (int r = 0; r < scaledMatrix.GetLength(0); r++)
        {
            for (int c = 0; c < scaledMatrix.GetLength(1); c++)
            {
                if (scaledMatrix[r, c] == 1)
                {
                    int boardX = position.x + c;
                    int boardY = position.y + r;
                    
                    if (board.IsValidPosition(boardX, boardY))
                    {
                        board.SetCell(boardX, boardY, blockType);
                    }
                }
            }
        }
        
        // 销毁原方块
        Destroy(gameObject);
    }
}