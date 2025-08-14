using UnityEngine;

public class Scene1 : MonoBehaviour
{
    [Header("Hero Factory Configuration")]
    [SerializeField] private HeroCatalog heroCatalog; // Inspector에서 할당
    
    [Header("Debug Line Settings")]
    [SerializeField] private bool showDebugLine = true;
    [SerializeField] private Color lineColor = Color.red;
    [SerializeField] private float lineWidth = 0.1f;
    
    // 생성된 영웅들 참조
    private BaseHero elfArcher;
    private BaseHero footMan;
    
    // 디버그 라인용 GameObject
    private GameObject debugLine;
    
    void Start()
    {
        // FrameController는 Main Camera에 자동으로 추가됨
        
        // y=0 디버그 라인 생성
        if (showDebugLine)
        {
            CreateDebugLine();
        }
        
        // HeroFactory에 카탈로그 설정
        if (heroCatalog != null)
        {
            HeroFactory.Instance.SetCatalog(heroCatalog);
        }
        else
        {
            Debug.LogError("[Scene1] HeroCatalog not assigned!");
            return;
        }
        
        // HeroFactory를 사용하여 영웅 생성
        CreateHeroesUsingFactory();
        
        // 디버그 목적으로만 프레임 체크 (영웅들은 자체적으로 FrameController에 등록됨)
        FrameController.SetSpeed(.7f);
        FrameController.Add(this.onPlayAnimation, this);
    }
    
    /// <summary>
    /// HeroFactory를 사용하여 영웅들 생성
    /// </summary>
    private void CreateHeroesUsingFactory()
    {
        // ElfArcher 생성 - GetHero에 데이터와 레벨 전달
        elfArcher = HeroFactory.Instance.GetHero("ElfArcher1", heroCatalog.GetData("ElfArcher1"), 1);
        if (elfArcher != null)
        {
            elfArcher.transform.position = new Vector3(-122, 0, 0);
            // ElfArcher는 적 팀으로 설정 (footMan이 공격할 수 있도록)
            elfArcher.SetDefaultTargetPosition(new Vector2(200, 0)); // 오른쪽으로 이동
        }
        
        // FootMan 생성 - GetHero에 데이터와 레벨 전달
        footMan = HeroFactory.Instance.GetHero("FootMan1", heroCatalog.GetData("FootMan1"), 1);
        if (footMan != null)
        {
            footMan.transform.position = new Vector3(122, 0, 0);
            footMan.SetSize(1.2f);
            // 기본 목표 위치 설정 (적이 없을 때 갈 곳)
            footMan.SetDefaultTargetPosition(new Vector2(-200, 0));
        }
        
        // 전투 리스트 설정 - FootMan과 ElfArcher를 서로 다른 팀으로 설정
        BaseHero[] playerTeam = new BaseHero[] { footMan };
        BaseHero[] enemyTeam = new BaseHero[] { elfArcher };
        
        // 전투 리스트 설정 (이제 서로를 적으로 인식)
        BaseHero.SetBattleLists(playerTeam, enemyTeam);
        
        // 전투 시작 - footMan이 elfArcher를 공격하도록
        if (footMan != null && elfArcher != null)
        {
            // SetTarget을 사용하여 직접 타겟 지정
            footMan.SetTarget(elfArcher);  // BaseHero 오버로드 사용
            
            // ElfArcher도 FootMan을 타겟으로 설정 (서로 싸우도록)
            elfArcher.SetTarget(footMan);
            
            // 초기 방향 설정 (타겟을 바라보도록)
            footMan.UpdateFacing(elfArcher.transform.position.x - footMan.transform.position.x);
            elfArcher.UpdateFacing(footMan.transform.position.x - elfArcher.transform.position.x);
            
            // 대기 상태로 시작 (자동으로 적을 찾아서 이동/공격)
            // STATE_MOVE를 직접 설정하지 않고, DoWait()에서 자연스럽게 전환되도록
            footMan.SetState(BaseHero.STATE_WAIT);
            elfArcher.SetState(BaseHero.STATE_WAIT);
            
            Debug.Log($"[Scene1] Battle started between FootMan and ElfArcher.");
        }
    }
    
    // 미사용 헬퍼 메서드
    private BaseHero CreateAndSetupHero(string heroType, Vector3 position, int level)
    {
        BaseHero hero = HeroFactory.Instance.GetHero(heroType);
        if (hero != null)
        {
            // 데이터 설정
            HeroData data = heroCatalog.GetData(heroType);
            if (data != null)
            {
                hero.SetData(data, level);
            }
            hero.transform.position = position;
        }
        return hero;
    }
    
    private void onPlayAnimation()
    {
        // 디버그용 프레임 체크
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
