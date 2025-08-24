using UnityEngine;

/// <summary>
/// DOT(Damage Over Time) 시스템 사용 예제
/// </summary>
public class DotSystemExample : MonoBehaviour
{
    private BaseHero attacker;
    private BaseHero target;
    private BaseHero healer;
    
    void Start()
    {
        // 영웅 찾기
        GameObject[] heroes = GameObject.FindGameObjectsWithTag("Hero");
        if (heroes.Length >= 3)
        {
            attacker = heroes[0].GetComponent<BaseHero>();
            target = heroes[1].GetComponent<BaseHero>();
            healer = heroes[2].GetComponent<BaseHero>();
        }
    }
    
    void Update()
    {
        // 테스트용 키 입력
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestPoisonDot();  // 독 데미지
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TestBurnDot();    // 화상 데미지
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TestBleedDot();   // 출혈 데미지
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TestHealOverTime();  // 지속 힐
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            TestMultipleDots();  // 여러 DOT 동시 적용
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            RemoveAllDots();  // 모든 DOT 제거
        }
    }
    
    // 1. 독 데미지 테스트 (3초간 0.5초마다 10 데미지)
    void TestPoisonDot()
    {
        if (attacker == null || target == null) return;
        
        Debug.Log("=== Poison DOT Test ===");
        Debug.Log($"Target HP before: {target.CurrentHealth}");
        
        // 독 데미지: 10 데미지, 180 프레임(3초), 30 프레임(0.5초)마다
        // ID: 1001 (독 데미지 ID)
        target.AddDotDamage(10f, 180, 30, attacker, 1001);
        
        Debug.Log("Poison applied: 10 damage every 0.5 seconds for 3 seconds");
        Debug.Log("Total expected damage: 60 (6 ticks)");
    }
    
    // 2. 화상 데미지 테스트 (5초간 1초마다 20 데미지)
    void TestBurnDot()
    {
        if (attacker == null || target == null) return;
        
        Debug.Log("=== Burn DOT Test ===");
        Debug.Log($"Target HP before: {target.CurrentHealth}");
        
        // 화상 데미지: 20 데미지, 300 프레임(5초), 60 프레임(1초)마다
        // ID: 1002 (화상 데미지 ID)
        target.AddDotDamage(20f, 300, 60, attacker, 1002);
        
        Debug.Log("Burn applied: 20 damage every 1 second for 5 seconds");
        Debug.Log("Total expected damage: 100 (5 ticks)");
    }
    
    // 3. 출혈 데미지 테스트 (2초간 0.2초마다 5 데미지)
    void TestBleedDot()
    {
        if (attacker == null || target == null) return;
        
        Debug.Log("=== Bleed DOT Test ===");
        Debug.Log($"Target HP before: {target.CurrentHealth}");
        
        // 출혈 데미지: 5 데미지, 120 프레임(2초), 12 프레임(0.2초)마다
        // ID: 1003 (출혈 데미지 ID)
        target.AddDotDamage(5f, 120, 12, attacker, 1003);
        
        Debug.Log("Bleed applied: 5 damage every 0.2 seconds for 2 seconds");
        Debug.Log("Total expected damage: 50 (10 ticks)");
    }
    
    // 4. 지속 힐 테스트 (10초간 1초마다 15 힐)
    void TestHealOverTime()
    {
        if (healer == null || target == null) return;
        
        Debug.Log("=== Heal Over Time Test ===");
        Debug.Log($"Target HP before: {target.CurrentHealth}/{target.MaxHealth}");
        
        // 먼저 대상에게 데미지를 줌
        target.TakeDamage(100f);
        Debug.Log($"After 100 damage: {target.CurrentHealth}/{target.MaxHealth}");
        
        // 지속 힐: 15 힐, 600 프레임(10초), 60 프레임(1초)마다
        target.AddDotHeal(15f, 600, 60, healer);
        
        Debug.Log("HOT applied: 15 heal every 1 second for 10 seconds");
        Debug.Log("Total expected healing: 150 (10 ticks)");
    }
    
    // 5. 여러 DOT 동시 적용 테스트
    void TestMultipleDots()
    {
        if (attacker == null || target == null || healer == null) return;
        
        Debug.Log("=== Multiple DOTs Test ===");
        Debug.Log($"Target HP before: {target.CurrentHealth}");
        
        // 독 데미지
        target.AddDotDamage(10f, 180, 30, attacker, 1001);
        
        // 화상 데미지 (다른 ID)
        target.AddDotDamage(15f, 240, 60, attacker, 1002);
        
        // 지속 힐
        target.AddDotHeal(8f, 300, 30, healer);
        
        Debug.Log("Applied:");
        Debug.Log("- Poison: 10 damage every 0.5s for 3s");
        Debug.Log("- Burn: 15 damage every 1s for 4s");
        Debug.Log("- HOT: 8 heal every 0.5s for 5s");
        
        // 같은 ID로 독 데미지 갱신 (덮어쓰기)
        Debug.Log("\nUpdating poison with stronger effect...");
        target.AddDotDamage(20f, 180, 30, attacker, 1001);
        Debug.Log("Poison updated to: 20 damage every 0.5s for 3s");
    }
    
    // 6. 모든 DOT 제거
    void RemoveAllDots()
    {
        if (target == null) return;
        
        Debug.Log("=== Remove All DOTs ===");
        target.RemoveAllDots();
        Debug.Log("All DOT effects removed from target");
    }
    
    void OnGUI()
    {
        if (target != null)
        {
            GUI.Box(new Rect(10, 10, 300, 100), "DOT System Test");
            
            GUI.Label(new Rect(20, 40, 280, 20), 
                $"Target HP: {target.CurrentHealth:F0}/{target.MaxHealth:F0}");
            
            // DOT 상태 표시
            string dotStatus = "Active DOTs: ";
            if (target.GetComponent<BaseHero>() != null)
            {
                // DOT 개수 표시 (실제 리스트 접근은 protected이므로 public 프로퍼티 필요)
                dotStatus += "[Check console for details]";
            }
            
            GUI.Label(new Rect(20, 60, 280, 20), dotStatus);
            
            GUI.Label(new Rect(20, 80, 280, 20), 
                "Press 1-6 to test different DOT effects");
        }
    }
}