using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class Scene1 : MonoBehaviour
{
    [Header("Battle Controller")]
    [SerializeField] private BattleController battleController;
    
    [Header("Hero Factory Configuration")]
    [SerializeField] private HeroCatalog heroCatalog; // Inspector에서 할당
    
    [Header("Debug Line Settings")]
    [SerializeField] private bool showDebugLine = true;
    [SerializeField] private Color lineColor = Color.red;
    [SerializeField] private float lineWidth = 0.1f;
    
    [Header("Battle Configuration")]
    [SerializeField] private float battleSpeed = 0.7f;
    
    // 디버그 라인용 GameObject
    private GameObject debugLine;
    
    void Start()
    {
        // y=0 디버그 라인 생성
        if (showDebugLine)
        {
            CreateDebugLine();
        }
        
        // BattleController 찾기 또는 생성
        if (battleController == null)
        {
            battleController = BattleController.Instance;
        }
        
        // BattleController 초기화 및 전투 시작
        InitializeBattle();
    }
    
    /// <summary>
    /// 전투 초기화 및 시작
    /// </summary>
    private void InitializeBattle()
    {
        if (heroCatalog == null)
        {
            Debug.LogError("[Scene1] HeroCatalog not assigned!");
            return;
        }
        
        // BattleController 설정
        battleController.InitializeBattle(heroCatalog);
        battleController.SetBattleSpeed(battleSpeed);
        
        // 전투 시작
        battleController.StartBattle();
        
        Debug.Log($"[Scene1] Battle started with speed: {battleSpeed}");
    }
    
    // 기존 메서드는 BattleController로 이동됨
    // CreateHeroesUsingFactory는 더 이상 사용하지 않음
    
    /// <summary>
    /// 키보드 입력 처리 (새로운 Input System 사용)
    /// </summary>
    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // 스페이스바로 전투 일시정지/재개
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            if (battleController.IsPaused)
            {
                battleController.ResumeBattle();
            }
            else
            {
                battleController.PauseBattle();
            }
        }
        
        // R키로 전투 재시작
        if (keyboard.rKey.wasPressedThisFrame)
        {
            battleController.StopBattle();
            InitializeBattle();
        }
        
        // ESC키로 전투 중지
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            battleController.StopBattle();
        }
    }
    
    // y=0 디버그 라인 (LineRenderer)
    private void CreateDebugLine()
    {
        debugLine = new GameObject("Debug Line Y=0");
        LineRenderer lineRenderer = debugLine.AddComponent<LineRenderer>();
        
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        
        Vector3[] positions = new Vector3[2];
        positions[0] = new Vector3(-500, 0, 0);
        positions[1] = new Vector3(500, 0, 0);
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(positions);
        lineRenderer.sortingOrder = -1;
    }
    
    // 대안: Cube로 디버그 라인
    private void CreateDebugLineWithCube()
    {
        debugLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
        debugLine.name = "Debug Line Y=0";
        debugLine.transform.position = new Vector3(0, 0, 0);
        debugLine.transform.localScale = new Vector3(1000, lineWidth, 0.01f);
        
        Renderer renderer = debugLine.GetComponent<Renderer>();
        renderer.material.color = lineColor;
        Destroy(debugLine.GetComponent<Collider>());
    }
    
    public void ToggleDebugLine()
    {
        if (debugLine != null)
        {
            debugLine.SetActive(!debugLine.activeSelf);
        }
    }
    
    // Editor Gizmo
    void OnDrawGizmos()
    {
        if (showDebugLine)
        {
            Gizmos.color = lineColor;
            Gizmos.DrawLine(new Vector3(-500, 0, 0), new Vector3(500, 0, 0));
        }
    }
    
}
