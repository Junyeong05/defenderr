using UnityEngine;

// PixiJS 스타일의 메인 게임 레이어 컨테이너
public class GameLayer : MonoBehaviour
{
    private static GameLayer instance;
    
    public static GameLayer Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("GameLayer");
                instance = go.AddComponent<GameLayer>();
                instance.Initialize();
                // GameManager가 DontDestroyOnLoad를 처리하므로 여기서는 하지 않음
            }
            return instance;
        }
    }
    
    void Initialize()
    {
        // 레이어들을 순서대로 추가 (아래부터 위로 렌더링)
        BackgroundLayer.Instance.transform.SetParent(transform);
        BackgroundLayer.Instance.transform.SetSiblingIndex(0);

        EditorLayer.Instance.transform.SetParent( transform );
        EditorLayer.Instance.transform.SetSiblingIndex( 1 );
        EditorLayer.Instance.transform.localScale = new Vector3( EditorConfig.EDITOR_SCALE, EditorConfig.EDITOR_SCALE, 1f );
        
        GroundEffectLayer.Instance.transform.SetParent(transform);
        GroundEffectLayer.Instance.transform.SetSiblingIndex(2);
        
        UnitLayer.Instance.transform.SetParent(transform);
        UnitLayer.Instance.transform.SetSiblingIndex(3);
        
        WeaponLayer.Instance.transform.SetParent(transform);
        WeaponLayer.Instance.transform.SetSiblingIndex(4);
        
        EffectLayer.Instance.transform.SetParent(transform);
        EffectLayer.Instance.transform.SetSiblingIndex(5);
        
        UILayer.Instance.transform.SetParent(transform);
        UILayer.Instance.transform.SetSiblingIndex(6);
        
        PopupLayer.Instance.transform.SetParent(transform);
        PopupLayer.Instance.transform.SetSiblingIndex(7);


    }
}