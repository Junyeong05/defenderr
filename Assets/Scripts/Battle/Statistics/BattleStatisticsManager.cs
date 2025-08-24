using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 개선된 전투 통계 관리자
/// 팀을 동적으로 설정하고 관리
/// </summary>
public class BattleStatisticsManager : MonoBehaviour
{
    private static BattleStatisticsManager instance;
    public static BattleStatisticsManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("BattleStatisticsManager");
                instance = go.AddComponent<BattleStatisticsManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
    
    // 팀별 영웅 리스트
    private List<BaseHero> allyTeam = new List<BaseHero>();
    private List<BaseHero> enemyTeam = new List<BaseHero>();
    
    // 영웅 타입별 통계 (키: "{kindNum}_{team}")
    private Dictionary<string, HeroTypeStatistics> typeStatistics = new Dictionary<string, HeroTypeStatistics>();
    
    // 영웅 인스턴스 추적
    private Dictionary<BaseHero, string> heroToTypeKey = new Dictionary<BaseHero, string>();
    
    // 전투 시간 추적
    private int battleStartFrame = 0;
    private int battleEndFrame = 0;
    private bool isBattleActive = false;
    
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// 팀 설정 - 전투 시작 전에 호출
    /// </summary>
    public void SetTeams(List<BaseHero> allyHeroes, List<BaseHero> enemyHeroes)
    {
        // 팀 리스트 초기화
        allyTeam.Clear();
        enemyTeam.Clear();
        
        // 아군 팀 설정
        if (allyHeroes != null)
        {
            allyTeam.AddRange(allyHeroes);
            foreach (var hero in allyHeroes)
            {
                RegisterHero(hero, true); // true = ally
            }
        }
        
        // 적군 팀 설정
        if (enemyHeroes != null)
        {
            enemyTeam.AddRange(enemyHeroes);
            foreach (var hero in enemyHeroes)
            {
                RegisterHero(hero, false); // false = enemy
            }
        }
        
        Debug.Log($"[BattleStatistics] Teams set - Allies: {allyTeam.Count}, Enemies: {enemyTeam.Count}");
    }
    
    /// <summary>
    /// 전투 시작
    /// </summary>
    public void StartBattle()
    {
        battleStartFrame = Time.frameCount;
        isBattleActive = true;
        typeStatistics.Clear();
        heroToTypeKey.Clear();
        
        // 이미 설정된 팀의 영웅들을 다시 등록
        foreach (var hero in allyTeam)
        {
            RegisterHero(hero, true);
        }
        
        foreach (var hero in enemyTeam)
        {
            RegisterHero(hero, false);
        }
        
        Debug.Log("[BattleStatistics] Battle tracking started");
    }
    
    /// <summary>
    /// 전투 종료
    /// </summary>
    public void EndBattle()
    {
        if (!isBattleActive) return;
        
        battleEndFrame = Time.frameCount;
        isBattleActive = false;
        
        // 전투 요약 출력
        PrintBattleSummary();
    }
    
    /// <summary>
    /// 영웅 등록 (내부용)
    /// </summary>
    private void RegisterHero(BaseHero hero, bool isAlly)
    {
        if (hero == null)
        {
            Debug.LogWarning("[BattleStatistics] RegisterHero called with null hero");
            return;
        }
        
        // 영웅 정보 가져오기 (public 속성 사용)
        int kindNum = hero.KindNum;
        string className = hero.HeroClassName;
        string heroName = hero.HeroName;
        
        // kindNum이 0인 경우 상세 정보 출력
        if (kindNum == 0)
        {
            Debug.LogWarning($"[BattleStatistics] Hero {hero.name} has kindNum=0. " +
                           $"Class: {className}, Name: {heroName}, Team: {(isAlly ? "ally" : "enemy")}");
            
            // className 기반으로 임시 번호 할당
            if (className.Contains("Elf") || className.Contains("Archer"))
                kindNum = 2;
            else if (className.Contains("Foot") || className.Contains("Man"))
                kindNum = 1;
            else
                kindNum = 99; // 기타 영웅용 기본값
                
            Debug.Log($"[BattleStatistics] Assigned temporary kindNum={kindNum} for {heroName} based on className: {className}");
        }
        
        string team = isAlly ? "ally" : "enemy";
        
        // 타입 키 생성
        string typeKey = $"{kindNum}_{team}";
        
        // 타입별 통계가 없으면 생성
        if (!typeStatistics.ContainsKey(typeKey))
        {
            var stats = new HeroTypeStatistics();
            stats.Initialize(kindNum, className, heroName, team);
            typeStatistics[typeKey] = stats;
            Debug.Log($"[BattleStatistics] New type registered: {heroName} ({team}) - Type #{kindNum}, Class: {className}");
        }
        
        // 영웅 인스턴스 매핑
        heroToTypeKey[hero] = typeKey;
        
        // 스폰 카운트 증가
        typeStatistics[typeKey].OnHeroSpawned(hero.GetInstanceID());
        Debug.Log($"[BattleStatistics] Hero spawned: {heroName} ({team}) Instance: {hero.GetInstanceID()}");
    }
    
