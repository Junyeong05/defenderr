using UnityEngine;
using UnityEditor;
using System.Net;
using System.IO;

// Google Sheets 무기 임포터
public class WeaponGoogleSheetsImporter : EditorWindow
{
    private string sheetUrl = "https://docs.google.com/spreadsheets/d/1N_6QOHM364m23vFul7VipEjRRpEvyMLH7rsaF15uE_s/export?format=csv&gid=1259885847#gid=1259885847";
    private string lastImportPath = "";
    
    [MenuItem("Tools/Weapon Google Sheets Importer")]
    public static void ShowWindow()
    {
        GetWindow<WeaponGoogleSheetsImporter>("Weapon Sheets Importer");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Weapon Google Sheets Importer", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "구글 시트 CSV 내보내기 방법:\n" +
            "1. 구글 시트 열기\n" +
            "2. 파일 > 다운로드 > CSV\n" +
            "3. 또는 공유 링크를 CSV 형식으로 변환:\n" +
            "   /edit를 /export?format=csv로 변경", 
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        sheetUrl = EditorGUILayout.TextField("Sheet CSV URL", sheetUrl);
        
        if (GUILayout.Button("Import from URL"))
        {
            ImportFromURL();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Import from Local CSV File"))
        {
            ImportFromFile();
        }
        
        if (!string.IsNullOrEmpty(lastImportPath))
        {
            EditorGUILayout.HelpBox($"Last import: {lastImportPath}", MessageType.None);
        }
    }
    
    private void ImportFromURL()
    {
        if (string.IsNullOrEmpty(sheetUrl))
        {
            EditorUtility.DisplayDialog("Error", "Please enter a valid URL", "OK");
            return;
        }
        
        try
        {
            using (WebClient client = new WebClient())
            {
                string csvData = client.DownloadString(sheetUrl);
                ProcessCSVData(csvData);
                lastImportPath = sheetUrl;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to download CSV: {e.Message}");
            EditorUtility.DisplayDialog("Error", $"Failed to download: {e.Message}", "OK");
        }
    }
    
    private void ImportFromFile()
    {
        string path = EditorUtility.OpenFilePanel("Select CSV File", "", "csv");
        if (string.IsNullOrEmpty(path)) return;
        
        try
        {
            string csvData = File.ReadAllText(path);
            ProcessCSVData(csvData);
            lastImportPath = path;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to read CSV: {e.Message}");
        }
    }
    
    private void ProcessCSVData(string csvData)
    {
        // CSV 데이터를 WeaponData로 변환
        string[] lines = csvData.Split('\n');
        if (lines.Length < 2)
        {
            Debug.LogError("CSV is empty");
            return;
        }
        
        string[] headers = ParseCSVLine(lines[0]);
        Debug.Log($"[CSV Import] Headers: {string.Join(", ", headers)}");
        int importCount = 0;
        System.Collections.Generic.List<WeaponData> importedWeapons = new System.Collections.Generic.List<WeaponData>();
        
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            
            string[] values = ParseCSVLine(lines[i]);
            Debug.Log($"[CSV Import] Line {i}: {lines[i]}");
            Debug.Log($"[CSV Import] Values: {string.Join(" | ", values)}");
            
            WeaponData weaponData = CreateWeaponFromCSVLine(headers, values);
            if (weaponData != null)
            {
                importedWeapons.Add(weaponData);
                importCount++;
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // WeaponCatalog 자동 업데이트
        if (importedWeapons.Count > 0)
        {
            UpdateWeaponCatalog(importedWeapons);
        }
        
        EditorUtility.DisplayDialog("Import Complete", 
            $"Successfully imported {importCount} weapons and updated WeaponCatalog", "OK");
    }
    
    private WeaponData CreateWeaponFromCSVLine(string[] headers, string[] values)
    {
        if (values.Length < 2) return null; // 최소한 이름과 클래스는 있어야 함
        
        WeaponData weaponData = ScriptableObject.CreateInstance<WeaponData>();
        
        // 헤더에 따라 값 매핑
        for (int i = 0; i < headers.Length && i < values.Length; i++)
        {
            string header = headers[i].Trim();
            string value = values[i].Trim();
            
            Debug.Log($"[CSV Import] Processing header '{header}' (index {i}) with value '{value}'");
            
            MapValueToWeaponData(weaponData, header, value);
        }
        
        // 기본값 설정
        SetDefaults(weaponData);
        
        // 저장
        if (!string.IsNullOrEmpty(weaponData.weaponClass) && !string.IsNullOrEmpty(weaponData.weaponName))
        {
            // weaponClass를 사용하여 파일명 생성 (Arrow, Bullet, Beam 등)
            string path = $"Assets/Resources/WeaponData/{weaponData.weaponClass}.asset";
            
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/WeaponData"))
                AssetDatabase.CreateFolder("Assets/Resources", "WeaponData");
            
            AssetDatabase.CreateAsset(weaponData, path);
            return weaponData;
        }
        
        return null;
    }
    
    private void MapValueToWeaponData(WeaponData data, string header, string value)
    {
        // 한글/영문 헤더 모두 지원
        switch (header.ToLower())
        {
            case "name":
            case "weaponname":
            case "무기이름":
                data.weaponName = value;
                break;
                
            case "class":
            case "weaponclass":
            case "무기클래스":
                data.weaponClass = value;
                break;
                
            case "type":
            case "weapontype":
            case "무기타입":
                // WeaponType enum으로 변환
                if (System.Enum.TryParse<WeaponType>(value, true, out WeaponType weaponType))
                {
                    data.weaponType = weaponType;
                }
                else if (value.ToLower() == "projectile" || value == "투사체")
                {
                    data.weaponType = WeaponType.Projectile;
                }
                else if (value.ToLower() == "beam" || value == "빔")
                {
                    data.weaponType = WeaponType.Beam;
                }
                else if (value.ToLower() == "custom" || value == "커스텀")
                {
                    data.weaponType = WeaponType.Custom;
                }
                break;
                
            case "texturename":
            case "texture":
            case "텍스처":
                data.textureName = value;
                break;
                
            case "startframe":
            case "시작프레임":
                if (int.TryParse(value, out int startFrame))
                    data.startFrame = startFrame;
                break;
                
            case "endframe":
            case "끝프레임":
                if (int.TryParse(value, out int endFrame))
                    data.endFrame = endFrame;
                break;
                
            case "animationspeed":
            case "animspeed":
            case "애니메이션속도":
                if (float.TryParse(value, out float animSpeed))
                    data.animationSpeed = animSpeed;
                break;
                
            case "speed":
            case "initialspeed":
            case "속도":
                if (float.TryParse(value, out float speed))
                    data.initialSpeed = speed;
                break;
                
            case "acceleration":
            case "accel":
            case "가속도":
                if (float.TryParse(value, out float accel))
                    data.acceleration = accel;
                break;
                
            case "rotationspeed":
            case "rotation":
            case "회전속도":
                if (float.TryParse(value, out float rotation))
                    data.rotationSpeed = rotation;
                break;
                
            case "rotatetodirection":
            case "방향회전":
                data.rotateToDirection = value.ToLower() == "true" || value == "1" || value == "예";
                break;
                
            case "lifetime":
            case "생존시간":
                if (float.TryParse(value, out float lifetime))
                    data.lifetime = lifetime;
                break;
                
            case "damagemultiplier":
            case "damage":
            case "데미지배율":
                if (float.TryParse(value, out float damage))
                    data.damageMultiplier = damage;
                break;
                
            case "critchancebonus":
            case "critchance":
            case "치명타확률":
                if (float.TryParse(value, out float critChance))
                    data.critChanceBonus = critChance;
                break;
                
            case "critmultiplierbonus":
            case "critmultiplier":
            case "치명타배율":
                if (float.TryParse(value, out float critMulti))
                    data.critMultiplierBonus = critMulti;
                break;
                
            case "penetration":
            case "관통":
                if (int.TryParse(value, out int penetration))
                    data.penetration = penetration;
                break;
                
            case "hitradius":
            case "radius":
            case "충돌반경":
                if (float.TryParse(value, out float radius))
                    data.hitRadius = radius;
                break;
                
            case "destroyonhit":
            case "타격시파괴":
                data.destroyOnHit = value.ToLower() == "true" || value == "1" || value == "예";
                break;
                
            case "hiteffectname":
            case "hiteffect":
            case "타격이펙트":
                data.hitEffectName = value;
                break;
                
            case "showhiteffect":
            case "이펙트표시":
                data.showHitEffect = value.ToLower() == "true" || value == "1" || value == "예";
                break;
                
            case "hiteffectduration":
            case "이펙트지속시간":
                if (float.TryParse(value, out float effectDuration))
                    data.hitEffectDuration = effectDuration;
                break;
                
            case "delayframes":
            case "delay":
            case "지연프레임":
                if (int.TryParse(value, out int delay))
                    data.delayFrames = delay;
                break;
        }
    }
    
    private void SetDefaults(WeaponData data)
    {
        // 기본값 설정
        if (data.damageMultiplier == 0)
            data.damageMultiplier = 1f;
            
        if (data.initialSpeed == 0)
            data.initialSpeed = 5f;
            
        if (data.lifetime == 0)
            data.lifetime = 5f;
            
        if (data.hitRadius == 0)
            data.hitRadius = 10f;
            
        if (data.animationSpeed == 0)
            data.animationSpeed = 1f;
            
        if (data.hitEffectDuration == 0)
            data.hitEffectDuration = 1f;
            
        // 텍스처 이름이 없으면 weaponClass를 기본값으로
        if (string.IsNullOrEmpty(data.textureName))
            data.textureName = data.weaponClass;
    }
    
    // CSV 라인을 파싱 (따옴표 안의 쉼표 처리)
    private string[] ParseCSVLine(string csvLine)
    {
        System.Collections.Generic.List<string> result = new System.Collections.Generic.List<string>();
        bool inQuotes = false;
        string currentField = "";
        
        for (int i = 0; i < csvLine.Length; i++)
        {
            char c = csvLine[i];
            
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField);
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }
        
        // 마지막 필드 추가
        result.Add(currentField);
        
        return result.ToArray();
    }
    
    // WeaponCatalog를 자동으로 업데이트
    private void UpdateWeaponCatalog(System.Collections.Generic.List<WeaponData> importedWeapons)
    {
        // WeaponCatalog 찾기 또는 생성
        WeaponCatalog catalog = null;
        string catalogPath = "Assets/Resources/WeaponData/WeaponCatalog.asset";
        
        // 기존 카탈로그 찾기
        string[] guids = AssetDatabase.FindAssets("t:WeaponCatalog");
        if (guids.Length > 0)
        {
            catalogPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            catalog = AssetDatabase.LoadAssetAtPath<WeaponCatalog>(catalogPath);
        }
        
        // 카탈로그가 없으면 생성
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<WeaponCatalog>();
            
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/WeaponData"))
                AssetDatabase.CreateFolder("Assets/Resources", "WeaponData");
                
            AssetDatabase.CreateAsset(catalog, catalogPath);
            Debug.Log($"[CSV Import] Created new WeaponCatalog at {catalogPath}");
        }
        else
        {
            Debug.Log($"[CSV Import] Found existing WeaponCatalog at {catalogPath}");
        }
        
        // weapons 리스트 초기화 (기존 항목 모두 제거)
        catalog.weapons.Clear();
        Debug.Log("[CSV Import] Cleared existing weapon entries");
        
        foreach (WeaponData weaponData in importedWeapons)
        {
            string weaponClass = weaponData.weaponClass;
            Debug.Log($"[CSV Import] Processing weapon: {weaponClass}");
            
            // 프리팹 자동 찾기
            GameObject prefab = null;
            string[] prefabPaths = new string[] {
                $"Assets/Prefabs/Weapons/{weaponClass}.prefab",
                $"Assets/Prefabs/Weapons/Projectiles/{weaponClass}.prefab",
                $"Assets/Prefabs/Weapons/{weaponData.weaponName}.prefab"
            };
            
            foreach (string prefabPath in prefabPaths)
            {
                GameObject testPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (testPrefab != null)
                {
                    prefab = testPrefab;
                    Debug.Log($"[CSV Import] Found prefab for {weaponClass} at {prefabPath}");
                    break;
                }
            }
            
            if (prefab == null)
            {
                Debug.LogWarning($"[CSV Import] Could not find prefab for {weaponClass}");
            }
            
            // 이펙트 프리팹 자동 찾기
            GameObject hitEffectPrefab = null;
            if (!string.IsNullOrEmpty(weaponData.hitEffectName))
            {
                string[] effectPaths = new string[] {
                    $"Assets/Prefabs/Effects/{weaponData.hitEffectName}.prefab",
                    $"Assets/Prefabs/Effects/HitEffects/{weaponData.hitEffectName}.prefab"
                };
                
                foreach (string effectPath in effectPaths)
                {
                    GameObject testEffect = AssetDatabase.LoadAssetAtPath<GameObject>(effectPath);
                    if (testEffect != null)
                    {
                        hitEffectPrefab = testEffect;
                        Debug.Log($"[CSV Import] Found effect prefab for {weaponData.hitEffectName}");
                        break;
                    }
                }
            }
            
            // 새 엔트리 생성 및 추가
            WeaponCatalog.WeaponEntry newEntry = new WeaponCatalog.WeaponEntry();
            newEntry.weaponClass = weaponClass;
            newEntry.data = weaponData;
            newEntry.prefab = prefab;
            newEntry.hitEffectPrefab = hitEffectPrefab;
            
            catalog.weapons.Add(newEntry);
            Debug.Log($"[CSV Import] Added entry for {weaponClass}");
        }
        
        // 카탈로그 저장
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // 최종 검증
        int finalCount = catalog.weapons.Count;
        Debug.Log($"[CSV Import] Updated WeaponCatalog with {importedWeapons.Count} weapons. Catalog now has {finalCount} entries.");
        
        if (finalCount == 0)
        {
            Debug.LogError("[CSV Import] WARNING: WeaponCatalog still has 0 entries after import!");
        }
    }
}