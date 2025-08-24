using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// BattleStatisticsManager용 UI
/// 동적 팀 할당을 지원하는 개선된 통계 시스템
/// </summary>
public class BattleStatisticsUI : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private bool showStatistics = true;
    [SerializeField] private float updateInterval = 1f; // UI 업데이트 주기 (초)
    
    [Header("UI Position & Size")]
    [SerializeField] private Vector2 position = new Vector2(10, 10);
    [SerializeField] private Vector2 size = new Vector2(600, 700);
    
    private float lastUpdateTime = 0f;
    private string cachedStatsText = "";
    
    // GUI 스타일
    private GUIStyle titleStyle;
    private GUIStyle allyHeaderStyle;
    private GUIStyle enemyHeaderStyle;
    private GUIStyle normalStyle;
    private GUIStyle damageStyle;
    private GUIStyle healStyle;
    private GUIStyle survivalStyle;
    
    void Start()
    {
        InitializeStyles();
    }
    
    void InitializeStyles()
    {
        titleStyle = new GUIStyle();
        titleStyle.fontSize = 18;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.white;
        
        allyHeaderStyle = new GUIStyle();
        allyHeaderStyle.fontSize = 14;
        allyHeaderStyle.fontStyle = FontStyle.Bold;
        allyHeaderStyle.normal.textColor = new Color(0.5f, 0.8f, 1f); // 파란색 계열
        
        enemyHeaderStyle = new GUIStyle();
        enemyHeaderStyle.fontSize = 14;
        enemyHeaderStyle.fontStyle = FontStyle.Bold;
        enemyHeaderStyle.normal.textColor = new Color(1f, 0.5f, 0.5f); // 빨간색 계열
        
        normalStyle = new GUIStyle();
        normalStyle.fontSize = 12;
        normalStyle.normal.textColor = Color.white;
        
        damageStyle = new GUIStyle();
        damageStyle.fontSize = 12;
        damageStyle.normal.textColor = new Color(1f, 0.8f, 0.5f); // 주황색
        
        healStyle = new GUIStyle();
        healStyle.fontSize = 12;
        healStyle.normal.textColor = new Color(0.5f, 1f, 0.5f); // 초록색
        
        survivalStyle = new GUIStyle();
        survivalStyle.fontSize = 12;
        survivalStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f); // 회색
    }
    
    void Update()
    {
        // 키 입력 처리
        HandleInput();
        
        // 주기적으로 통계 텍스트 업데이트
        if (Time.time - lastUpdateTime > updateInterval)
        {
            UpdateStatisticsText();
            lastUpdateTime = Time.time;
        }
    }
    
    void HandleInput()
    {
        // Input System 호환성 체크
        #if ENABLE_LEGACY_INPUT_MANAGER
        // Tab: 통계 표시 토글
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            showStatistics = !showStatistics;
        }
        
        // R: 통계 리셋
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (BattleStatisticsManager.Instance != null)
            {
                BattleStatisticsManager.Instance.Reset();
                Debug.Log("Battle statistics reset");
            }
        }
        
        // P: 전투 요약 출력
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (BattleStatisticsManager.Instance != null)
            {
                BattleStatisticsManager.Instance.PrintBattleSummary();
            }
        }
        
        // C: CSV 내보내기
        if (Input.GetKeyDown(KeyCode.C))
        {
            ExportToCSV();
        }
        #else
        // Input System을 사용하는 경우 키 입력 비활성화
        // Project Settings에서 Input Handling을 "Both"로 변경하거나
        // BattleStatisticsUI_InputSystemVersion.cs를 사용하세요
        #endif
    }
    
    void UpdateStatisticsText()
    {
        if (BattleStatisticsManager.Instance == null) return;
        
        var allStats = BattleStatisticsManager.Instance.GetAllStatistics();
        if (allStats.Count == 0)
        {
            cachedStatsText = "No battle data available\n\nWaiting for battle to start...";
            return;
        }
        
        int battleDuration = BattleStatisticsManager.Instance.GetBattleDuration();
        float seconds = battleDuration / 60f;
        
        StringBuilder sb = new StringBuilder();
        
        // 전투 시간
        sb.AppendLine($"Battle Time: {seconds:F1}s ({battleDuration} frames)");
        sb.AppendLine();
        
        // 아군과 적군 분리
        var allyStats = allStats
            .Where(kvp => kvp.Value.team == "ally")
            .OrderByDescending(kvp => kvp.Value.totalDamageDealt)
            .ToList();
            
        var enemyStats = allStats
            .Where(kvp => kvp.Value.team == "enemy")
            .OrderByDescending(kvp => kvp.Value.totalDamageDealt)
            .ToList();
        
        // 아군 통계
        sb.AppendLine("【 ALLY FORCES 】");
        if (allyStats.Count > 0)
        {
            foreach (var kvp in allyStats)
            {
                AppendHeroTypeStats(sb, kvp.Value, battleDuration, true);
            }
            
            // 아군 총계
            float allyTotalDamage = allyStats.Sum(x => x.Value.totalDamageDealt);
            int allyTotalSpawned = allyStats.Sum(x => x.Value.totalSpawnedCount);
            int allyActive = allyStats.Sum(x => x.Value.currentActiveCount);
            sb.AppendLine($"ALLY TOTAL: {allyTotalSpawned} spawned, {allyActive} active");
            sb.AppendLine($"Total Damage: {allyTotalDamage:F0}");
        }
        else
        {
            sb.AppendLine("  No ally units registered");
        }
        sb.AppendLine();
        
        // 적군 통계
        sb.AppendLine("【 ENEMY FORCES 】");
        if (enemyStats.Count > 0)
        {
            foreach (var kvp in enemyStats)
            {
                AppendHeroTypeStats(sb, kvp.Value, battleDuration, false);
            }
            
            // 적군 총계
            float enemyTotalDamage = enemyStats.Sum(x => x.Value.totalDamageDealt);
            int enemyTotalSpawned = enemyStats.Sum(x => x.Value.totalSpawnedCount);
            int enemyActive = enemyStats.Sum(x => x.Value.currentActiveCount);
            sb.AppendLine($"ENEMY TOTAL: {enemyTotalSpawned} spawned, {enemyActive} active");
            sb.AppendLine($"Total Damage: {enemyTotalDamage:F0}");
        }
        else
        {
            sb.AppendLine("  No enemy units registered");
        }
        sb.AppendLine();
        
        // 매치업 요약
        if (allyStats.Count > 0 && enemyStats.Count > 0)
        {
            sb.AppendLine("【 BATTLE SUMMARY 】");
            
            float allyDamage = allyStats.Sum(x => x.Value.totalDamageDealt);
            float enemyDamage = enemyStats.Sum(x => x.Value.totalDamageDealt);
            int allyKills = allyStats.Sum(x => x.Value.kills);
            int enemyKills = enemyStats.Sum(x => x.Value.kills);
            
            sb.AppendLine($"Damage Ratio: {(enemyDamage > 0 ? allyDamage / enemyDamage : allyDamage):F2}");
            sb.AppendLine($"Kill Score: Ally {allyKills} - {enemyKills} Enemy");
            
            // 승리 판정
            string winner = "ONGOING";
            if (allyStats.Sum(x => x.Value.currentActiveCount) == 0)
                winner = "ENEMY VICTORY";
            else if (enemyStats.Sum(x => x.Value.currentActiveCount) == 0)
                winner = "ALLY VICTORY";
                
            sb.AppendLine($"Status: {winner}");
        }
        
        cachedStatsText = sb.ToString();
    }
    
    void AppendHeroTypeStats(StringBuilder sb, HeroTypeStatistics stats, int battleDuration, bool isAlly)
    {
        // 헤더: 영웅 이름과 타입 번호
        sb.AppendLine($"  ■ {stats.heroName} (Type #{stats.kindNum})");
        
        // 유닛 정보
        sb.AppendLine($"    Units: {stats.totalSpawnedCount} spawned, {stats.currentActiveCount} active, {stats.totalDeathCount} dead");
        sb.AppendLine($"    Survival: {stats.GetSurvivalRate():F1}%");
        
        // 데미지 정보
        if (stats.totalDamageDealt > 0)
        {
            sb.AppendLine($"    Damage: {stats.totalDamageDealt:F0} ({stats.GetDPS(battleDuration):F1} DPS)");
            if (stats.currentActiveCount > 0)
            {
                sb.AppendLine($"    Per Unit: {stats.GetAverageDPS(battleDuration):F1} DPS/unit");
            }
            
            // 크리티컬 정보
            if (stats.totalCriticalHits > 0)
            {
                sb.AppendLine($"    Critical: {stats.GetCriticalRate():F1}% ({stats.totalCriticalHits} hits)");
            }
            
            // DOT 정보
            if (stats.totalDotDamageDealt > 0)
            {
                float dotPercent = (stats.totalDotDamageDealt / stats.totalDamageDealt) * 100;
                sb.AppendLine($"    DOT: {stats.totalDotDamageDealt:F0} ({dotPercent:F1}%)");
            }
        }
        else
        {
            sb.AppendLine($"    Damage: 0 (0.0 DPS)");
        }
        
        // 힐링 정보
        if (stats.totalHealingDone > 0)
        {
            sb.AppendLine($"    Healing: {stats.totalHealingDone:F0} ({stats.GetHPS(battleDuration):F1} HPS)");
        }
        
        // K/D 정보
        sb.AppendLine($"    K/D: {stats.kills}/{stats.totalDeathCount} (Ratio: {stats.GetKDRatio():F2})");
        
        // 효율성
        if (stats.totalDamageDealt > 0 && stats.totalDamageTaken > 0)
        {
            sb.AppendLine($"    Efficiency: {stats.GetEfficiencyScore():F2}");
        }
        
        sb.AppendLine();
    }
    
    void OnGUI()
    {
        if (!showStatistics) return;
        
        // 배경 박스
        GUI.Box(new Rect(position.x, position.y, size.x, size.y), "");
        
        // 제목
        GUI.Label(new Rect(position.x + 10, position.y + 10, size.x - 20, 30), 
            "BATTLE STATISTICS (DYNAMIC TEAMS)", titleStyle);
        
        // 조작 안내
        GUI.Label(new Rect(position.x + 10, position.y + 35, size.x - 20, 20), 
            "Tab: Toggle | R: Reset | P: Print Summary | C: Export CSV", normalStyle);
        
        // 통계 내용
        float yOffset = 60;
        string[] lines = cachedStatsText.Split('\n');
        
        foreach (string line in lines)
        {
            if (string.IsNullOrEmpty(line))
            {
                yOffset += 5;
                continue;
            }
            
            GUIStyle style = normalStyle;
            
            // 스타일 선택
            if (line.Contains("【 ALLY"))
            {
                style = allyHeaderStyle;
            }
            else if (line.Contains("【 ENEMY"))
            {
                style = enemyHeaderStyle;
            }
            else if (line.Contains("【"))
            {
                style = titleStyle;
            }
            else if (line.Contains("Damage:") || line.Contains("DPS"))
            {
                style = damageStyle;
            }
            else if (line.Contains("Healing:") || line.Contains("HPS"))
            {
                style = healStyle;
            }
            else if (line.Contains("Survival:") || line.Contains("Units:"))
            {
                style = survivalStyle;
            }
            
            GUI.Label(new Rect(position.x + 10, position.y + yOffset, size.x - 20, 20), 
                line, style);
            
            yOffset += 16;
            
            // 화면을 벗어나면 중단
            if (yOffset > size.y - 20) break;
        }
    }
    
    void ExportToCSV()
    {
        if (BattleStatisticsManager.Instance == null) return;
        
        string csv = BattleStatisticsManager.Instance.ExportToCSV();
        string filename = $"BattleStats_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
        string path = System.IO.Path.Combine(Application.persistentDataPath, filename);
        
        try
        {
            System.IO.File.WriteAllText(path, csv);
            Debug.Log($"[BattleStatistics] Exported to: {path}");
            
            // 클립보드에 복사 (Editor에서만 작동)
            #if UNITY_EDITOR
            GUIUtility.systemCopyBuffer = csv;
            Debug.Log("[BattleStatistics] CSV copied to clipboard!");
            #endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[BattleStatistics] Export failed: {e.Message}");
        }
    }
}