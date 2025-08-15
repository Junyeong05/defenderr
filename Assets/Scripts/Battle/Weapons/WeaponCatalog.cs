using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

// 무기 카탈로그 - 모든 무기 데이터와 프리팹 관리
[CreateAssetMenu(fileName = "WeaponCatalog", menuName = "Battle/WeaponCatalog")]
public class WeaponCatalog : ScriptableObject
{
    [System.Serializable]
    public class WeaponEntry
    {
        public string weaponClass;      // "Arrow", "Bullet" 등
        public GameObject prefab;        // 무기 프리팹 (BaseWeapon 컴포넌트 포함)
        public WeaponData data;         // 무기 데이터 (ScriptableObject)
        public GameObject hitEffectPrefab;  // 타격 이펙트 프리팹
        
        // 검증
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(weaponClass) && 
                   prefab != null && 
                   prefab.GetComponent<BaseWeapon>() != null;
        }
    }
    
    [Header("Weapon Entries")]
    [SerializeField] private List<WeaponEntry> weapons = new List<WeaponEntry>();
    
    // 빠른 검색을 위한 딕셔너리 (런타임에서만 사용)
    private Dictionary<string, WeaponEntry> weaponMap;
    
    // 카탈로그 초기화
    public void Initialize()
    {
        weaponMap = new Dictionary<string, WeaponEntry>();
        
        foreach (var entry in weapons)
        {
            if (entry.IsValid())
            {
                weaponMap[entry.weaponClass] = entry;
            }
            else
            {
                Debug.LogWarning($"[WeaponCatalog] Invalid entry: {entry.weaponClass}");
            }
        }
        
        Debug.Log($"[WeaponCatalog] Initialized with {weaponMap.Count} weapons");
    }
    
    // 무기 엔트리 가져오기
    public WeaponEntry GetWeaponEntry(string weaponClass)
    {
        if (weaponMap == null)
        {
            Initialize();
        }
        
        if (weaponMap.TryGetValue(weaponClass, out WeaponEntry entry))
        {
            return entry;
        }
        
        Debug.LogError($"[WeaponCatalog] Weapon class not found: {weaponClass}");
        return null;
    }
    
    // 프리팹 가져오기
    public GameObject GetPrefab(string weaponClass)
    {
        var entry = GetWeaponEntry(weaponClass);
        return entry?.prefab;
    }
    
    // 데이터 가져오기
    public WeaponData GetData(string weaponClass)
    {
        var entry = GetWeaponEntry(weaponClass);
        return entry?.data;
    }
    
    // 타격 이펙트 프리팹 가져오기
    public GameObject GetHitEffectPrefab(string weaponClass)
    {
        var entry = GetWeaponEntry(weaponClass);
        return entry?.hitEffectPrefab;
    }
    
    // 모든 무기 데이터 가져오기
    public List<WeaponData> GetAllWeaponData()
    {
        List<WeaponData> dataList = new List<WeaponData>();
        foreach (var entry in weapons)
        {
            if (entry.data != null)
            {
                dataList.Add(entry.data);
            }
        }
        return dataList;
    }
    
    // 타입별 무기 데이터 가져오기
    public List<WeaponData> GetWeaponsByType(WeaponType type)
    {
        return GetAllWeaponData().Where(data => data != null && data.weaponType == type).ToList();
    }
    
    // 모든 무기 클래스 가져오기
    public List<string> GetAllWeaponClasses()
    {
        List<string> classes = new List<string>();
        foreach (var entry in weapons)
        {
            if (entry.IsValid())
            {
                classes.Add(entry.weaponClass);
            }
        }
        return classes;
    }
    
    
    #if UNITY_EDITOR
    // WeaponData 자동 업데이트 및 프리팹 매칭 (엑셀 임포트 후 사용)
    [ContextMenu("Auto Update From WeaponData")]
    public void AutoUpdateFromWeaponData()
    {
        // Resources/WeaponData 폴더의 모든 WeaponData 찾기
        string[] dataGuids = AssetDatabase.FindAssets("t:WeaponData", new[] { "Assets/Resources/WeaponData" });
        
        foreach (string guid in dataGuids)
        {
            string dataPath = AssetDatabase.GUIDToAssetPath(guid);
            WeaponData weaponData = AssetDatabase.LoadAssetAtPath<WeaponData>(dataPath);
            
            if (weaponData != null)
            {
                string weaponClass = weaponData.weaponClass;
                
                // 기존 엔트리 찾기 또는 생성
                WeaponEntry entry = weapons.Find(e => e.weaponClass == weaponClass);
                if (entry == null)
                {
                    entry = new WeaponEntry();
                    entry.weaponClass = weaponClass;
                    weapons.Add(entry);
                }
                
                // 데이터 업데이트
                entry.data = weaponData;
                
                // 프리팹 자동 매칭
                if (entry.prefab == null)
                {
                    // 여러 가능한 프리팹 경로 시도
                    string[] possiblePaths = new string[]
                    {
                        $"Assets/Prefabs/Weapons/{weaponClass}.prefab",
                        $"Assets/Prefabs/Weapons/Projectiles/{weaponClass}.prefab",
                        $"Assets/Prefabs/Weapons/{weaponData.weaponName}.prefab"
                    };
                    
                    foreach (string prefabPath in possiblePaths)
                    {
                        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                        if (prefab != null && prefab.GetComponent<BaseWeapon>() != null)
                        {
                            entry.prefab = prefab;
                            Debug.Log($"[WeaponCatalog] Matched {weaponClass} with prefab: {prefabPath}");
                            break;
                        }
                    }
                    
                    if (entry.prefab == null)
                    {
                        Debug.LogWarning($"[WeaponCatalog] Could not find prefab for {weaponClass}");
                    }
                }
                
                // 이펙트 프리팹 자동 매칭
                if (entry.hitEffectPrefab == null && !string.IsNullOrEmpty(weaponData.hitEffectName))
                {
                    string[] effectPaths = new string[]
                    {
                        $"Assets/Prefabs/Effects/{weaponData.hitEffectName}.prefab",
                        $"Assets/Prefabs/Effects/HitEffects/{weaponData.hitEffectName}.prefab"
                    };
                    
                    foreach (string effectPath in effectPaths)
                    {
                        GameObject effectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(effectPath);
                        if (effectPrefab != null)
                        {
                            entry.hitEffectPrefab = effectPrefab;
                            Debug.Log($"[WeaponCatalog] Matched effect {weaponData.hitEffectName} with prefab");
                            break;
                        }
                    }
                }
            }
        }
        
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"[WeaponCatalog] Auto-updated {weapons.Count} entries from WeaponData");
    }
    
    // 에디터에서 자동으로 무기 프리팹들을 찾아서 채우기
    [ContextMenu("Auto Populate From Prefabs")]
    private void AutoPopulateFromPrefabs()
    {
        weapons.Clear();
        
        // Prefabs/Weapons 폴더에서 모든 프리팹 찾기
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Weapons" });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null && prefab.GetComponent<BaseWeapon>() != null)
            {
                WeaponEntry entry = new WeaponEntry
                {
                    weaponClass = prefab.name,
                    prefab = prefab
                };
                
                // 같은 이름의 WeaponData 찾기
                string dataPath = $"Assets/Resources/WeaponData/{prefab.name}Data.asset";
                WeaponData data = AssetDatabase.LoadAssetAtPath<WeaponData>(dataPath);
                if (data != null)
                {
                    entry.data = data;
                }
                
                weapons.Add(entry);
                Debug.Log($"[WeaponCatalog] Added: {entry.weaponClass}");
            }
        }
        
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"[WeaponCatalog] Auto-populated {weapons.Count} weapons");
    }
    
    // 유효성 검사
    [ContextMenu("Validate Entries")]
    private void ValidateEntries()
    {
        int validCount = 0;
        int invalidCount = 0;
        
        foreach (var entry in weapons)
        {
            if (entry.IsValid())
            {
                validCount++;
            }
            else
            {
                invalidCount++;
                Debug.LogWarning($"[WeaponCatalog] Invalid entry: {entry.weaponClass}");
            }
        }
        
        Debug.Log($"[WeaponCatalog] Validation complete. Valid: {validCount}, Invalid: {invalidCount}");
    }
    
    // 모든 데이터 리프레시 (Google Sheets 임포트 후 사용)
    [ContextMenu("Refresh All (Data + Prefabs)")]
    public void RefreshAll()
    {
        AutoUpdateFromWeaponData();
        ValidateEntries();
    }
    #endif
}