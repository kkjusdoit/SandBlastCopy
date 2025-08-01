using UnityEngine;

public static class Constants
{
    // 游戏区域尺寸
    public const int BOARD_WIDTH = 10;
    public const int BOARD_HEIGHT = 20;
    
    // 缩放因子 - 将1x1方块拆分为7x7沙粒
    public const int SCALE_FACTOR = 7;
    
    // 沙粒大小
    public const float SAND_SIZE = 0.1f;
    
    // 物理更新频率
    public const float PHYSICS_UPDATE_RATE = 0.05f;
    
    // 方块类型
    public enum BlockType
    {
        Empty,
        Red,
        Blue,
        Green,
        Yellow
    }
    
    // 俄罗斯方块形状定义
    public static readonly int[][,] TETRIS_SHAPES = new int[][,]
    {
        // I型
        new int[,] { {1,1,1,1} },
        
        // O型
        new int[,] { 
            {1,1},
            {1,1}
        },
        
        // T型
        new int[,] {
            {0,1,0},
            {1,1,1}
        },
        
        // L型
        new int[,] {
            {1,0},
            {1,0},
            {1,1}
        },
        
        // J型
        new int[,] {
            {0,1},
            {0,1},
            {1,1}
        },
        
        // S型
        new int[,] {
            {0,1,1},
            {1,1,0}
        },
        
        // Z型
        new int[,] {
            {1,1,0},
            {0,1,1}
        }
    };
}