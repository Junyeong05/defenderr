using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 영웅 타입별 전투 통계 데이터
/// 같은 kindNum을 가진 모든 영웅들의 통계를 집계
/// </summary>
[Serializable]
public class HeroTypeStatistics
{
    [Header("Hero Type Information")]
    public int kindNum;                      // 영웅 타입 번호
    public string className;                 // 클래스명 (FootMan1, ElfArcher1 등)
    public string heroName;                  // 표시 이름 (footMan, elfArcher 등)
    public string team;                      // 소속 팀 (ally/enemy)
    
    [Header("Damage Statistics")]
    public float totalDamageDealt = 0f;      // 총 입힌 데미지
    public float totalDamageTaken = 0f;      // 총 받은 데미지
    public float totalDamageBlocked = 0f;    // 총 막은 데미지
    
    public float totalDirectDamageDealt = 0f;    // 직접 데미지
    public float totalDotDamageDealt = 0f;       // DOT 데미지
    public float totalCriticalDamageDealt = 0f;  // 크리티컬 데미지
    
    public int totalHits = 0;                    // 총 타격 수
    public int totalCriticalHits = 0;            // 크리티컬 타격 수
    public int totalDotTicks = 0;                // DOT 틱 수
    
    [Header("Healing Statistics")]
    public float totalHealingDone = 0f;      // 총 시전한 힐
    public float totalHealingReceived = 0f;  // 총 받은 힐
    
    public float totalDirectHealingDone = 0f;    // 직접 힐
    public float totalDotHealingDone = 0f;       // DOT 힐
    
    public int totalHeals = 0;               // 총 힐 시전 수
    public int totalHealDotTicks = 0;        // 힐 DOT 틱 수
    
    [Header("Combat Results")]
    public int kills = 0;                    // 킬 수
    public int assists = 0;                  // 어시스트 수
    public int deaths = 0;                   // 데스 수
    
    [Header("Hero Count Statistics")]
    public int currentActiveCount = 0;       // 현재 활동 중인 영웅 수
    public int totalSpawnedCount = 0;        // 총 생성된 영웅 수
    public int totalDeathCount = 0;          // 총 사망한 영웅 수
    
    [Header("Target Tracking")]
    // 타겟별 데미지 추적
    private Dictionary<string, float> damageByTarget = new Dictionary<string, float>();
    // 타겟별 힐 추적  
    private Dictionary<string, float> healingByTarget = new Dictionary<string, float>();
    // 스킬별 데미지 추적
    private Dictionary<int, float> damageBySkill = new Dictionary<int, float>();
    
    [Header("Individual Hero Tracking")]
    // 개별 영웅 인스턴스 추적 (오브젝트 풀링 대응)
    private HashSet<int> activeHeroInstanceIds = new HashSet<int>();
    
    /// <summary>
    /// 영웅 타입 통계 초기화
    /// </summary>
    public void Initialize(int kindNum, string className, string heroName, string team)
    {
        this.kindNum = kindNum;
        this.className = className;
        this.heroName = heroName;
        this.team = team;
        Reset();
    }
    
    /// <summary>
    /// 데미지 입힌 기록
    /// </summary>
    public void RecordDamageDealt(float damage, BaseHero target = null, bool isDot = false, bool isCritical = false, int skillId = -1)
    {
        totalDamageDealt += damage;
        totalHits++;
        
        if (isDot)
        {
            totalDotDamageDealt += damage;
            totalDotTicks++;
        }
        else
        {
            totalDirectDamageDealt += damage;
        }
        
        if (isCritical)
        {
            totalCriticalDamageDealt += damage;
            totalCriticalHits++;
        }
        
        // 타겟별 데미지 기록
        if (target != null)
        {
            string targetName = target.HeroName;
            if (!damageByTarget.ContainsKey(targetName))
                damageByTarget[targetName] = 0;
            damageByTarget[targetName] += damage;
        }
        
        // 스킬별 데미지 기록
        if (skillId >= 0)
        {
            if (!damageBySkill.ContainsKey(skillId))
                damageBySkill[skillId] = 0;
            damageBySkill[skillId] += damage;
        }
    }
    
    /// <summary>
    /// 데미지 받은 기록
    /// </summary>
    public void RecordDamageTaken(float damage)
    {
        totalDamageTaken += damage;
    }
    
    /// <summary>
    /// 데미지 막은 기록
    /// </summary>
    public void RecordDamageBlocked(float damage)
    {
        totalDamageBlocked += damage;
    }
    
