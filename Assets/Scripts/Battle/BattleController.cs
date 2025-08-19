using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// 전투를 관리하는 컨트롤러
// 아군/적군 생성, 전투 시작/중지, 프레임별 업데이트 관리
public class BattleController : MonoBehaviour
{
    #region Singleton
    private static BattleController instance;
    public static BattleController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<BattleController>();
                if (instance == null)
                {
                    GameObject go = new GameObject("BattleController");
                    instance = go.AddComponent<BattleController>();
                }
            }
            return instance;
        }
    }
    #endregion

    #region Fields
    [Header("Battle Configuration")]
    [SerializeField] private HeroCatalog heroCatalog;
    [SerializeField] private int playerUnitCount = 1;
    [SerializeField] private int enemyUnitCount = 1;
    
    [Header("Battle State")]
    [SerializeField] private bool isBattleActive = false;
    [SerializeField] private bool isPaused = false;
    
    // 유닛 리스트
    private List<BaseHero> playerUnits = new List<BaseHero>();
    private List<BaseHero> enemyUnits = new List<BaseHero>();
    private List<BaseHero> allUnits = new List<BaseHero>();
    
    // 전투 결과
    public enum BattleResult
    {
        InProgress = -1,
        PlayerWin = 1,
        EnemyWin = 2,
        Draw = 0
    }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // GameMain에서 모든 Factory 초기화를 처리하므로 여기서는 하지 않음
        // Factory 설정은 GameMain.InitializeFactories()에서 처리
        
        // 필요한 경우 BattleController 고유의 초기화 코드만 추가
        
        // BattleController를 FrameController에 한 번만 등록
        FrameController.Add(OnFrame, this);
    }
    
    private void OnDestroy()
    {
        // FrameController에서 제거
        if (FrameController.Instance != null)
        {
            FrameController.Remove(OnFrame, this);
        }
    }
    #endregion
    
    #region Frame Update
    // 매 프레임마다 호출되는 메서드 - 모든 유닛의 Execute() 호출
    private void OnFrame()
    {
        if (!isBattleActive || isPaused) return;
        
        // 아군 유닛 업데이트
        for (int i = playerUnits.Count - 1; i >= 0; i--)
        {
            if (playerUnits[i] != null && playerUnits[i].IsAlive)
            {
                playerUnits[i].Execute();
            }
            else if (playerUnits[i] != null && !playerUnits[i].IsAlive)
            {
                // 죽은 유닛 처리
                RemoveDeadUnit(playerUnits[i]);
            }
        }
        
        // 적군 유닛 업데이트
        for (int i = enemyUnits.Count - 1; i >= 0; i--)
        {
            if (enemyUnits[i] != null && enemyUnits[i].IsAlive)
            {
                enemyUnits[i].Execute();
            }
            else if (enemyUnits[i] != null && !enemyUnits[i].IsAlive)
            {
                // 죽은 유닛 처리
                RemoveDeadUnit(enemyUnits[i]);
            }
        }
        
        // 모든 유닛의 Execute() 완료 후 Y 좌표 기준으로 깊이 정렬
        UpdateAllUnitsDepth();
    }
    
    // 모든 유닛을 Y 좌표로 정렬하여 깊이 일괄 업데이트
    private void UpdateAllUnitsDepth()
    {
        // 살아있는 모든 유닛을 Y 좌표로 정렬 (Y가 큰 것부터 = 위쪽부터)
        var sortedUnits = allUnits
            .Where(unit => unit != null && unit.IsAlive)
            .OrderByDescending(unit => unit.transform.position.y)
            .ToList();
        
        // 정렬된 순서대로 sortingOrder 할당
        // 위쪽(Y가 큰) 유닛부터 0, 1, 2... 순서로 할당
        // 아래쪽 유닛일수록 큰 값 = 앞에 그려짐
        for (int i = 0; i < sortedUnits.Count; i++)
        {
            sortedUnits[i].SetSortingOrder(i);
        }
    }
    #endregion

    #region Public Methods
    // 전투 초기화 및 유닛 생성
    public void InitializeBattle(HeroCatalog catalog = null)
    {
        if (catalog != null)
        {
            heroCatalog = catalog;
            HeroFactory.Instance.SetCatalog(heroCatalog);
        }

        if (heroCatalog == null)
        {
            Debug.LogError("[BattleController] HeroCatalog not assigned!");
            return;
        }
        
        // WeaponFactory 초기화 확인
        WeaponCatalog weaponCatalog = Resources.Load<WeaponCatalog>("WeaponData/WeaponCatalog");
        if (weaponCatalog != null)
        {
            WeaponFactory.Instance.SetCatalog(weaponCatalog);
            Debug.Log("[BattleController] WeaponCatalog set to WeaponFactory in InitializeBattle");
        }

        ResetBattle();
        GeneratePlayerUnits();
        GenerateEnemyUnits();
        SetupBattleLists();
        
        Debug.Log($"[BattleController] Battle initialized with {playerUnits.Count} player units and {enemyUnits.Count} enemy units");
    }

    // 아군 유닛 생성
    public void GeneratePlayerUnits()
    {
        // 기존 유닛들을 먼저 정리
        foreach (var unit in playerUnits)
        {
            if (unit != null)
            {
                // FrameController 제거 불필요 - BattleController의 OnFrame에서 처리
            }
        }
        
        playerUnits.Clear();

        HeroData elfArcherData = heroCatalog.GetData("ElfArcher1");
        
        // 디버그 로그 추가
        if (elfArcherData != null)
        {
            Debug.Log($"[BattleController] ElfArcher1 data - isRanged: {elfArcherData.isRanged}, weaponClass: {elfArcherData.weaponClass}, range: {elfArcherData.attackRange}");
        }
        
        for (int i = 0; i < playerUnitCount; i++)
        {
            BaseHero unit = HeroFactory.Instance.GetHero("ElfArcher1", elfArcherData, 1);
            if (unit != null)
            {
                // 위치 설정 (하단 배치)
                float xPos = Random.Range(-300f, 300f);
                float yPos = Random.Range(-300f, -500f);
                unit.transform.position = new Vector3(xPos, yPos, 0);
                unit.SetSize(1.25f);
                
                // 기본 타겟 위치 설정 (위쪽으로 이동)
                unit.SetDefaultTargetPosition(new Vector2(xPos, 400));
                
                playerUnits.Add(unit);
                allUnits.Add(unit);
                
                // FrameController 등록 제거 - BattleController의 OnFrame에서 처리
            }
        }
        
        Debug.Log($"[BattleController] Generated {playerUnits.Count} player units");
    }

    // 적군 유닛 생성
    public void GenerateEnemyUnits()
    {
        // 기존 유닛들을 먼저 정리
        foreach (var unit in enemyUnits)
        {
            if (unit != null)
            {
                // FrameController 제거 불필요 - BattleController의 OnFrame에서 처리
            }
        }
        
        enemyUnits.Clear();

        HeroData footManData = heroCatalog.GetData("FootMan1");
        
        // 디버그 로그 추가
        if (footManData != null)
        {
            Debug.Log($"[BattleController] FootMan1 data - isRanged: {footManData.isRanged}, weaponClass: {footManData.weaponClass}, range: {footManData.attackRange}");
        }
        
        for (int i = 0; i < enemyUnitCount; i++)
        {
            BaseHero unit = HeroFactory.Instance.GetHero("FootMan1", footManData, 1);
            if (unit != null)
            {
                // 위치 설정 (상단 배치)
                float xPos = Random.Range(-300f, 300f);
                float yPos = Random.Range(300f, 500f);
                unit.transform.position = new Vector3(xPos, yPos, 0);
                unit.SetSize(1.25f);
                
                // 기본 타겟 위치 설정 (아래쪽으로 이동)
                unit.SetDefaultTargetPosition(new Vector2(xPos, -400));
                
                enemyUnits.Add(unit);
                allUnits.Add(unit);
                
                // FrameController 등록 제거 - BattleController의 OnFrame에서 처리
            }
        }
        
        Debug.Log($"[BattleController] Generated {enemyUnits.Count} enemy units");
    }

    // 전투 시작
    public void StartBattle()
    {
        if (playerUnits.Count == 0 || enemyUnits.Count == 0)
        {
            Debug.LogWarning("[BattleController] Cannot start battle - no units generated!");
            return;
        }

        isBattleActive = true;
        isPaused = false;
        
        // 모든 유닛을 대기 상태로 설정
        foreach (var unit in allUnits)
        {
            unit.SetState(BaseHero.STATE_WAIT);
        }
        
        // FrameController 시작
        FrameController.Play();
        
        Debug.Log("[BattleController] Battle started!");
    }

    // 전투 일시정지
    public void PauseBattle()
    {
        isPaused = true;
        FrameController.Stop();
        Debug.Log("[BattleController] Battle paused");
    }

    // 전투 재개
    public void ResumeBattle()
    {
        if (isBattleActive)
        {
            isPaused = false;
            FrameController.Play();
            Debug.Log("[BattleController] Battle resumed");
        }
    }

    // 전투 중지
    public void StopBattle()
    {
        isBattleActive = false;
        isPaused = false;
        FrameController.Stop();
        
        // 모든 유닛 정리
        CleanupUnits();
        
        Debug.Log("[BattleController] Battle stopped");
    }

    // 전투 결과 확인
    public BattleResult CheckBattleResult()
    {
        // 모든 적군이 제거됐는지 확인
        if (enemyUnits.Count == 0 && playerUnits.Count > 0)
            return BattleResult.PlayerWin;
        
        // 모든 아군이 제거됐는지 확인
        if (playerUnits.Count == 0 && enemyUnits.Count > 0)
            return BattleResult.EnemyWin;
        
        // 둘 다 없으면 무승부
        if (playerUnits.Count == 0 && enemyUnits.Count == 0)
            return BattleResult.Draw;
        
        return BattleResult.InProgress;
    }

    // 전투 속도 설정
    public void SetBattleSpeed(float speed)
    {
        FrameController.SetSpeed(speed);
    }
    #endregion

    #region Private Methods
    // 전투 초기화
    private void ResetBattle()
    {
        // 먼저 모든 유닛 정리 (FrameController에서 제거)
        CleanupUnits();
        
        // 전투 중지
        StopBattle();
        
        playerUnits.Clear();
        enemyUnits.Clear();
        allUnits.Clear();
        
        Debug.Log("[BattleController] Battle reset - all units and events cleared");
    }

    // 전투 리스트 설정
    private void SetupBattleLists()
    {
        BaseHero[] playerArray = playerUnits.ToArray();
        BaseHero[] enemyArray = enemyUnits.ToArray();
        
        // 서로를 적으로 인식하도록 설정
        BaseHero.SetBattleLists(playerArray, enemyArray);
    }

    // 유닛 정리
    private void CleanupUnits()
    {
        // FrameController에서 제거
        foreach (var unit in allUnits)
        {
            if (unit != null)
            {
                // FrameController 제거 불필요 - BattleController의 OnFrame에서 처리
                // Die() 메서드가 없으므로 직접 GameObject 비활성화
                unit.gameObject.SetActive(false);
                // 또는 Factory로 반환
                if (HeroFactory.Instance != null)
                {
                    HeroFactory.Instance.ReturnHero(unit);
                }
            }
        }
        
        playerUnits.Clear();
        enemyUnits.Clear();
        allUnits.Clear();
    }

    // 죽은 유닛 제거
    public void RemoveDeadUnit(BaseHero unit)
    {
        if (unit == null) return;
        
        // 리스트에서 제거
        playerUnits.Remove(unit);
        enemyUnits.Remove(unit);
        allUnits.Remove(unit);
        
        // FrameController 제거 불필요 - OnFrame에서 리스트 순회로 처리
        
        // Factory로 반환
        if (HeroFactory.Instance != null)
        {
            HeroFactory.Instance.ReturnHero(unit);
        }
        
        // 전투 결과 확인
        BattleResult result = CheckBattleResult();
        if (result != BattleResult.InProgress)
        {
            OnBattleEnd(result);
        }
    }

    // 전투 종료 처리
    private void OnBattleEnd(BattleResult result)
    {
        StopBattle();
        
        switch (result)
        {
            case BattleResult.PlayerWin:
                Debug.Log("[BattleController] Player Victory!");
                break;
            case BattleResult.EnemyWin:
                Debug.Log("[BattleController] Enemy Victory!");
                break;
            case BattleResult.Draw:
                Debug.Log("[BattleController] Draw!");
                break;
        }
    }
    #endregion

    #region Properties
    public List<BaseHero> PlayerUnits => playerUnits;
    public List<BaseHero> EnemyUnits => enemyUnits;
    public List<BaseHero> AllUnits => allUnits;
    public bool IsBattleActive => isBattleActive;
    public bool IsPaused => isPaused;
    #endregion
}