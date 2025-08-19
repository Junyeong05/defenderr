using UnityEngine;

// 게임의 진입점 - 씬에 배치되어 GameLayer를 초기화
public class GameMain : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
   
    void Awake()
    {
        if (showDebugInfo)
            Debug.Log("[GameMain] Game Starting...");
        
        // 1. 프레임 설정
        Application.targetFrameRate = 60;
        
        // 2. GameLayer 초기화
        InitializeGameLayer();
        InitializeBattleController();
        
        if (showDebugInfo)
            Debug.Log("[GameMain] Awake Complete");
    }
    
    void Start()
    {
        if (showDebugInfo)
            Debug.Log("[GameMain] Start - GameLayer initialized");
        
        // 필요한 추가 초기화는 사용자가 결정하여 추가
        // 예: BattleController, Factory 시스템 등
    }
    
    void InitializeGameLayer()
    {
        // GameLayer 생성 및 확인
        if (GameLayer.Instance != null)
        {
            // GameLayer를 씬의 루트로 설정 (GameMain의 자식이 아닌)
            GameLayer.Instance.transform.SetParent(null);
            GameLayer.Instance.transform.localPosition = Vector3.zero;
            
            if (showDebugInfo)
            {
                Debug.Log("[GameMain] GameLayer initialized");
                Debug.Log($"  - BackgroundLayer: {BackgroundLayer.Instance != null}");
                Debug.Log($"  - UnitLayer: {UnitLayer.Instance != null}");
                Debug.Log($"  - WeaponLayer: {WeaponLayer.Instance != null}");
                Debug.Log($"  - EffectLayer: {EffectLayer.Instance != null}");
                Debug.Log($"  - UILayer: {UILayer.Instance != null}");
                Debug.Log($"  - PopupLayer: {PopupLayer.Instance != null}");
            }
        }
        else
        {
            Debug.LogError("[GameMain] Failed to initialize GameLayer!");
        }
    }

    void InitializeBattleController()
    {
        HeroCatalog heroCatalog = Resources.Load<HeroCatalog>("HeroData/HeroCatalog");
        BattleController.Instance.InitializeBattle(heroCatalog);
        BattleController.Instance.SetBattleSpeed( 0.7f);
        BattleController.Instance.StartBattle();
    }
    
    // Inspector에서 테스트용 버튼
    [ContextMenu("Print Layer Status")]
    void PrintLayerStatus()
    {
        Debug.Log("=== Layer Status ===");
        Debug.Log($"BackgroundLayer active: {BackgroundLayer.Instance?.gameObject.activeSelf}");
        Debug.Log($"UnitLayer active: {UnitLayer.Instance?.gameObject.activeSelf}");
        Debug.Log($"  - Children: {UnitLayer.Instance?.transform.childCount}");
        Debug.Log($"WeaponLayer active: {WeaponLayer.Instance?.gameObject.activeSelf}");
        Debug.Log($"  - Children: {WeaponLayer.Instance?.transform.childCount}");
        Debug.Log($"EffectLayer active: {EffectLayer.Instance?.gameObject.activeSelf}");
        Debug.Log($"UILayer active: {UILayer.Instance?.gameObject.activeSelf}");
    }
}