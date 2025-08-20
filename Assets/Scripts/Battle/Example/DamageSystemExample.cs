using UnityEngine;

/// <summary>
/// DamageManager 시스템 사용 예제
/// </summary>
public class DamageSystemExample : MonoBehaviour
{
    private BaseHero attacker;
    private BaseHero defender;
    
    void Start()
    {
        // 영웅 찾기
        GameObject[] heroes = GameObject.FindGameObjectsWithTag("Hero");
        if (heroes.Length >= 2)
        {
            attacker = heroes[0].GetComponent<BaseHero>();
            defender = heroes[1].GetComponent<BaseHero>();
        }
    }
    
    void Update()
    {
        // 테스트용 키 입력
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestBasicDamage();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TestBuffedDamage();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TestShieldSystem();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TestDotDamage();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            TestBuffSystem();
        }
    }
    
    // 1. 기본 데미지 테스트
    void TestBasicDamage()
    {
        if (attacker == null || defender == null) return;
        
        // 기본 데미지 100 적용
        Debug.Log("=== Basic Damage Test ===");
        Debug.Log($"Defender HP before: {defender.CurrentHealth}");
        
        attacker.DoDamage(defender, 100f);
        
        Debug.Log($"Defender HP after: {defender.CurrentHealth}");
    }
    
    // 2. 버프가 적용된 데미지 테스트
    void TestBuffedDamage()
    {
        if (attacker == null || defender == null) return;
        
        Debug.Log("=== Buffed Damage Test ===");
        
        // 데미지 버프 VO 생성
        DamageBuffVO buffVO = DamageBuffVO.GetVO();
        buffVO.critChanceUp = 0.5f;        // 크리티컬 확률 50% 증가
        buffVO.critMultiplierUp = 1.0f;    // 크리티컬 배수 1.0 증가
        buffVO.penetrateUp = 0.3f;          // 방어 관통 30% 증가
        
        Debug.Log($"Defender HP before: {defender.CurrentHealth}");
        
        attacker.DoDamage(defender, 100f, buffVO);
        
        Debug.Log($"Defender HP after: {defender.CurrentHealth}");
    }
    
    // 3. 보호막 시스템 테스트
    void TestShieldSystem()
    {
        if (defender == null) return;
        
        Debug.Log("=== Shield System Test ===");
        
        // 보호막 추가
        defender.AddShield(50f);  // 영구 보호막 50
        defender.AddShieldWithDuration(30f, 300);  // 5초(300프레임) 지속 보호막 30
        
        Debug.Log($"Shield: {defender.GetShield()}, Shield with Duration: {defender.GetShieldWithDuration()}");
        
        if (attacker != null)
        {
            // 데미지 적용 (보호막이 먼저 소모됨)
            attacker.DoDamage(defender, 100f);
            
            Debug.Log($"After 100 damage - Shield: {defender.GetShield()}, Shield with Duration: {defender.GetShieldWithDuration()}");
            Debug.Log($"Defender HP: {defender.CurrentHealth}");
        }
    }
    
    // 4. 도트 데미지 테스트 (방어력 무시)
    void TestDotDamage()
    {
        if (attacker == null || defender == null) return;
        
        Debug.Log("=== DOT Damage Test ===");
        Debug.Log($"Defender HP before: {defender.CurrentHealth}");
        Debug.Log($"Defender Defense: {defender.Defense}");
        
        // 도트 데미지는 방어력을 무시함
        attacker.DoDotDamage(defender, 50f);
        
        Debug.Log($"Defender HP after 50 DOT damage: {defender.CurrentHealth}");
    }
    
    // 5. 버프 시스템 테스트
    void TestBuffSystem()
    {
        if (attacker == null) return;
        
        Debug.Log("=== Buff System Test ===");
        Debug.Log($"Original Attack: {attacker.AttackPower}");
        
        // 공격력 버프 추가
        // 같은 ID의 버프는 최대값만 적용
        attacker.AddDamageBuff(BuffType.DAMAGE_A, 0.5f, 600);  // 50% 증가, 10초
        attacker.AddDamageBuff(BuffType.DAMAGE_A, 0.3f, 300);  // 30% 증가, 5초 (무시됨 - 더 작은 값)
        attacker.AddDamageBuff(BuffType.DAMAGE_B, 0.2f, 600);  // 20% 증가, 10초 (다른 ID라 합산)
        
        // 다음 프레임에서 버프가 적용됨
        // (UpdateActiveBuffs가 Execute에서 호출됨)
        
        Debug.Log($"Expected Buffed Attack: {attacker.AttackPower * 1.7f} (50% + 20%)");
        
        // 방어력 버프 추가
        Debug.Log($"Original Defense: {attacker.Defense}");
        attacker.AddDefenseBuff(BuffType.DEFENSE_A, 0.3f, 600);  // 30% 증가, 10초
        
        // 공격속도 버프 추가 (프레임 단위로 적용)
        Debug.Log($"Original Attack Interval: {attacker.AttackIntervalFrames} frames");
        attacker.AddAttackSpeedBuff(BuffType.ATTACK_SPEED_A, 0.5f, 600);  // 50% 빨라짐
        
        // 디버프 테스트 (음수 값)
        attacker.AddMoveSpeedBuff(BuffType.SLOW_FROST, -0.3f, 180);  // 30% 감소, 3초
    }
}