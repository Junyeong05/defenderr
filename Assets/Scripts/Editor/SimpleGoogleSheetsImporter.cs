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
        
        string[] headers = lines[0].Split(',');
        int importCount = 0;
        
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            
            string[] values = lines[i].Split(',');
            if (CreateHeroFromCSVLine(headers, values))
            {
                importCount++;
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Import Complete", 
            $"Successfully imported {importCount} heroes", "OK");
    }
    
    private bool CreateHeroFromCSVLine(string[] headers, string[] values)
    {
        if (values.Length < 3) return false; // 최소한 ID, 이름, 직업은 있어야 함
        
        HeroData heroData = ScriptableObject.CreateInstance<HeroData>();
        
        // 임시 프레임 저장용 변수
        int waitFrame = -1;
        int moveFrame = -1;
        int attackFrame = -1;
        int dieFrame = -1;
        int dieEnd = -1;
        string attackTriggerFrames = "";
        
        // 헤더에 따라 값 매핑
        for (int i = 0; i < headers.Length && i < values.Length; i++)
        {
            string header = headers[i].Trim();
            string value = values[i].Trim();
            
            // 프레임 데이터 처리
            switch (header.ToLower())
            {
                case "waitframe":
                    int.TryParse(value, out waitFrame);
                    break;
                case "moveframe":
                    int.TryParse(value, out moveFrame);
                    break;
                case "attackframe":
                    int.TryParse(value, out attackFrame);
                    break;
                case "dieframe":
                    int.TryParse(value, out dieFrame);
                    break;
                case "dieend":
                    int.TryParse(value, out dieEnd);
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
            // 구글 시트에서 받은 값으로 설정
            heroData.startWait = waitFrame;
            heroData.endWait = moveFrame - 1;
            heroData.startMove = moveFrame;
            heroData.endMove = attackFrame - 1;
            heroData.startAttack = attackFrame;
            heroData.endAttack = dieFrame - 1;
            heroData.startSkill = attackFrame;    // 스킬은 공격과 동일
            heroData.endSkill = dieFrame - 1;     // 스킬은 공격과 동일
            heroData.startDie = dieFrame;
            heroData.endDie = dieEnd;
        }
        else
        {
            // 기본값 설정
            SetDefaults(heroData);
        }
        
        // 공격 트리거 프레임 파싱 (예: "32,38" 또는 "32 38")
        if (!string.IsNullOrEmpty(attackTriggerFrames))
        {
            string[] frames = attackTriggerFrames.Replace(" ", ",").Split(',');
            System.Collections.Generic.List<int> frameList = new System.Collections.Generic.List<int>();
            
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
            }
            else
            {
                // 기본값: 공격 애니메이션의 중간 지점 2개
                int midPoint = (heroData.startAttack + heroData.endAttack) / 2;
                heroData.attackTriggerFrames = new int[] { midPoint - 2, midPoint + 2 };
            }
        }
        else
        {
            // 기본값 설정
            int midPoint = (heroData.startAttack + heroData.endAttack) / 2;
            heroData.attackTriggerFrames = new int[] { midPoint - 2, midPoint + 2 };
        }
        
        // 저장
        if (!string.IsNullOrEmpty(heroData.heroClass) && !string.IsNullOrEmpty(heroData.heroName))
        {
            // heroClass를 사용하여 파일명 생성 (Archer, Warrior, Mage 등)
            string path = $"Assets/Data/Heroes/{heroData.heroClass}Data.asset";
            
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/Heroes"))
                AssetDatabase.CreateFolder("Assets/Data", "Heroes");
            
            AssetDatabase.CreateAsset(heroData, path);
            return true;
        }
        
        return false;
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
}