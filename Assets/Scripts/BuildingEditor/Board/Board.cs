using UnityEngine;
using System;
using System.Collections.Generic;


public class Board : MonoBehaviour
{
        // 0 : block not activated
        // 1 : placeable
        // 2 : occupied (building filled)
        public int[][] state = new int[][] {
        new int[] { 1, 1, 1, 1, 1, 1, 1, 1 },
        new int[] { 1, 1, 1, 1, 1, 1, 1, 1 },
        new int[] { 1, 1, 1, 1, 1, 1, 1, 1 },
        new int[] { 1, 1, 1, 1, 1, 1, 1, 1 },
        new int[] { 1, 1, 1, 1, 1, 1, 1, 1 },
        new int[] { 1, 1, 1, 1, 1, 1, 1, 1 },
        new int[] { 1, 1, 1, 1, 1, 1, 1, 1 },
        new int[] { 1, 1, 1, 1, 1, 1, 1, 1 },
        new int[] { 1, 1, 1, 1, 1, 1, 1, 1 },
        new int[] { 1, 1, 1, 1, 1, 1, 1, 1 }
    };

    public int[][] orgState = new int[][] {
        new int[] { 1, 1, 1, 1, 1, 1, 1, 1 },
        new int[] { 1, 1, 1, 1, 1, 1, 1, 1 },
        new int[] { 1, 1, 1, 1, 1, 1, 1, 1 },
        new int[] { 1, 1, 1, 1, 1, 1, 1, 1 },
        new int[] { 1, 1, 1, 1, 1, 1, 1, 1 },
        new int[] { 1, 1, 1, 1, 1, 1, 1, 1 },
        new int[] { 1, 1, 1, 1, 1, 1, 1, 1 },
        new int[] { 1, 1, 1, 1, 1, 1, 1, 1 },
        new int[] { 1, 1, 1, 1, 1, 1, 1, 1 },
        new int[] { 1, 1, 1, 1, 1, 1, 1, 1 }
    };

    public GameObject[][] buildingList = new GameObject[][] {
        new GameObject[] { null, null, null, null, null, null, null, null },
        new GameObject[] { null, null, null, null, null, null, null, null },
        new GameObject[] { null, null, null, null, null, null, null, null },  
        new GameObject[] { null, null, null, null, null, null, null, null },
        new GameObject[] { null, null, null, null, null, null, null, null },
        new GameObject[] { null, null, null, null, null, null, null, null },
        new GameObject[] { null, null, null, null, null, null, null, null },
        new GameObject[] { null, null, null, null, null, null, null, null },
        new GameObject[] { null, null, null, null, null, null, null, null },
        new GameObject[] { null, null, null, null, null, null, null, null },
    };

    public Block[][] blockList = new Block[][] {
        new Block[] { null, null, null, null, null, null, null, null },
        new Block[] { null, null, null, null, null, null, null, null },
        new Block[] { null, null, null, null, null, null, null, null },  
        new Block[] { null, null, null, null, null, null, null, null },
        new Block[] { null, null, null, null, null, null, null, null },
        new Block[] { null, null, null, null, null, null, null, null },
        new Block[] { null, null, null, null, null, null, null, null },
        new Block[] { null, null, null, null, null, null, null, null },
        new Block[] { null, null, null, null, null, null, null, null },
        new Block[] { null, null, null, null, null, null, null, null },
    };


    [SerializeField] GameObject blockPrefab;

    void Awake() {
        drawBoard();
    }

    

    public void drawBoard() {
        for( int i = 0; i < EditorConfig.MAX_NUM_Y; i++ ) {

            for( int j = 0; j < EditorConfig.MAX_NUM_X; j++ ) {
                GameObject blockObj = Instantiate( blockPrefab, transform );
                Transform blockTransform = blockObj.GetComponent< Transform >();
                blockTransform.SetParent( gameObject.transform );
                blockTransform.localPosition = new Vector3( EditorConfig.SPACE_X * j + EditorConfig.BLOCK_GAP * j, EditorConfig.SPACE_Y * i + EditorConfig.BLOCK_GAP * i, 0 );
                Block block = blockObj.GetComponent< Block >();
                block.SetColor( BlockColor.DARK_GRAY );
                //blockList.Add( block );

            }
        }
    }

    public void Place( int xGrid, int yGrid, Vector2Int[] type ) {
        if( this.IsPlaceable( xGrid, yGrid, type ) == false ) return;
        for( int i = 0; i < type.Length; i++ ) {
            Vector2Int block = type[ i ];
            int blockX = xGrid + block.x;
            int blockY = yGrid + block.y;
            this.state[ blockY ][ blockX ] = 1;
            this.buildingList[ blockY ][ blockX ] =  null;
                // vo 제작 후 type 을 building 으로 변경할것
        }
        return;
    }

    public bool IsPlaceable( int xGrid, int yGrid, Vector2Int[] type ) {
        for( int i = 0; i < type.Length; i++ ) {
            Vector2Int block = type[ i ];
            int blockX = xGrid + block.x;
            int blockY = yGrid + block.y;
            if( blockX < EditorConfig.MAX_NUM_X && blockX >= 0
            && blockY < EditorConfig.MAX_NUM_Y && blockY >= 0  ) {
                if( this.state[ blockY ][ blockX ] == 0 ) {
                    return false;
                }
            } else {
                return false;
            }
        }
        return true;
    }

    public void Remove( int xGrid, int yGrid, Vector2Int[] type ) {

    }

    public void UpdateColor( int xGrid, int yGrid, Vector2Int[] type ) {
        for( int i = 0; i < type.Length; i++ ) {
            Vector2Int block = type[ i ];
            int blockX = xGrid + block.x;
            int blockY = yGrid + block.y;
            if( blockX <= EditorConfig.MAX_NUM_X && blockX > 0
            && blockY <= EditorConfig.MAX_NUM_Y && blockY > 0 ) {
                Block updateBlock = blockList[ blockY ][ blockX ];
                updateBlock.SetColor( BlockColor.RED );

            }
        }
    }




    

}
