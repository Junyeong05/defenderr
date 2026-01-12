using UnityEngine;
using UnityEngine.InputSystem;

// Editor 레이어 - 단순 컨테이너
public class EditorLayer : MonoBehaviour
{
    private static EditorLayer instance;
    
    public static EditorLayer Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("EditorLayer");
                instance = go.AddComponent<EditorLayer>();
            
            }
            return instance;
        }
    }
    
    void Awake() {
        SetBoard();
    }

    private GameObject boardPrefab;
    private GameObject boardObj;
    private Board board;

    private bool isDragging = false;
    private GameObject draggingBlock;
    private float xOffset = 0;
    private float yOffset = 0;

    public void SetBoard() {
        boardPrefab = Resources.Load<GameObject>("Prefabs/BuildingEditor/Board");
        boardObj = Instantiate(boardPrefab);
        boardObj.transform.SetParent( transform );  // Instance 대신 transform 사용!

        boardObj.transform.localPosition = new Vector3( EditorConfig.START_X, EditorConfig.START_Y );
        board = boardObj.GetComponent< Board >();
    }

    void Update() {

        if( Input.GetMouseButtonDown( 0 ) ) {
            Vector3 clickPosition = Camera.main.ScreenToWorldPoint( Input.mousePosition );
            clickPosition.z = 0;

            Debug.Log( clickPosition );
            RaycastHit2D hit = Physics2D.Raycast( clickPosition, Vector2.zero );

            if( hit.collider != null ) {
                isDragging = true;
                draggingBlock = hit.collider.gameObject;
                xOffset = clickPosition.x - draggingBlock.transform.position.x;
                yOffset = clickPosition.y - draggingBlock.transform.position.y;
            }
        }

        if( Input.GetMouseButton( 0 ) && isDragging == true ) {
            Debug.Log( "dragging" );
            Vector3 clickPosition = Camera.main.ScreenToWorldPoint( Input.mousePosition );
            clickPosition.z = 0;

            Vector3 blockPosition = Vector3.zero;

            blockPosition.x = clickPosition.x - xOffset;
            blockPosition.y = clickPosition.y - yOffset;
            
            draggingBlock.transform.position = blockPosition;
        }

        if( Input.GetMouseButtonUp( 0 ) ) {
            isDragging = false;
        }

    }
}
/*
public enum EditingMod {
    NONE = 0;
    EDITING = 1;
}
*/