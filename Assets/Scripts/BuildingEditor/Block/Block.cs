using UnityEngine;

public class Block : MonoBehaviour
{
    void Awake() {
        CallComponent();
        SetBg();
        // SetColor( BlockColor.GREEN );
    }

    public GameObject bgObj;
    public GameObject spObj;

    private SpriteRenderer bgRenderer;
    private SpriteRenderer spRenderer;

    private Transform bgTransform;
    private Transform spTransform;
    
    private Sprite bgSprite;
    private Sprite spSprite;

    private void CallComponent() {
        bgRenderer = bgObj.GetComponent< SpriteRenderer >();
        spRenderer = spObj.GetComponent< SpriteRenderer >();

        bgTransform = bgObj.GetComponent< Transform >();
        spTransform = spObj.GetComponent< Transform >();
    }

    public void SetBg() {
        bgSprite = TextureManager.GetSprite( "atlases/Editor", "COMMON_BG0001" );
        bgRenderer.sprite = bgSprite;
        float scale = ( float ) EditorConfig.SPACE_X / EditorConfig.BASIC_SIZE;
        bgTransform.localScale = new Vector3( scale, scale, 0 );
        bgRenderer.color = new Color( 1f, 1f, 1f, 0.5f );
    }

    public void SetColor( BlockColor color ) {
        spSprite = TextureManager.GetSprite( "atlases/Editor", $"COMMON_BG000{ ( int )color }" );
        float spX = ( float ) ( EditorConfig.SPACE_X - EditorConfig.BLOCK_SIZE ) * 0.5f;
        float spY = ( float ) ( EditorConfig.SPACE_Y - EditorConfig.BLOCK_SIZE ) * 0.5f;
        spRenderer.sprite = spSprite;

        float scale = ( float ) EditorConfig.BLOCK_SIZE / EditorConfig.BASIC_SIZE;
        spTransform.localScale = new Vector3( scale, scale, 0 );
        spTransform.localPosition = new Vector3( spX, spY, 0 );

        bgRenderer.sortingOrder = 1;
        spRenderer.sortingOrder = 2;
    }

}

public enum BlockColor {
    LIGHT_ORANGE = 1,
    ORANGE = 2,
    RED = 3,
    GREEN = 4,
    DARK_GREEN = 5,
    GRAY = 6,
    BLACK = 7,
    DARK_GRAY = 8
}  
