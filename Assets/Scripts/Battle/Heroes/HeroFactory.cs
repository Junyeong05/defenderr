using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 영웅 생성 및 객체풀 관리 팩토리
/// AS3.0 스타일의 getHero 인터페이스 제공
/// </summary>
public class HeroFactory : MonoBehaviour
{
    #region Singleton
    private static HeroFactory instance;
    public static HeroFactory Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("HeroFactory");
                instance = go.AddComponent<HeroFactory>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
    #endregion
    
    [Header("Configuration")]
    [SerializeField] private HeroCatalog heroCatalog;
    
    [Header("Pool Management")]
    [SerializeField] private Transform poolContainer; // 비활성 영웅들을 담을 컨테이너
    
    // 객체풀 - 타입별로 Queue 관리
    private Dictionary<string, Queue<BaseHero>> heroPools = new Dictionary<string, Queue<BaseHero>>();
    
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        Initialize();
    }
    
    /// <summary>
    /// 팩토리 초기화
    /// </summary>
    private void Initialize()
    {
        // 풀 컨테이너 생성
        if (poolContainer == null)
        {
            GameObject container = new GameObject("HeroPoolContainer");
            container.transform.SetParent(transform);
            poolContainer = container.transform;
        }
        
        // 카탈로그 초기화
        if (heroCatalog != null)
        {
            heroCatalog.Initialize();
        }
        // 카탈로그는 나중에 SetCatalog()으로 설정될 수 있으므로 경고 제거
    }
    
    /// <summary>
    /// 카탈로그 설정 (런타임에 변경 가능)
    /// </summary>
    public void SetCatalog(HeroCatalog catalog)
    {
        heroCatalog = catalog;
        if (catalog != null)
        {
            catalog.Initialize();
        }
    }
    
    /// <summary>
    /// AS3.0 스타일 - 영웅 가져오기
    /// </summary>
    public BaseHero GetHero(string heroType, HeroData heroData = null, int level = 1)
    {
        if (heroCatalog == null)
        {
            Debug.LogError("[HeroFactory] No hero catalog assigned!");
            return null;
        }
        
        // 1. 풀이 없으면 생성
        if (!heroPools.ContainsKey(heroType))
        {
            heroPools[heroType] = new Queue<BaseHero>();
        }
        
        BaseHero hero = null;
        
        // 2. 풀에서 가져오기 시도
        if (heroPools[heroType].Count > 0)
        {
            hero = heroPools[heroType].Dequeue();
            hero.gameObject.SetActive(true);
            hero.transform.SetParent(null); // 풀 컨테이너에서 빼내기
        }
        else
        {
            // 3. 없으면 새로 생성
            GameObject prefab = heroCatalog.GetPrefab(heroType);
            if (prefab == null)
            {
                Debug.LogError($"[HeroFactory] Prefab not found for hero type: {heroType}");
                return null;
            }
            
            GameObject heroObj = Instantiate(prefab);
            hero = heroObj.GetComponent<BaseHero>();
            
            if (hero == null)
            {
                Debug.LogError($"[HeroFactory] BaseHero component not found on prefab: {heroType}");
                Destroy(heroObj);
                return null;
            }
            
            // 팩토리 참조 설정
            hero.SetFactory(this);
        }
        
        // 4. 데이터 설정 (재사용 가능하도록)
        if (heroData != null)
        {
            hero.SetData(heroData, level);
        }
        else
        {
            // heroData가 없으면 카탈로그에서 기본 데이터 가져오기
            HeroData defaultData = heroCatalog.GetData(heroType);
            if (defaultData != null)
            {
                hero.SetData(defaultData, level);
            }
        }
        
        return hero;
    }
    
    /// <summary>
    /// 영웅을 풀로 반환
    /// </summary>
    public void ReturnHero(BaseHero hero)
    {
        if (hero == null) return;
        
        string heroType = hero.GetType().Name;
        
        // 상태 초기화는 SetData에서 자동으로 처리됨
        
        // 비활성화 및 풀 컨테이너로 이동
        hero.gameObject.SetActive(false);
        hero.transform.SetParent(poolContainer);
        hero.transform.position = Vector3.zero;
        
        // 풀에 추가
        if (!heroPools.ContainsKey(heroType))
        {
            heroPools[heroType] = new Queue<BaseHero>();
        }
        heroPools[heroType].Enqueue(hero);
    }
    
    /// <summary>
    /// 특정 타입의 풀 크기 가져오기
    /// </summary>
    public int GetPoolSize(string heroType)
    {
        if (heroPools.ContainsKey(heroType))
        {
            return heroPools[heroType].Count;
        }
        return 0;
    }
    
    /// <summary>
    /// 모든 풀 초기화 (씬 전환 시 사용)
    /// </summary>
    public void ClearAllPools()
    {
        // 활성 영웅들 강제 반환
        BaseHero[] activeHeroes = GameObject.FindObjectsByType<BaseHero>(FindObjectsSortMode.None);
        foreach (var hero in activeHeroes)
        {
            if (hero != null)
            {
                hero.Remove();
            }
        }
        
        // 풀에 있는 모든 객체 제거
        foreach (var pool in heroPools.Values)
        {
            while (pool.Count > 0)
            {
                var hero = pool.Dequeue();
                if (hero != null && hero.gameObject != null)
                {
                    Destroy(hero.gameObject);
                }
            }
        }
        
        heroPools.Clear();
    }
    
    void OnDestroy()
    {
        if (instance == this)
        {
            ClearAllPools();
            instance = null;
        }
    }
}