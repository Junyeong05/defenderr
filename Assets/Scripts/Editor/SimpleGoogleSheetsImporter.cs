using UnityEngine;
using UnityEditor;
using System.Net;
using System.IO;

/// <summary>
/// 간단한 Google Sheets 임포터 (Editor Coroutines 불필요)
/// </summary>
public class SimpleGoogleSheetsImporter : EditorWindow
{
    private string sheetUrl = "https://docs.google.com/spreadsheets/d/1N_6QOHM364m23vFul7VipEjRRpEvyMLH7rsaF15uE_s/export?format=csv&gid=0";
    private string lastImportPath = "https://docs.google.com/spreadsheets/d/1N_6QOHM364m23vFul7VipEjRRpEvyMLH7rsaF15uE_s/export?format=csv&gid=0";
    
    [MenuItem("Tools/Simple Google Sheets Importer")]
    public static void ShowWindow()
    {
        GetWindow<SimpleGoogleSheetsImporter>("Simple Sheets Importer");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Simple Google Sheets Importer", EditorStyles.boldLabel);
        
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
        // CSV 데이터를 HeroData로 변환
        string[] lines = csvData.Split('\n');
        if (lines.Length < 2)
        {
            Debug.LogError("CSV is empty");
            return;
        }
        
        string[] headers = ParseCSVLine(lines[0]);
        Debug.Log($"[CSV Import] Headers: {string.Join(", ", headers)}");
        int importCount = 0;
        System.Collections.Generic.List<HeroData> importedHeroes = new System.Collections.Generic.List<HeroData>();
        
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            
            // CSV 파싱 개선 - 따옴표 처리
            string[] values = ParseCSVLine(lines[i]);
            Debug.Log($"[CSV Import] Line {i}: {lines[i]}");
            Debug.Log($"[CSV Import] Values: {string.Join(" | ", values)}");
            
            HeroData heroData = CreateHeroFromCSVLine(headers, values);
            if (heroData != null)
            {
                importedHeroes.Add(heroData);
                importCount++;
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // HeroCatalog 자동 업데이트
        if (importedHeroes.Count > 0)
        {
            UpdateHeroCatalog(importedHeroes);
        }
        
        EditorUtility.DisplayDialog("Import Complete", 
            $"Successfully imported {importCount} heroes and updated HeroCatalog", "OK");
    }
    
    private HeroData CreateHeroFromCSVLine(string[] headers, string[] values)
    {
        if (values.Length < 3) return null; // 최소한 ID, 이름, 직업은 있어야 함
        
        HeroData heroData = ScriptableObject.CreateInstance<HeroData>();
        
        // 임시 이름 (디버깅용)
        string tempName = values.Length > 1 ? values[1] : "Unknown";
        
        // 임시 프레임 저장용 변수
        int waitFrame = -1;
        int moveFrame = -1;
        int attackFrame = -1;
        int skillFrame = -1;
        int dieFrame = -1;
        int dieEnd = -1;
        string attackTriggerFrames = "";
        
        // 헤더에 따라 값 매핑
        for (int i = 0; i < headers.Length && i < values.Length; i++)
        {
            string header = headers[i].Trim();
            string value = values[i].Trim();
            
            // 디버그: 현재 처리중인 헤더와 값
            Debug.Log($"[CSV Import] Processing header '{header}' (index {i}) with value '{value}'");
            
            // 프레임 데이터 처리
            switch (header.ToLower())
            {
                case "waitframe":
                    if (int.TryParse(value, out waitFrame))
                        Debug.Log($"[CSV Import] {tempName} - waitFrame parsed: {waitFrame} from '{value}'");
                    break;
                case "moveframe":
                    if (int.TryParse(value, out moveFrame))
                        Debug.Log($"[CSV Import] {tempName} - moveFrame parsed: {moveFrame} from '{value}'");
                    break;
                case "attackframe":
                    if (int.TryParse(value, out attackFrame))
                        Debug.Log($"[CSV Import] {tempName} - attackFrame parsed: {attackFrame} from '{value}'");
                    break;
                case "skillframe":
                    if (int.TryParse(value, out skillFrame))
                        Debug.Log($"[CSV Import] {tempName} - skillFrame parsed: {skillFrame} from '{value}'");
                    break;
                case "dieframe":
                    if (int.TryParse(value, out dieFrame))
                        Debug.Log($"[CSV Import] {tempName} - dieFrame parsed: {dieFrame} from '{value}'");
                    break;
                case "dieend":
                    if (int.TryParse(value, out dieEnd))
                        Debug.Log($"[CSV Import] {tempName} - dieEnd parsed: {dieEnd} from '{value}'");
                    break;
                case "attacktriggerframes":
                case "attackframes":
                case "공격프레임":
                    attackTriggerFrames = value;
                    break;
                default:
                    MapValueToHeroData(heroData, header, value);
                    break;
            }
        }
        
        // 프레임 설정
        if (waitFrame >= 0 && moveFrame > 0 && attackFrame > 0 && dieFrame > 0 && dieEnd > 0)
        {
            // 구글 시트의 값은 각 애니메이션의 시작 프레임을 나타냄
            // 각 애니메이션의 끝은 다음 애니메이션 시작 -1
            heroData.startWait = waitFrame;
            heroData.endWait = moveFrame - 1;
            
            heroData.startMove = moveFrame;
            heroData.endMove = attackFrame - 1;
            
            heroData.startAttack = attackFrame;
            
            // 스킬 프레임 처리
            if (skillFrame > 0 && skillFrame < 999)  // 999는 스킬 없음을 의미
            {
                heroData.endAttack = skillFrame - 1;
                heroData.startSkill = skillFrame;
                heroData.endSkill = dieFrame - 1;
            }
            else
            {
                // 스킬이 없으면 공격이 죽음 프레임 전까지
                heroData.endAttack = dieFrame - 1;
                // 스킬은 공격과 동일하게 설정
                heroData.startSkill = heroData.startAttack;
                heroData.endSkill = heroData.endAttack;
            }
            
            heroData.startDie = dieFrame;
            heroData.endDie = dieEnd;
            
            Debug.Log($"[CSV Import] {heroData.heroName} frames set - Wait: {heroData.startWait}-{heroData.endWait}, Move: {heroData.startMove}-{heroData.endMove}, Attack: {heroData.startAttack}-{heroData.endAttack}, Skill: {heroData.startSkill}-{heroData.endSkill}, Die: {heroData.startDie}-{heroData.endDie}");
        }
        else
        {
            Debug.LogWarning($"[CSV Import] {heroData.heroName} using default frames - waitFrame:{waitFrame}, moveFrame:{moveFrame}, attackFrame:{attackFrame}, skillFrame:{skillFrame}, dieFrame:{dieFrame}, dieEnd:{dieEnd}");
            // 기본값 설정
            SetDefaults(heroData);
        }
        
        // 공격 트리거 프레임 파싱 (예: "32,38" 또는 "32 38" 또는 단일 프레임 "62")
        if (!string.IsNullOrEmpty(attackTriggerFrames))
        {
            System.Collections.Generic.List<int> frameList = new System.Collections.Generic.List<int>();
            
            // 쉼표나 공백으로 구분된 프레임들 파싱
            string[] frames = attackTriggerFrames.Replace(" ", ",").Split(new char[] {','}, System.StringSplitOptions.RemoveEmptyEntries);
            
            foreach (string frame in frames)
            {
                if (int.TryParse(frame.Trim(), out int frameNum))
                {
                    frameList.Add(frameNum);
                }
            }
            
            if (frameList.Count > 0)
            {
                heroData.attackTriggerFrames = frameList.ToArray();
                Debug.Log($"[CSV Import] {heroData.heroName} attack frames: [{string.Join(", ", frameList)}]");
            }
            else
            {
                // 기본값: 공격 애니메이션의 중간 지점
                int midPoint = (heroData.startAttack + heroData.endAttack) / 2;
                heroData.attackTriggerFrames = new int[] { midPoint };
                Debug.Log($"[CSV Import] {heroData.heroName} using default attack frame: {midPoint}");
            }
        }
        else
        {
            // 기본값 설정
            int midPoint = (heroData.startAttack + heroData.endAttack) / 2;
            heroData.attackTriggerFrames = new int[] { midPoint };
            Debug.Log($"[CSV Import] {heroData.heroName} no attack frames specified, using default: {midPoint}");
        }
        
        // 저장
        if (!string.IsNullOrEmpty(heroData.heroClass) && !string.IsNullOrEmpty(heroData.heroName))
        {
            // heroClass를 사용하여 파일명 생성 (Archer, Warrior, Mage 등)
            string path = $"Assets/Resources/HeroData/{heroData.heroClass}.asset";
            
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/HeroData"))
                AssetDatabase.CreateFolder("Assets/Resources", "HeroData");
            
            AssetDatabase.CreateAsset(heroData, path);
            return heroData;
        }
        
        return null;
    }
    
    private void MapValueToHeroData(HeroData data, string header, string value)
    {
        // 한글/영문 헤더 모두 지원
        switch (header.ToLower())
        {
            case "kindnum":
            case "kind":
            case "종류번호":
                if (int.TryParse(value, out int kindNum))
                    data.kindNum = kindNum;
                break;
            case "name":
            case "이름":
                data.heroName = value;
                data.spriteName = value;
                break;
            case "class":
            case "직업":
                data.heroClass = value;
                break;
            case "hp":
            case "체력":
                float.TryParse(value, out data.maxHealth);
                break;
            case "atk":
            case "공격력":
                float.TryParse(value, out data.attackPower);
                break;
            case "def":
            case "방어력":
                float.TryParse(value, out data.defense);
                break;
            case "speed":
            case "이동속도":
                float.TryParse(value, out data.moveSpeed);
                break;
            case "range":
            case "사거리":
                float.TryParse(value, out data.attackRange);
                break;
            case "crit":
            case "치명타확률":
                float.TryParse(value, out data.criticalChance);
                break;
            case "isranged":
            case "원거리":
                data.isRanged = value.ToLower() == "true" || value == "1" || value == "원거리";
                break;
            case "attackinterval":
            case "공격간격":
                if (int.TryParse(value, out int interval))
                    data.attackInterval = interval;
                break;
        }
    }
    
    private void SetDefaults(HeroData data)
    {
        // 애니메이션 프레임 기본값
        data.startWait = 0;
        data.endWait = 19;
        data.startMove = 20;
        data.endMove = 39;
        data.startAttack = 40;
        data.endAttack = 59;
        data.startSkill = 40;    // 스킬은 공격과 동일
        data.endSkill = 59;      // 스킬은 공격과 동일
        data.startDie = 90;
        data.endDie = 119;
        
        // 성장 스탯
        data.healthPerLevel = data.maxHealth * 0.1f;
        data.attackPerLevel = data.attackPower * 0.1f;
        data.defensePerLevel = data.defense * 0.1f;
    }
    
    /// <summary>
    /// CSV 라인을 파싱 (따옴표 안의 쉼표 처리)
    /// </summary>
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
    
    /// <summary>
    /// HeroCatalog를 자동으로 업데이트
    /// </summary>
    private void UpdateHeroCatalog(System.Collections.Generic.List<HeroData> importedHeroes)
    {
        // HeroCatalog 찾기 또는 생성
        HeroCatalog catalog = null;
        string catalogPath = "Assets/Resources/HeroCatalog.asset";
        
        // 기존 카탈로그 찾기
        string[] guids = AssetDatabase.FindAssets("t:HeroCatalog");
        if (guids.Length > 0)
        {
            catalogPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            catalog = AssetDatabase.LoadAssetAtPath<HeroCatalog>(catalogPath);
        }
        
        // 카탈로그가 없으면 생성
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<HeroCatalog>();
            
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
                
            AssetDatabase.CreateAsset(catalog, catalogPath);
            Debug.Log($"[CSV Import] Created new HeroCatalog at {catalogPath}");
        }
        
        // 카탈로그 업데이트
        System.Reflection.FieldInfo heroesField = typeof(HeroCatalog).GetField("heroes", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (heroesField != null)
        {
            var heroList = heroesField.GetValue(catalog) as System.Collections.Generic.List<HeroCatalog.HeroEntry>;
            if (heroList == null)
            {
                heroList = new System.Collections.Generic.List<HeroCatalog.HeroEntry>();
            }
            
            foreach (HeroData heroData in importedHeroes)
            {
                // heroClass를 그대로 사용
                string heroType = heroData.heroClass;
                
                // 기존 엔트리 찾기
                HeroCatalog.HeroEntry existingEntry = heroList.Find(e => e.heroType == heroType);
                
                if (existingEntry == null)
                {
                    // 새 엔트리 생성
                    existingEntry = new HeroCatalog.HeroEntry();
                    existingEntry.heroType = heroType;
                    heroList.Add(existingEntry);
                }
                
                // 데이터 업데이트
                existingEntry.data = heroData;
                
                // 프리팹 자동 찾기 및 연결
                string[] prefabPaths = new string[] {
                    $"Assets/Prefabs/Heroes/{heroType}.prefab",
                    $"Assets/Prefabs/Heroes/{heroData.heroName}.prefab"  // heroName으로도 시도
                };
                
                foreach (string prefabPath in prefabPaths)
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (prefab != null && prefab.GetComponent<BaseHero>() != null)
                    {
                        existingEntry.prefab = prefab;
                        Debug.Log($"[CSV Import] Linked {heroType} to prefab at {prefabPath}");
                        break;
                    }
                }
                
                if (existingEntry.prefab == null)
                {
                    Debug.LogWarning($"[CSV Import] Could not find prefab for {heroType}");
                }
            }
            
            // 리스트 다시 설정
            heroesField.SetValue(catalog, heroList);
            
            // 카탈로그 저장
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"[CSV Import] Updated HeroCatalog with {importedHeroes.Count} heroes");
        }
    }
}