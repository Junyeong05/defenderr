using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 모든 영웅의 프리팹과 데이터를 관리하는 카탈로그
/// HeroFactory에서 사용
/// </summary>
[CreateAssetMenu(fileName = "HeroCatalog", menuName = "Heroes/Hero Catalog")]
public class HeroCatalog : ScriptableObject
{
    [System.Serializable]
    public class HeroEntry
    {
        public string heroType;        // "ElfArcher1", "FootMan1" 등
        public GameObject prefab;      // 영웅 프리팹 (BaseHero 컴포넌트 포함)
        public HeroData data;         // 영웅 데이터 (ScriptableObject)
        
        // 검증
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(heroType) && 
                   prefab != null && 
                   prefab.GetComponent<BaseHero>() != null;
        }
    }
    
    [Header("Hero Entries")]
    [SerializeField] private List<HeroEntry> heroes = new List<HeroEntry>();
    
    // 빠른 검색을 위한 딕셔너리 (런타임에 생성)
    private Dictionary<string, HeroEntry> heroMap;
    
    /// <summary>
    /// 카탈로그 초기화
    /// </summary>
    public void Initialize()
    {
        heroMap = new Dictionary<string, HeroEntry>();
        
        foreach (var entry in heroes)
        {
            if (entry.IsValid())
            {
                heroMap[entry.heroType] = entry;
            }
            else
            {
                Debug.LogWarning($"[HeroCatalog] Invalid entry: {entry.heroType}");
            }
        }
        
        Debug.Log($"[HeroCatalog] Initialized with {heroMap.Count} heroes");
    }
    
    /// <summary>
    /// 영웅 엔트리 가져오기
    /// </summary>
    public HeroEntry GetHeroEntry(string heroType)
    {
        if (heroMap == null)
        {
            Initialize();
        }
        
        if (heroMap.TryGetValue(heroType, out HeroEntry entry))
        {
            return entry;
        }
        
        Debug.LogError($"[HeroCatalog] Hero type not found: {heroType}");
        return null;
    }
    
    /// <summary>
    /// 프리팹 가져오기
    /// </summary>
    public GameObject GetPrefab(string heroType)
    {
        var entry = GetHeroEntry(heroType);
        return entry?.prefab;
    }
    
    /// <summary>
    /// 데이터 가져오기
    /// </summary>
    public HeroData GetData(string heroType)
    {
        var entry = GetHeroEntry(heroType);
        return entry?.data;
    }
    
    /// <summary>
    /// 모든 영웅 타입 가져오기
    /// </summary>
    public List<string> GetAllHeroTypes()
    {
        List<string> types = new List<string>();
        foreach (var entry in heroes)
        {
            if (entry.IsValid())
            {
                types.Add(entry.heroType);
            }
        }
        return types;
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// 에디터에서 자동으로 영웅 프리팹들을 찾아서 채우기
    /// </summary>
    [ContextMenu("Auto Populate From Prefabs")]
    private void AutoPopulateFromPrefabs()
    {
        heroes.Clear();
        
        // Prefabs/Heroes 폴더에서 모든 프리팹 찾기
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Heroes" });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null && prefab.GetComponent<BaseHero>() != null)
            {
                HeroEntry entry = new HeroEntry
                {
                    heroType = prefab.name,
                    prefab = prefab
                };
                
                // 같은 이름의 HeroData 찾기
                string dataPath = $"Assets/Resources/HeroData/{prefab.name}Data.asset";
                HeroData data = AssetDatabase.LoadAssetAtPath<HeroData>(dataPath);
                if (data != null)
                {
                    entry.data = data;
                }
                
                heroes.Add(entry);
                Debug.Log($"[HeroCatalog] Added: {entry.heroType}");
            }
        }
        
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"[HeroCatalog] Auto-populated {heroes.Count} heroes");
    }
    
    /// <summary>
    /// 유효성 검사
    /// </summary>
    [ContextMenu("Validate Entries")]
    private void ValidateEntries()
    {
        int validCount = 0;
        int invalidCount = 0;
        
        foreach (var entry in heroes)
        {
            if (entry.IsValid())
            {
                validCount++;
            }
            else
            {
                invalidCount++;
                Debug.LogWarning($"[HeroCatalog] Invalid entry: {entry.heroType}");
            }
        }
        
        Debug.Log($"[HeroCatalog] Validation complete. Valid: {validCount}, Invalid: {invalidCount}");
    }
#endif
}