    /// <summary>
    /// 데미지 기록
    /// </summary>
    public void RecordDamage(BaseHero attacker, BaseHero target, float damage, bool isDot = false, bool isCritical = false, int skillId = -1)
    {
        if (!isBattleActive) return;
        
        // 공격자 통계
        if (attacker != null && heroToTypeKey.ContainsKey(attacker))
        {
            string typeKey = heroToTypeKey[attacker];
            if (typeStatistics.ContainsKey(typeKey))
            {
                typeStatistics[typeKey].RecordDamageDealt(damage, target, isDot, isCritical, skillId);
            }
        }
        
        // 피격자 통계
        if (target != null && heroToTypeKey.ContainsKey(target))
        {
            string typeKey = heroToTypeKey[target];
            if (typeStatistics.ContainsKey(typeKey))
            {
                typeStatistics[typeKey].RecordDamageTaken(damage);
            }
        }
    }
    
    /// <summary>
    /// 힐 기록
    /// </summary>
    public void RecordHealing(BaseHero healer, BaseHero target, float healing, bool isDot = false)
    {
        if (!isBattleActive) return;
        
        // 힐러 통계
        if (healer != null && heroToTypeKey.ContainsKey(healer))
        {
            string typeKey = heroToTypeKey[healer];
            if (typeStatistics.ContainsKey(typeKey))
            {
                typeStatistics[typeKey].RecordHealingDone(healing, target, isDot);
            }
        }
        
        // 대상 통계
        if (target != null && heroToTypeKey.ContainsKey(target))
        {
            string typeKey = heroToTypeKey[target];
            if (typeStatistics.ContainsKey(typeKey))
            {
                typeStatistics[typeKey].RecordHealingReceived(healing);
            }
        }
    }
    
    /// <summary>
    /// 킬 기록
    /// </summary>
    public void RecordKill(BaseHero killer, BaseHero victim)
    {
        if (!isBattleActive) return;
        
        if (killer != null && heroToTypeKey.ContainsKey(killer))
        {
            string typeKey = heroToTypeKey[killer];
            if (typeStatistics.ContainsKey(typeKey))
            {
                typeStatistics[typeKey].RecordKill();
            }
        }
        
        if (victim != null && heroToTypeKey.ContainsKey(victim))
        {
            string typeKey = heroToTypeKey[victim];
            if (typeStatistics.ContainsKey(typeKey))
            {
                typeStatistics[typeKey].RecordDeath();
                typeStatistics[typeKey].OnHeroDied(victim.GetInstanceID());
            }
        }
    }
    
    /// <summary>
    /// 영웅 제거 (풀 반환 시)
    /// </summary>
    public void UnregisterHero(BaseHero hero)
    {
        if (hero == null) return;
        
        if (heroToTypeKey.ContainsKey(hero))
        {
            string typeKey = heroToTypeKey[hero];
            if (typeStatistics.ContainsKey(typeKey))
            {
                typeStatistics[typeKey].OnHeroReturnedToPool(hero.GetInstanceID());
            }
            heroToTypeKey.Remove(hero);
        }
        
        // 팀 리스트에서도 제거
        allyTeam.Remove(hero);
        enemyTeam.Remove(hero);
    }
    
    /// <summary>
    /// 전투 시간 (프레임)
    /// </summary>
    public int GetBattleDuration()
    {
        if (isBattleActive)
        {
            return Time.frameCount - battleStartFrame;
        }
        return battleEndFrame - battleStartFrame;
    }
    
