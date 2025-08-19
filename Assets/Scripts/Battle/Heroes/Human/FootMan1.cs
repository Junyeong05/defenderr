using UnityEngine;

/// <summary>
/// FootMan 영웅 구현
/// 근접 전사, 높은 방어력과 체력
/// </summary>
public class FootMan1 : BaseHero
{
    [Header("FootMan Specific")]
    [SerializeField] private float blockChance = 10f;        // 방어 확률
    [SerializeField] private float blockDamageReduction = 0.5f; // 방어 시 데미지 감소율
    [SerializeField] private GameObject shieldEffect;         // 방어 이펙트
    
    private float tauntRadius = 5f;  // 도발 범위
    
    protected override void OnInitialize()
    {
    }
    
    /// <summary>
    /// AS3.0/PixiJS 스타일 - 필요시 텍스처 이름이나 시트 이름 재정의
    /// </summary>
    protected override void InitializeTextureName()
    {
        // 기본적으로 className("FootMan")을 사용
        // 만약 다른 텍스처 이름이 필요하면 여기서 수정
        // 예: className = "foot_man"; // 파일명과 다른 경우
    }
    
    protected override void UpdateLogic()
    {
        // 레거시 메서드 - 삭제 예정
    }
    
    /// <summary>
    /// FootMan의 대기 상태 처리
    /// </summary>
    protected override void DoWait()
    {
        base.DoWait();
        
        // FootMan 특수: 일정 확률로 도발 스킬 사용
        if (framesSinceLastAttack >= attackIntervalFrames && UnityEngine.Random.Range(0f, 100f) < 10f)
        {
            // 주변에 적이 2명 이상이면 도발
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, tauntRadius);
            int enemyCount = 0;
            foreach (var enemy in enemies)
            {
                BaseHero enemyHero = enemy.GetComponent<BaseHero>();
                // IsPlayerTeam 속성을 사용하여 적 판별 (FootMan이 플레이어 팀이면 적은 플레이어 팀이 아닌 것)
                if (enemyHero != null && enemyHero.IsAlive && enemyHero.IsPlayerTeam != this.IsPlayerTeam)
                {
                    enemyCount++;
                }
            }
            
            if (enemyCount >= 2)
            {
                Taunt();
            }
        }
    }
    
    public override void TakeDamage(float damage)
    {
        if (!isAlive) return;
        
        // 방어 판정
        if (Random.Range(0f, 100f) < blockChance)
        {
            // 방어 성공
            damage *= blockDamageReduction;
            
            Debug.Log($"[FootMan1] Blocked! Reduced damage to {damage}");
            
            // 방어 이펙트 표시
            if (shieldEffect != null)
            {
                GameObject effect = Instantiate(shieldEffect, transform.position, Quaternion.identity);
                Destroy(effect, 1f);
            }
        }
        
        base.TakeDamage(damage);
    }
    
    /// <summary>
    /// FootMan의 근거리 공격 재정의
    /// </summary>
    protected override void DoMeleeAttack()
    {
        // 기본 근거리 공격 실행
        base.DoMeleeAttack();
        
        // FootMan 특수: 일정 확률로 기절 효과 추가
        if (Random.Range(0f, 100f) < 15f) // 15% 확률
        {
            Debug.Log($"[FootMan1] Stunning blow!");
            
            // 기절 이펙트 추가 예시 (이펙트 프리팹이 있다면)
            if (target != null && shieldEffect != null) // shieldEffect를 임시로 사용
            {
                // 타겟 위치에 기절 이펙트 생성                
            }
        }
    }
    
    protected override void OnHealth20()
    {
        base.OnHealth20();
        
        // FootMan 특수 능력: 방어력 증가
        defense *= 1.5f;
        blockChance += 20f;
        Debug.Log($"[FootMan] Last Stand activated! Defense increased!");
    }
    
    protected override void OnKillEnemy(BaseHero enemy)
    {
        base.OnKillEnemy(enemy);
        
        // FootMan 특수 능력: 체력 회복
        float healAmount = maxHealth * 0.1f;
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        Debug.Log($"[FootMan1] Healed {healAmount} HP after kill!");
    }
    
    /// <summary>
    /// 도발 스킬 - 주변 적들의 타겟을 자신으로 변경
    /// </summary>
    public void Taunt()
    {
        if (!isAlive) return;
        
        SetState(STATE_SKILL, false);
        
        // 도발 범위 내의 모든 적들 찾기
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, tauntRadius);
        
        foreach (Collider2D enemy in enemies)
        {
            BaseHero enemyHero = enemy.GetComponent<BaseHero>();
            // IsPlayerTeam 속성을 사용하여 적 판별
            if (enemyHero != null && enemyHero.IsAlive && enemyHero.IsPlayerTeam != this.IsPlayerTeam)
            {
                enemyHero.SetTarget(transform);
            }
        }
        
        Debug.Log($"[FootMan1] Taunted enemies in {tauntRadius} radius!");
    }
    
    public override void ResetHero()
    {
        base.ResetHero();
        
        // FootMan 특유 리셋
        blockChance = 10f;
    }
    
    // Inspector에서 도발 범위 표시
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, tauntRadius);
    }
}