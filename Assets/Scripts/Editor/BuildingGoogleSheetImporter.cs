using UnityEngine;
using UnityEditor;
using System.Net;
using System.IO;
using System.Collections.Generic;

// Google Sheets CSV 임포터 (Building 데이터용)
public class BuildingGoogleSheetImporter : EditorWindow
{
    private string sheetUrl = "https://docs.google.com/spreadsheets/d/1mkBIdiYGYI0g0INEe3kJlYtNjy84OzId0hqefBYvmms/export?format=csv&gid=0";
    private string lastImportPath = "";

    [MenuItem("Tools/Building/Import from Google Sheet")]
    public static void ShowWindow()
    {
        GetWindow<BuildingGoogleSheetImporter>("Building Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Building Google Sheets Importer", EditorStyles.boldLabel);

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
        // CSV 데이터를 BuildBookVO로 변환
        string[] lines = csvData.Split('\n');
        if (lines.Length < 2)
        {
            Debug.LogError("CSV is empty");
            return;
        }

        string[] headers = ParseCSVLine(lines[0]);
        Debug.Log($"[CSV Import] Headers: {string.Join(", ", headers)}");

        List<BuildBookVO> buildings = new List<BuildBookVO>();

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = ParseCSVLine(lines[i]);
            Debug.Log($"[CSV Import] Line {i}: {lines[i]}");
            Debug.Log($"[CSV Import] Values: {string.Join(" | ", values)}");

            BuildBookVO building = CreateBuildingFromCSVLine(headers, values);
            if (building != null)
            {
                buildings.Add(building);
            }
        }

        // BuildingCatalog에 저장
        SaveBuildingsToCatalog(buildings);

        EditorUtility.DisplayDialog("Import Complete",
            $"Successfully imported {buildings.Count} buildings", "OK");
    }

    private BuildBookVO CreateBuildingFromCSVLine(string[] headers, string[] values)
    {
        if (values.Length < 3) return null;

        BuildBookVO building = new BuildBookVO();

        // 헤더에 따라 값 매핑
        for (int i = 0; i < headers.Length && i < values.Length; i++)
        {
            string header = headers[i].Trim();
            string value = values[i].Trim();

            Debug.Log($"[CSV Import] Processing header '{header}' with value '{value}'");

            MapValueToBuildingData(building, header, value);
        }

        return building;
    }

    private void MapValueToBuildingData(BuildBookVO data, string header, string value)
    {
        switch (header.ToLower())
        {
            case "kindnum":
            case "kind":
            case "번호":
                if (int.TryParse(value, out int kindNum))
                    data.kindNum = kindNum;
                break;

            case "classname":
            case "name":
            case "이름":
                data.className = value;
                break;

            case "type":
            case "타입":
                data.type = value;
                break;

            case "blocks":
            case "블록":
                // 원본 문자열 그대로 저장 (예: "0|0#1|0")
                data.blocks = value;
                break;

            case "blocknum":
            case "블록수":
                if (int.TryParse(value, out int blockNum))
                    data.blockNum = blockNum;
                break;

            case "interval":
            case "간격":
                if (int.TryParse(value, out int interval))
                    data.interval = interval;
                break;

            case "maxcapacity":
            case "최대용량":
                // 원본 문자열 그대로 저장 (예: "2|4|6|8")
                data.maxCapacity = value;
                break;

            case "buffvalues":
            case "버프값":
                // 원본 문자열 그대로 저장 (예: "5|10|15|20")
                data.buffValues = value;
                break;

            case "desc":
            case "설명":
                data.desc = value;
                break;
        }
    }

    private void SaveBuildingsToCatalog(List<BuildBookVO> buildings)
    {
        // Resources 폴더 확인 및 생성
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        string catalogPath = "Assets/Resources/BuildingCatalog.asset";

        // 기존 카탈로그 로드 또는 새로 생성
        BuildingCatalog catalog = AssetDatabase.LoadAssetAtPath<BuildingCatalog>(catalogPath);

        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<BuildingCatalog>();
            AssetDatabase.CreateAsset(catalog, catalogPath);
            Debug.Log($"[CSV Import] Created new BuildingCatalog at {catalogPath}");
        }
        else
        {
            Debug.Log($"[CSV Import] Updating existing BuildingCatalog at {catalogPath}");
        }

        // 카탈로그 데이터 업데이트
        catalog.SetBuildings(buildings);

        // 변경사항 저장
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[CSV Import] Saved {buildings.Count} buildings to BuildingCatalog");
    }

    // CSV 라인을 파싱 (따옴표 안의 쉼표 처리)
    private string[] ParseCSVLine(string csvLine)
    {
        List<string> result = new List<string>();
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
}