    /// <summary>
    /// 힐링 시전 기록
    /// </summary>
    public void RecordHealingDone(float healing, BaseHero target = null, bool isDot = false)
    {
        totalHealingDone += healing;
        totalHeals++;
        
        if (isDot)
        {
            totalDotHealingDone += healing;
            totalHealDotTicks++;
        }
        else
        {
            totalDirectHealingDone += healing;
        }
        
        // 타겟별 힐 기록
        if (target != null)
        {
            string targetName = target.HeroName;
            if (!healingByTarget.ContainsKey(targetName))
                healingByTarget[targetName] = 0;
            healingByTarget[targetName] += healing;
        }
    }
    
    /// <summary>
    /// 힐링 받은 기록
    /// </summary>
    public void RecordHealingReceived(float healing)
    {
        totalHealingReceived += healing;
    }
    
    /// <summary>
    /// 킬 기록
    /// </summary>
    public void RecordKill()
    {
        kills++;
    }
    
    /// <summary>
    /// 어시스트 기록
    /// </summary>
    public void RecordAssist()
    {
        assists++;
    }
    
    /// <summary>
    /// 데스 기록
    /// </summary>
    public void RecordDeath()
    {
        deaths++;
    }
    
    /// <summary>
    /// 영웅 스폰 시 호출
    /// </summary>
    public void OnHeroSpawned(int instanceId)
    {
        if (!activeHeroInstanceIds.Contains(instanceId))
        {
            activeHeroInstanceIds.Add(instanceId);
            currentActiveCount++;
            totalSpawnedCount++;
        }
    }
    
    /// <summary>
    /// 영웅 사망 시 호출
    /// </summary>
    public void OnHeroDied(int instanceId)
    {
        if (activeHeroInstanceIds.Contains(instanceId))
        {
            activeHeroInstanceIds.Remove(instanceId);
            currentActiveCount--;
            totalDeathCount++;
        }
    }
    
    /// <summary>
    /// 영웅이 풀로 반환될 때 호출 (재사용 대비)
    /// </summary>
    public void OnHeroReturnedToPool(int instanceId)
    {
        if (activeHeroInstanceIds.Contains(instanceId))
        {
            activeHeroInstanceIds.Remove(instanceId);
            currentActiveCount--;
        }
    }
    
    /// <summary>
    /// DPS 계산
    /// </summary>
    public float GetDPS(int totalFrames)
    {
        if (totalFrames <= 0) return 0;
        float seconds = totalFrames / 60f; // 60 FPS 기준
        return totalDamageDealt / seconds;
    }
    
    /// <summary>
    /// 평균 DPS 계산 (활동 중인 영웅 기준)
    /// </summary>
    public float GetAverageDPS(int totalFrames)
    {
        if (currentActiveCount <= 0 || totalFrames <= 0) return 0;
        float totalDPS = GetDPS(totalFrames);
        return totalDPS / currentActiveCount;
    }
    
    /// <summary>
    /// HPS 계산
    /// </summary>
    public float GetHPS(int totalFrames)
    {
        if (totalFrames <= 0) return 0;
        float seconds = totalFrames / 60f;
        return totalHealingDone / seconds;
    }
    
    /// <summary>
    /// 평균 HPS 계산 (활동 중인 힐러 기준)
    /// </summary>
    public float GetAverageHPS(int totalFrames)
    {
        if (currentActiveCount <= 0 || totalFrames <= 0) return 0;
        float totalHPS = GetHPS(totalFrames);
        return totalHPS / currentActiveCount;
    }
    
    /// <summary>
    /// 크리티컬 확률 계산
    /// </summary>
    public float GetCriticalRate()
    {
        if (totalHits <= 0) return 0;
        return (float)totalCriticalHits / totalHits * 100f;
    }
    
    /// <summary>
    /// 영웅당 평균 데미지 (총 생성된 영웅 기준)
    /// </summary>
    public float GetDamagePerHero()
    {
        if (totalSpawnedCount <= 0) return 0;
        return totalDamageDealt / totalSpawnedCount;
    }
    
    /// <summary>
    /// 영웅당 평균 힐링 (총 생성된 영웅 기준)
    /// </summary>
    public float GetHealingPerHero()
    {
        if (totalSpawnedCount <= 0) return 0;
        return totalHealingDone / totalSpawnedCount;
    }
    
    /// <summary>
    /// 생존율 계산
    /// </summary>
    public float GetSurvivalRate()
    {
        if (totalSpawnedCount <= 0) return 0;
        return (float)currentActiveCount / totalSpawnedCount * 100f;
    }
    
    /// <summary>
    /// K/D 비율 계산
    /// </summary>
    public float GetKDRatio()
    {
        if (totalDeathCount <= 0) return kills;
        return (float)kills / totalDeathCount;
    }
    
