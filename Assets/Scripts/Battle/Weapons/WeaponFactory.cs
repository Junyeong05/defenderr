using UnityEngine;
using System.Collections.Generic;

// 무기 팩토리 - 싱글톤 패턴 + 오브젝트 풀링
public class WeaponFactory : MonoBehaviour
{
    #region Singleton
    private static WeaponFactory instance;
    public static WeaponFactory Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<WeaponFactory>();
                if (instance == null)
                {
                    GameObject go = new GameObject("WeaponFactory");
                    instance = go.AddComponent<WeaponFactory>();
                }
            }
            return instance;
        }
    }
    #endregion
    
    #region Fields
    [Header("Configuration")]
    [SerializeField] private WeaponCatalog weaponCatalog;
    [SerializeField] private Transform poolContainer;
    [SerializeField] private int maxPoolSize = 100;  // 각 무기 타입별 최대 풀 크기
    
    // 오브젝트 풀
    private Dictionary<string, Queue<BaseWeapon>> weaponPools = new Dictionary<string, Queue<BaseWeapon>>();
    private Dictionary<string, WeaponData> weaponDataCache = new Dictionary<string, WeaponData>();
    private Dictionary<string, int> activeWeaponCounts = new Dictionary<string, int>();
    #endregion
    
    #region Unity Lifecycle
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePools();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // 풀 컨테이너 생성
        if (poolContainer == null)
        {
            GameObject container = new GameObject("WeaponPool");
            container.transform.SetParent(transform);
            poolContainer = container.transform;
        }
    }
    #endregion
    
    #region Initialization
    public void SetCatalog(WeaponCatalog catalog)
    {
        weaponCatalog = catalog;
        InitializePools();
    }
    
    private void InitializePools()
    {
        // Lazy Loading 방식 - 미리 생성하지 않음
        // 필요할 때 GetWeapon에서 생성
        // 카탈로그 초기화만 수행
        if (weaponCatalog != null)
        {
            weaponCatalog.Initialize();
        }
    }
    
    private void InitializePool(string weaponClass, WeaponData data)
    {
        if (!weaponPools.ContainsKey(weaponClass))
        {
            weaponPools[weaponClass] = new Queue<BaseWeapon>();
            weaponDataCache[weaponClass] = data;
            activeWeaponCounts[weaponClass] = 0;
            
            // Lazy Loading - 초기 풀 생성하지 않음
            // 첫 요청 시 생성됨
        }
    }
    #endregion
    
    #region Weapon Management
    public BaseWeapon GetWeapon(string weaponClass, BaseHero owner, BaseHero target, float damagePercent = 1f)
    {
        if (string.IsNullOrEmpty(weaponClass))
        {
            Debug.LogError("[WeaponFactory] Weapon class is null or empty!");
            return null;
        }
        
        // 풀이 없으면 생성
        if (!weaponPools.ContainsKey(weaponClass))
        {
            WeaponData data = GetWeaponData(weaponClass);
            if (data == null)
            {
                Debug.LogError($"[WeaponFactory] No WeaponData found for {weaponClass}");
                return null;
            }
            InitializePool(weaponClass, data);
        }
        
        BaseWeapon weapon = null;
        
        // 풀에서 가져오기
        if (weaponPools[weaponClass].Count > 0)
        {
            weapon = weaponPools[weaponClass].Dequeue();
        }
        else
        {
            // 풀이 비어있으면 새로 생성
            WeaponData data = weaponDataCache[weaponClass];
            weapon = CreateNewWeapon(weaponClass, data, false);
        }
        
        if (weapon != null)
        {
            // 활성화 및 설정
            weapon.gameObject.SetActive(true);
            weapon.SetupWeapon(owner, target, damagePercent);
            
            // 활성 카운트 증가
            if (!activeWeaponCounts.ContainsKey(weaponClass))
            {
                activeWeaponCounts[weaponClass] = 0;
            }
            activeWeaponCounts[weaponClass]++;
        }
        
        return weapon;
    }
    
    public GameObject GetHitEffectPrefab(string weaponClass)
    {
        if (weaponCatalog != null)
        {
            return weaponCatalog.GetHitEffectPrefab(weaponClass);
        }
        return null;
    }
    
    public void ReturnWeapon(BaseWeapon weapon)
    {
        if (weapon == null) return;
        
        string weaponClass = weapon.WeaponClass;
        
        // 풀로 반환
        weapon.ReturnToPool();
        
        // 최대 풀 크기 체크
        if (weaponPools.ContainsKey(weaponClass))
        {
            if (weaponPools[weaponClass].Count < maxPoolSize)
            {
                weaponPools[weaponClass].Enqueue(weapon);
            }
            else
            {
                // 풀이 가득 찬 경우 파괴
                Destroy(weapon.gameObject);
            }
            
            // 활성 카운트 감소
            if (activeWeaponCounts.ContainsKey(weaponClass))
            {
                activeWeaponCounts[weaponClass]--;
            }
        }
        else
        {
            // 풀이 없으면 파괴
            Destroy(weapon.gameObject);
        }
    }
    #endregion
    
    #region Helper Methods
    private BaseWeapon CreateNewWeapon(string weaponClass, WeaponData data, bool addToPool = true)
    {
        if (data == null)
        {
            Debug.LogError($"[WeaponFactory] Invalid weapon data for {weaponClass}");
            return null;
        }
        
        // 카탈로그에서 프리팹 가져오기
        GameObject prefab = weaponCatalog.GetPrefab(weaponClass);
        if (prefab == null)
        {
            Debug.LogError($"[WeaponFactory] No prefab found for {weaponClass}");
            return null;
        }
        
        // 프리팹에서 인스턴스 생성
        GameObject weaponObj = Instantiate(prefab, poolContainer);
        BaseWeapon weapon = weaponObj.GetComponent<BaseWeapon>();
        
        if (weapon == null)
        {
            weapon = weaponObj.AddComponent<BaseWeapon>();
        }
        
        // 초기화
        weapon.Initialize(data, weaponClass);
        weapon.SetFactory(this);
        
        // 비활성화
        weaponObj.SetActive(false);
        
        // 풀에 추가
        if (addToPool && weaponPools.ContainsKey(weaponClass))
        {
            weaponPools[weaponClass].Enqueue(weapon);
        }
        
        return weapon;
    }
    
    private WeaponData GetWeaponData(string weaponClass)
    {
        // 캐시에서 찾기
        if (weaponDataCache.ContainsKey(weaponClass))
        {
            return weaponDataCache[weaponClass];
        }
        
        // 카탈로그에서 찾기
        if (weaponCatalog != null)
        {
            WeaponData data = weaponCatalog.GetData(weaponClass);
            if (data != null)
            {
                weaponDataCache[weaponClass] = data;
                return data;
            }
        }
        
        return null;
    }
    
    // PrewarmPool 제거 - Lazy Loading 방식으로 변경
    // 필요시에만 아래 메서드 사용
    public void EnsureMinimumPool(string weaponClass, int minCount)
    {
        // 특정 상황에서 최소 풀 크기 보장이 필요한 경우만 사용
        // 예: 보스전 시작 직전 등
        if (!weaponPools.ContainsKey(weaponClass))
        {
            WeaponData data = GetWeaponData(weaponClass);
            if (data != null)
            {
                InitializePool(weaponClass, data);
            }
        }
        
        if (weaponPools.ContainsKey(weaponClass))
        {
            WeaponData data = weaponDataCache[weaponClass];
            int currentCount = weaponPools[weaponClass].Count;
            
            for (int i = currentCount; i < minCount && i < maxPoolSize; i++)
            {
                CreateNewWeapon(weaponClass, data);
            }
        }
    }
    
    public void ClearPool(string weaponClass)
    {
        if (weaponPools.ContainsKey(weaponClass))
        {
            while (weaponPools[weaponClass].Count > 0)
            {
                BaseWeapon weapon = weaponPools[weaponClass].Dequeue();
                if (weapon != null)
                {
                    Destroy(weapon.gameObject);
                }
            }
        }
    }
    
    public void ClearAllPools()
    {
        foreach (var poolKey in weaponPools.Keys)
        {
            ClearPool(poolKey);
        }
        weaponPools.Clear();
        activeWeaponCounts.Clear();
    }
    #endregion
    
    #region Debug
    public void PrintPoolStatus()
    {
        Debug.Log("[WeaponFactory] Pool Status:");
        foreach (var kvp in weaponPools)
        {
            int active = activeWeaponCounts.ContainsKey(kvp.Key) ? activeWeaponCounts[kvp.Key] : 0;
            Debug.Log($"  {kvp.Key}: {kvp.Value.Count} in pool, {active} active");
        }
    }
    #endregion
}