    /// <summary>
    /// 전투 요약 출력
    /// </summary>
    public void PrintBattleSummary()
    {
        int duration = GetBattleDuration();
        float seconds = duration / 60f;
        
        StringBuilder summary = new StringBuilder();
        summary.AppendLine("\n╔════════════════════════════════════════╗");
        summary.AppendLine("║     BATTLE STATISTICS SUMMARY          ║");
        summary.AppendLine("╚════════════════════════════════════════╝");
        summary.AppendLine($"Duration: {seconds:F1} seconds ({duration} frames)");
        summary.AppendLine();
        
        // 아군과 적군 분리
        var allyStats = typeStatistics.Where(kvp => kvp.Value.team == "ally").ToList();
        var enemyStats = typeStatistics.Where(kvp => kvp.Value.team == "enemy").ToList();
        
        // 아군 통계
        if (allyStats.Count > 0)
        {
            summary.AppendLine("┌─────────────────────────────────────┐");
            summary.AppendLine("│         ALLY FORCES                 │");
            summary.AppendLine("└─────────────────────────────────────┘");
            
            foreach (var kvp in allyStats.OrderByDescending(x => x.Value.totalDamageDealt))
            {
                var stats = kvp.Value;
                summary.AppendLine($"• {stats.heroName} (Type #{stats.kindNum})");
                summary.AppendLine($"  Units: {stats.totalSpawnedCount} spawned, {stats.currentActiveCount} active");
                summary.AppendLine($"  Damage: {stats.totalDamageDealt:F0} ({stats.GetDPS(duration):F1} DPS)");
                if (stats.currentActiveCount > 0)
                {
                    summary.AppendLine($"  Per Unit: {stats.GetAverageDPS(duration):F1} DPS/unit");
                }
                summary.AppendLine($"  K/D: {stats.kills}/{stats.totalDeathCount}");
                summary.AppendLine();
            }
        }
        
        // 적군 통계
        if (enemyStats.Count > 0)
        {
            summary.AppendLine("┌─────────────────────────────────────┐");
            summary.AppendLine("│         ENEMY FORCES                │");
            summary.AppendLine("└─────────────────────────────────────┘");
            
            foreach (var kvp in enemyStats.OrderByDescending(x => x.Value.totalDamageDealt))
            {
                var stats = kvp.Value;
                summary.AppendLine($"• {stats.heroName} (Type #{stats.kindNum})");
                summary.AppendLine($"  Units: {stats.totalSpawnedCount} spawned, {stats.currentActiveCount} active");
                summary.AppendLine($"  Damage: {stats.totalDamageDealt:F0} ({stats.GetDPS(duration):F1} DPS)");
                if (stats.currentActiveCount > 0)
                {
                    summary.AppendLine($"  Per Unit: {stats.GetAverageDPS(duration):F1} DPS/unit");
                }
                summary.AppendLine($"  K/D: {stats.kills}/{stats.totalDeathCount}");
                summary.AppendLine();
            }
        }
        
        // 전투 결과
        summary.AppendLine("┌─────────────────────────────────────┐");
        summary.AppendLine("│         BATTLE RESULT               │");
        summary.AppendLine("└─────────────────────────────────────┘");
        
        float allyDamage = allyStats.Sum(x => x.Value.totalDamageDealt);
        float enemyDamage = enemyStats.Sum(x => x.Value.totalDamageDealt);
        int allyKills = allyStats.Sum(x => x.Value.kills);
        int enemyKills = enemyStats.Sum(x => x.Value.kills);
        int allySurvivors = allyStats.Sum(x => x.Value.currentActiveCount);
        int enemySurvivors = enemyStats.Sum(x => x.Value.currentActiveCount);
        
        summary.AppendLine($"Total Damage - Ally: {allyDamage:F0} | Enemy: {enemyDamage:F0}");
        summary.AppendLine($"Total Kills - Ally: {allyKills} | Enemy: {enemyKills}");
        summary.AppendLine($"Survivors - Ally: {allySurvivors} | Enemy: {enemySurvivors}");
        
        // 승리 판정
        string winner = "DRAW";
        if (allySurvivors > 0 && enemySurvivors == 0)
            winner = "ALLY VICTORY!";
        else if (enemySurvivors > 0 && allySurvivors == 0)
            winner = "ENEMY VICTORY!";
        
        summary.AppendLine($"\nResult: {winner}");
        summary.AppendLine("════════════════════════════════════════");
        
        Debug.Log(summary.ToString());
    }
    
    /// <summary>
    /// 통계 초기화
    /// </summary>
    public void Reset()
    {
        allyTeam.Clear();
        enemyTeam.Clear();
        typeStatistics.Clear();
        heroToTypeKey.Clear();
        battleStartFrame = 0;
        battleEndFrame = 0;
        isBattleActive = false;
    }
    
    /// <summary>
    /// 실시간 통계 조회
    /// </summary>
    public Dictionary<string, HeroTypeStatistics> GetAllStatistics()
    {
        return new Dictionary<string, HeroTypeStatistics>(typeStatistics);
    }
    
    /// <summary>
    /// CSV 내보내기
    /// </summary>
    public string ExportToCSV()
    {
        StringBuilder csv = new StringBuilder();
        csv.AppendLine("Team,Type,KindNum,ClassName,TotalSpawned,CurrentActive,Deaths,TotalDamage,DPS,Kills,KDRatio");
        
        int duration = GetBattleDuration();
        foreach (var kvp in typeStatistics.OrderBy(x => x.Value.team).ThenBy(x => x.Value.kindNum))
        {
            var stats = kvp.Value;
            csv.AppendLine($"{stats.team}," +
                          $"{stats.heroName}," +
                          $"{stats.kindNum}," +
                          $"{stats.className}," +
                          $"{stats.totalSpawnedCount}," +
                          $"{stats.currentActiveCount}," +
                          $"{stats.totalDeathCount}," +
                          $"{stats.totalDamageDealt:F0}," +
                          $"{stats.GetDPS(duration):F1}," +
                          $"{stats.kills}," +
                          $"{stats.GetKDRatio():F2}");
        }
        
        return csv.ToString();
    }
}