    /// <summary>
    /// 효율성 점수 계산 (데미지 딜링 vs 받은 데미지)
    /// </summary>
    public float GetEfficiencyScore()
    {
        if (totalDamageTaken <= 0) return totalDamageDealt;
        return totalDamageDealt / totalDamageTaken;
    }
    
    /// <summary>
    /// 타겟별 데미지 정보 가져오기
    /// </summary>
    public Dictionary<string, float> GetDamageByTarget()
    {
        return new Dictionary<string, float>(damageByTarget);
    }
    
    /// <summary>
    /// 타겟별 힐 정보 가져오기
    /// </summary>
    public Dictionary<string, float> GetHealingByTarget()
    {
        return new Dictionary<string, float>(healingByTarget);
    }
    
    /// <summary>
    /// 스킬별 데미지 정보 가져오기
    /// </summary>
    public Dictionary<int, float> GetDamageBySkill()
    {
        return new Dictionary<int, float>(damageBySkill);
    }
    
    /// <summary>
    /// 통계 초기화
    /// </summary>
    public void Reset()
    {
        // 데미지 통계
        totalDamageDealt = 0f;
        totalDamageTaken = 0f;
        totalDamageBlocked = 0f;
        totalDirectDamageDealt = 0f;
        totalDotDamageDealt = 0f;
        totalCriticalDamageDealt = 0f;
        totalHits = 0;
        totalCriticalHits = 0;
        totalDotTicks = 0;
        
        // 힐링 통계
        totalHealingDone = 0f;
        totalHealingReceived = 0f;
        totalDirectHealingDone = 0f;
        totalDotHealingDone = 0f;
        totalHeals = 0;
        totalHealDotTicks = 0;
        
        // 전투 결과
        kills = 0;
        assists = 0;
        deaths = 0;
        
        // 영웅 카운트
        currentActiveCount = 0;
        totalSpawnedCount = 0;
        totalDeathCount = 0;
        
        // 추적 데이터
        damageByTarget.Clear();
        healingByTarget.Clear();
        damageBySkill.Clear();
        activeHeroInstanceIds.Clear();
    }
    
    /// <summary>
    /// 타입별 통계 요약 문자열
    /// </summary>
    public string GetTypeSummary(int totalFrames)
    {
        float seconds = totalFrames / 60f;
        
        return $"=== {heroName} ({team}) - Type #{kindNum} ===\n" +
               $"[Deployment]\n" +
               $"Total Spawned: {totalSpawnedCount} | Active: {currentActiveCount} | Deaths: {totalDeathCount}\n" +
               $"Survival Rate: {GetSurvivalRate():F1}%\n" +
               $"\n[Damage Output]\n" +
               $"Total: {totalDamageDealt:F0} ({GetDPS(totalFrames):F1} DPS)\n" +
               $"Average per Hero: {GetDamagePerHero():F0} | Average DPS: {GetAverageDPS(totalFrames):F1}\n" +
               $"Direct: {totalDirectDamageDealt:F0} | DOT: {totalDotDamageDealt:F0}\n" +
               $"Critical: {totalCriticalDamageDealt:F0} ({GetCriticalRate():F1}%)\n" +
               $"\n[Healing Output]\n" +
               $"Total: {totalHealingDone:F0} ({GetHPS(totalFrames):F1} HPS)\n" +
               $"Average per Hero: {GetHealingPerHero():F0} | Average HPS: {GetAverageHPS(totalFrames):F1}\n" +
               $"\n[Defense]\n" +
               $"Damage Taken: {totalDamageTaken:F0} | Healing Received: {totalHealingReceived:F0}\n" +
               $"Damage Blocked: {totalDamageBlocked:F0}\n" +
               $"\n[Performance]\n" +
               $"K/D Ratio: {GetKDRatio():F2} ({kills}/{totalDeathCount})\n" +
               $"Efficiency Score: {GetEfficiencyScore():F2}";
    }
    
    /// <summary>
    /// 매치업 분석용 데이터
    /// </summary>
    public Dictionary<string, float> GetMatchupData()
    {
        var matchupData = new Dictionary<string, float>();
        
        // 타겟별 데미지를 영웅 타입별로 재집계
        foreach (var kvp in GetDamageByTarget())
        {
            // kvp.Key는 개별 영웅 이름이므로, 타입으로 변환 필요
            // 이 부분은 BattleStatisticsManager에서 처리하는 것이 더 적절
            matchupData[kvp.Key] = kvp.Value;
        }
        
        return matchupData;
    }
}