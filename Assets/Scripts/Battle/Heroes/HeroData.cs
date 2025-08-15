using UnityEngine;

/// <summary>
/// 영웅 데이터를 담는 ScriptableObject
/// 기존 HeroBook 엑셀 데이터를 Unity에서 관리
/// </summary>
[CreateAssetMenu(fileName = "HeroData", menuName = "Heroes/Hero Data", order = 1)]
public class HeroData : ScriptableObject
{
    [Header("Hero Identity")]
    public int kindNum;            // 병사 종류 번호 (1001, 1002, 1003...)
    public string heroName;        // 표시 이름 (Archer)
    public string heroClass;       // 직업 (Archer, Warrior, Mage 등)
    
    [Header("Visual Settings")]
    public string sheetName = "atlases/Battle";  // 텍스처 아틀라스 경로
    public string spriteName;      // 스프라이트 프리픽스 (Archer)
    
    [Header("Animation Frame Indices")]
    [Space(10)]
    [Tooltip("대기 애니메이션 프레임")]
    public int startWait = 0;
    public int endWait = 19;
    
    [Tooltip("이동 애니메이션 프레임")]
    public int startMove = 20;
    public int endMove = 39;
    
    [Tooltip("공격 애니메이션 프레임")]
    public int startAttack = 40;
    public int endAttack = 59;
    
    [Tooltip("스킬 애니메이션 프레임")]
    public int startSkill = 60;
    public int endSkill = 89;
    
    [Tooltip("죽음 애니메이션 프레임")]
    public int startDie = 90;
    public int endDie = 119;
    
    [Header("Base Stats")]
    [Space(10)]
    public float maxHealth = 100f;
    public float attackPower = 10f;
    public float defense = 5f;
    public float moveSpeed = 5f;
    public float attackSpeed = 1f;   // 공격 애니메이션 속도 배수
    public float attackRange = 5f;
    public int attackInterval = 60;  // 공격 인터벌 (프레임 단위, 60 = 1초)
    
    [Header("Growth Stats (Per Level)")]
    [Space(10)]
    public float healthPerLevel = 10f;
    public float attackPerLevel = 2f;
    public float defensePerLevel = 1f;
    
    [Header("Special Properties")]
    [Space(10)]
    [Tooltip("크리티컬 확률 (0-100)")]
    [Range(0f, 100f)]
    public float criticalChance = 5f;
    
    [Tooltip("크리티컬 데미지 배수")]
    public float criticalMultiplier = 2f;
    
    [Tooltip("회피 확률 (0-100)")]
    [Range(0f, 100f)]
    public float dodgeChance = 0f;
    
    [Header("Attack Type")]
    [Tooltip("근거리/원거리 구분")]
    public bool isRanged = false;
    
    [Tooltip("무기 클래스 (원거리 공격시 사용할 무기)")]
    public string weaponClass = "";  // Arrow, Bullet, Lightning 등
    
    [Header("Attack Frame Triggers")]
    [Tooltip("공격이 발생하는 프레임 번호들 (AS3.0 스타일)")]
    public int[] attackTriggerFrames = new int[] { 32, 38 };
    
    
    /// <summary>
    /// 레벨에 따른 스탯 계산
    /// </summary>
    public float GetMaxHealth(int level)
    {
        return maxHealth + (healthPerLevel * (level - 1));
    }
    
    public float GetAttackPower(int level)
    {
        return attackPower + (attackPerLevel * (level - 1));
    }
    
    public float GetDefense(int level)
    {
        return defense + (defensePerLevel * (level - 1));
    }
}