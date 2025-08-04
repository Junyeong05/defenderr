using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// HeroData를 한눈에 볼 수 있는 뷰어
/// </summary>
public class HeroDataViewer : EditorWindow
{
    private Vector2 scrollPosition;
    private List<HeroData> heroDataList = new List<HeroData>();
    private string searchFilter = "";
    private bool showStats = true;
    private bool showAnimationFrames = false;
    
    [MenuItem("Tools/Hero Data Viewer")]
    public static void ShowWindow()
    {
        GetWindow<HeroDataViewer>("Hero Data Viewer");
    }
    
    private void OnEnable()
    {
        RefreshHeroData();
    }
    
    private void RefreshHeroData()
    {
        // Assets/Data/Heroes 폴더의 모든 HeroData 로드
        heroDataList.Clear();
        string[] guids = AssetDatabase.FindAssets("t:HeroData", new[] { "Assets/Data/Heroes" });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            HeroData heroData = AssetDatabase.LoadAssetAtPath<HeroData>(path);
            if (heroData != null)
            {
                heroDataList.Add(heroData);
            }
        }
        
        // kindNum으로 정렬
        heroDataList = heroDataList.OrderBy(h => h.kindNum).ToList();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Hero Data Viewer", EditorStyles.boldLabel);
        if (GUILayout.Button("Refresh", GUILayout.Width(80)))
        {
            RefreshHeroData();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // 검색 필터
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Search:", GUILayout.Width(50));
        searchFilter = EditorGUILayout.TextField(searchFilter);
        EditorGUILayout.EndHorizontal();
        
        // 표시 옵션
        EditorGUILayout.BeginHorizontal();
        showStats = EditorGUILayout.Toggle("Show Stats", showStats);
        showAnimationFrames = EditorGUILayout.Toggle("Show Animation Frames", showAnimationFrames);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // 총 개수 표시
        EditorGUILayout.LabelField($"Total Heroes: {heroDataList.Count}");
        
        EditorGUILayout.Space();
        
        // 스크롤 뷰
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        foreach (var heroData in heroDataList)
        {
            if (!string.IsNullOrEmpty(searchFilter))
            {
                if (!heroData.heroName.ToLower().Contains(searchFilter.ToLower()) &&
                    !heroData.heroClass.ToLower().Contains(searchFilter.ToLower()) &&
                    !heroData.kindNum.ToString().Contains(searchFilter))
                {
                    continue;
                }
            }
            
            DrawHeroData(heroData);
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawHeroData(HeroData heroData)
    {
        EditorGUILayout.BeginVertical("box");
        
        // 헤더
        EditorGUILayout.BeginHorizontal();
        
        // Hero 이름과 kindNum
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 14;
        EditorGUILayout.LabelField($"[{heroData.kindNum}] {heroData.heroName} - {heroData.heroClass}", headerStyle);
        
        // 선택 버튼
        if (GUILayout.Button("Select", GUILayout.Width(60)))
        {
            Selection.activeObject = heroData;
            EditorGUIUtility.PingObject(heroData);
        }
        
        EditorGUILayout.EndHorizontal();
        
        // 스탯 표시
        if (showStats)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("HP:", GUILayout.Width(80));
            EditorGUILayout.LabelField(heroData.maxHealth.ToString(), GUILayout.Width(60));
            EditorGUILayout.LabelField("ATK:", GUILayout.Width(80));
            EditorGUILayout.LabelField(heroData.attackPower.ToString(), GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("DEF:", GUILayout.Width(80));
            EditorGUILayout.LabelField(heroData.defense.ToString(), GUILayout.Width(60));
            EditorGUILayout.LabelField("Speed:", GUILayout.Width(80));
            EditorGUILayout.LabelField(heroData.moveSpeed.ToString(), GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Range:", GUILayout.Width(80));
            EditorGUILayout.LabelField(heroData.attackRange.ToString(), GUILayout.Width(60));
            EditorGUILayout.LabelField("Crit%:", GUILayout.Width(80));
            EditorGUILayout.LabelField(heroData.criticalChance.ToString(), GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }
        
        // 애니메이션 프레임 표시
        if (showAnimationFrames)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField($"Wait: {heroData.startWait}-{heroData.endWait}, Move: {heroData.startMove}-{heroData.endMove}");
            EditorGUILayout.LabelField($"Attack: {heroData.startAttack}-{heroData.endAttack}, Skill: {heroData.startSkill}-{heroData.endSkill}");
            EditorGUILayout.LabelField($"Die: {heroData.startDie}-{heroData.endDie}");
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }
}

/// <summary>
/// HeroData 통계 뷰어
/// </summary>
public class HeroDataStatistics : EditorWindow
{
    private List<HeroData> heroDataList = new List<HeroData>();
    
    [MenuItem("Tools/Hero Data Statistics")]
    public static void ShowWindow()
    {
        GetWindow<HeroDataStatistics>("Hero Statistics");
    }
    
    private void OnEnable()
    {
        RefreshData();
    }
    
    private void RefreshData()
    {
        heroDataList.Clear();
        string[] guids = AssetDatabase.FindAssets("t:HeroData", new[] { "Assets/Data/Heroes" });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            HeroData heroData = AssetDatabase.LoadAssetAtPath<HeroData>(path);
            if (heroData != null)
            {
                heroDataList.Add(heroData);
            }
        }
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Hero Data Statistics", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Refresh"))
        {
            RefreshData();
        }
        
        EditorGUILayout.Space();
        
        if (heroDataList.Count == 0)
        {
            EditorGUILayout.HelpBox("No Hero Data found in Assets/Data/Heroes/", MessageType.Info);
            return;
        }
        
        // 통계 표시
        EditorGUILayout.LabelField($"Total Heroes: {heroDataList.Count}");
        
        // 직업별 분류
        var classCounts = heroDataList.GroupBy(h => h.heroClass)
            .Select(g => new { Class = g.Key, Count = g.Count() })
            .OrderBy(x => x.Class);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Heroes by Class:");
        foreach (var cc in classCounts)
        {
            EditorGUILayout.LabelField($"  {cc.Class}: {cc.Count}");
        }
        
        // 평균 스탯
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Average Stats:");
        EditorGUILayout.LabelField($"  HP: {heroDataList.Average(h => h.maxHealth):F1}");
        EditorGUILayout.LabelField($"  ATK: {heroDataList.Average(h => h.attackPower):F1}");
        EditorGUILayout.LabelField($"  DEF: {heroDataList.Average(h => h.defense):F1}");
        EditorGUILayout.LabelField($"  Speed: {heroDataList.Average(h => h.moveSpeed):F1}");
        EditorGUILayout.LabelField($"  Range: {heroDataList.Average(h => h.attackRange):F1}");
        EditorGUILayout.LabelField($"  Crit%: {heroDataList.Average(h => h.criticalChance):F1}");
    }
}