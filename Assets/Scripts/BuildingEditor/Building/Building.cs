using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class Building : MonoBehaviour
{

    void Awake() {
        
        // Debug.Log( collider );
        Start();
    }

    void Start() {
        SetData( 0 );
        gameObject.transform.SetParent( EditorLayer.Instance.transform );
    }


    // public Vector2Int[] type1 = { new Vector2Int( 0, 0 ), new Vector2Int( 1, 0 ) };
    // public Vector2Int[] type2 = { new Vector2Int( 0, 0 ), new Vector2Int( 0, 1 ) };

    public List< Vector2Int[] > types = new List< Vector2Int[] >{
        new Vector2Int[] { new Vector2Int( 0, 0 ), new Vector2Int( 0, 1 ), new Vector2Int( 1, 0 ) },
        new Vector2Int[] { new Vector2Int( 0, 0 ), new Vector2Int( 0, 1 ) }
    };

    private SpriteRenderer BuildingRenderer;

    // private Sprite BlockSprite;
    private Sprite BuildingSprite;
    private Vector2Int[] BlockData;

    public void SetData( int blockId ) {
        SetComponent();
        LoadSprite();
        BlockData = types[ blockId ];
        // DrawBlock( types[ blockId ] );
        SetHitArea();
        gameObject.transform.localScale = new Vector3( EditorConfig.BUILDING_SCALE, EditorConfig.BUILDING_SCALE, 1 );
        Debug.Log( BuildingSprite );
    }


    private void SetHitArea() {
        BoxCollider2D collider = gameObject.AddComponent< BoxCollider2D >();
        collider.size = new Vector2( 2 * EditorConfig.BLOCK_SIZE, 2 * EditorConfig.BLOCK_SIZE );
    }

    public void SetComponent() {
        // renderer = gameObject.GetComponent< SpriteRenderer >();

        if( BuildingRenderer == null ) {
            BuildingRenderer = gameObject.AddComponent< SpriteRenderer >();
            Debug.Log( "sprite renderer ade" );
        }

        BuildingRenderer = gameObject.GetComponent< SpriteRenderer >();
        
    }

    public void LoadSprite() {
        // BlockSprite = TextureManager.GetSprite( "atlases/Battle", "Wall" );
        BuildingSprite = TextureManager.GetSprite( "atlases/Editor", "Archery0001" );
        //BlockSprite = TextureManager.GetSprite( "atlases/Editor", "Land1" );
            // Texturepacker 에서 가져올 때 algorithm polygon 아니고 maxrect 로 설정하기
        BuildingRenderer.sprite = BuildingSprite;
        Debug.Log( BuildingRenderer.sprite.rect.width );
    }



}



public static class BlockId {
    public static readonly Vector2Int[] type1 = { new Vector2Int( 0, 0 ), new Vector2Int( 1, 0 ) };
    public static readonly Vector2Int[] type2 = { new Vector2Int( 0, 0 ), new Vector2Int( 0, 1 ) };
}
 
