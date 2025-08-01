using UnityEngine;

public class SandParticle : MonoBehaviour
{
    [SerializeField] private Constants.BlockType particleType;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    public Constants.BlockType ParticleType 
    { 
        get => particleType; 
        set 
        { 
            particleType = value;
            UpdateVisual();
        } 
    }
    
    void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
    
    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;
        
        // 使用简单的颜色而不是加载Sprite资源
        Color color = Color.white;
        switch (particleType)
        {
            case Constants.BlockType.Red:
                color = new Color(1f, 0.2f, 0.2f);
                break;
            case Constants.BlockType.Blue:
                color = new Color(0.2f, 0.5f, 1f);
                break;
            case Constants.BlockType.Green:
                color = new Color(0.2f, 1f, 0.3f);
                break;
            case Constants.BlockType.Yellow:
                color = new Color(1f, 1f, 0.2f);
                break;
        }
        
        spriteRenderer.color = color;
    }
    
    public void SetPosition(Vector2 position)
    {
        transform.position = new Vector3(position.x, position.y, 0);
    }
}