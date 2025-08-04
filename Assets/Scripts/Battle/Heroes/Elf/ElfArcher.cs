using UnityEngine;

/// <summary>
/// ElfArcher 영웅 구현
/// 원거리 공격, 높은 명중률과 크리티컬
/// </summary>
public class ElfArcher : BaseHero
{
    [Header("ElfArcher Specific")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float arrowSpeed = 15f; // TODO: 투사체 시스템 구현 시 사용
    
    // 엘프 특성
    private float accuracyBonus = 0.95f;      // 95% 명중률
    private float doubleShootChance = 20f;    // 더블샷 확률
    private bool isAiming = false;
    private float aimingTime = 0.5f;          // 조준 시간
    private float currentAimTime = 0f;
    
    protected override void Initialize()
    {
        Debug.Log($"[ElfArcher] Initialized {HeroName} Level {level}");
        
        // ElfArcher 특유의 초기화
        if (shootPoint == null)
        {
            // 발사 지점이 없으면 자동 생성
            GameObject shootPointObj = new GameObject("ShootPoint");
            shootPointObj.transform.SetParent(transform);
            shootPointObj.transform.localPosition = new Vector3(0.8f, 0.3f, 0);
            shootPoint = shootPointObj.transform;
        }
        
        // 엘프 특성: 높은 크리티컬 확률
        if (heroData != null)
        {
            heroData.criticalChance *= 1.2f;  // 20% 증가
        }
    }
    
    /// <summary>
    /// AS3.0/PixiJS 스타일 - 필요시 텍스처 이름이나 시트 이름 재정의
    /// </summary>
    protected override void InitializeTextureName()
    {
        // 기본적으로 className("ElfArcher")을 사용
        // 만약 다른 텍스처 이름이 필요하면 여기서 수정
        // 예: className = "elf_archer"; // 파일명과 다른 경우
        
        // 다른 시트를 사용하는 경우
        // sheetName = SheetNames.HEROES2;
    }
    
    protected override void UpdateLogic()
    {
        // 레거시 메서드 - 삭제 예정
    }
    
    /// <summary>
    /// ElfArcher의 대기 상태 처리
    /// </summary>
    protected override void DoWait()
    {
        // 타겟이 없으면 찾기
        if (target == null)
        {
            target = FindNearestEnemy();
        }
        
        if (target != null)
        {
            float distance = Vector2.Distance(transform.position, target.position);
            float optimalRange = heroData.attackRange * 0.8f;
            
            // 거리에 따른 행동 결정
            if (distance > heroData.attackRange)
            {
                // 사거리 밖이면 이동
                SetState(STATE_MOVE);
            }
            else if (distance < optimalRange * 0.5f)
            {
                // 너무 가까우면 후퇴
                SetState(STATE_MOVE);
            }
            else if (framesSinceLastAttack >= attackIntervalFrames && !isAiming)
            {
                // 최적 거리에서 공격 상태로 전환
                BaseHero targetHero = target.GetComponent<BaseHero>();
                if (targetHero != null && targetHero.IsAlive)
                {
                    GotoAttackState(targetHero);
                    StartAiming();
                }
            }
        }
    }
    
    /// <summary>
    /// ElfArcher의 이동 상태 처리
    /// </summary>
    protected override void DoMove()
    {
        if (target == null)
        {
            SetState(STATE_WAIT);
            return;
        }
        
        float distance = Vector2.Distance(transform.position, target.position);
        float optimalRange = heroData.attackRange * 0.8f;
        
        if (distance < optimalRange * 0.5f)
        {
            // 너무 가까우면 후퇴
            MoveAwayFromTarget();
        }
        else
        {
            // 적절한 거리로 이동
            MoveTowardsTarget();
        }
        
        // 최적 거리에 도달했는지 체크
        if (distance >= optimalRange * 0.7f && distance <= optimalRange)
        {
            SetState(STATE_WAIT);
        }
    }
    
    /// <summary>
    /// ElfArcher의 공격 상태 처리
    /// </summary>
    protected override void DoAttack()
    {
        base.DoAttack();
        
        // 조준 처리
        if (isAiming)
        {
            currentAimTime += Time.deltaTime;
            if (currentAimTime >= aimingTime)
            {
                isAiming = false;
                currentAimTime = 0f;
                // 실제 공격은 AttackMain()에서 처리
            }
        }
    }
    
    
    private void MoveAwayFromTarget()
    {
        if (state != STATE_MOVE)
        {
            SetState(STATE_MOVE);
        }
        
        Vector3 direction = (transform.position - target.position).normalized;
        transform.position += direction * heroData.moveSpeed * 0.7f * Time.deltaTime; // 후퇴는 약간 느림
        
        // 후퇴할 때도 적을 바라봄 (x축 차이만 체크)
        UpdateFacing(target.position.x - transform.position.x);
    }
    
    private void StartAiming()
    {
        if (!isAiming && !isAttacking)
        {
            isAiming = true;
            currentAimTime = 0f;
            // 조준 애니메이션이 있다면 여기서 재생
        }
    }
    
    /// <summary>
    /// ElfArcher의 원거리 공격 재정의
    /// </summary>
    protected override void DoRangeAttack()
    {
        if (target == null) return;
        
        // 화살 발사
        ShootArrow();
        
        // 더블샷 판정
        if (Random.Range(0f, 100f) < doubleShootChance)
        {
            Invoke("ShootSecondArrow", 0.1f);
        }
    }
    
    private void ShootArrow()
    {
        if (arrowPrefab == null || shootPoint == null || target == null) return;
        
        // 명중률 판정
        if (Random.Range(0f, 1f) > accuracyBonus)
        {
            Debug.Log($"[ElfArcher] Miss!");
            return;
        }
        
        // 크리티컬 판정
        bool isCritical = Random.Range(0f, 100f) < heroData.criticalChance;
        float damage = attackPower;
        
        if (isCritical)
        {
            damage *= heroData.criticalMultiplier;
            Debug.Log($"[ElfArcher] Critical Hit! Damage: {damage}");
        }
        
        // 화살 생성
        if (arrowPrefab != null)
        {
            GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, Quaternion.identity);
            
            // 화살 방향 설정
            Vector2 direction = (target.position - shootPoint.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            // 화살 컴포넌트 설정 (Arrow 스크립트가 있다고 가정)
            // Arrow arrowScript = arrow.GetComponent<Arrow>();
            // if (arrowScript != null)
            // {
            //     arrowScript.Initialize(target, damage, arrowSpeed, isCritical);
            // }
            
            // 임시로 화살 제거
            Destroy(arrow, 2f);
        }
        
        // 직접 데미지 처리
        BaseHero targetHero = target.GetComponent<BaseHero>();
        if (targetHero != null)
        {
            targetHero.TakeDamage(damage);
            
            // 적을 죽였는지 확인
            if (!targetHero.IsAlive)
            {
                OnKillEnemy(targetHero);
            }
        }
    }
    
    private void ShootSecondArrow()
    {
        Debug.Log($"[ElfArcher] Double Shot!");
        ShootArrow();
    }
    
    protected override void OnHealth20()
    {
        base.OnHealth20();
        
        // ElfArcher 특수 능력: 공격 속도 증가, 회피율 증가
        animationSpeed = 1.5f;
        heroData.dodgeChance += 30f;
        aimingTime = 0.2f;  // 빠른 조준
        Debug.Log($"[ElfArcher] Elven Focus activated! Speed and dodge increased!");
    }
    
    protected override void OnKillEnemy(BaseHero enemy)
    {
        base.OnKillEnemy(enemy);
        
        // ElfArcher 특수 능력: 다음 공격 자동 크리티컬
        heroData.criticalChance = 100f;
        Invoke("ResetCriticalChance", 3f); // 3초 후 원래대로
        Debug.Log($"[ElfArcher] Hunter's Mark! Next shot will be critical!");
    }
    
    private void ResetCriticalChance()
    {
        heroData.criticalChance = heroData.criticalChance / 1.2f; // 초기화에서 증가시킨 만큼 감소
    }
    
    protected override void OnFriendDie(BaseHero friend)
    {
        base.OnFriendDie(friend);
        
        // ElfArcher 특수 능력: 복수의 화살
        doubleShootChance += 20f;
        Debug.Log($"[ElfArcher] Vengeance! Double shot chance increased!");
    }
    
    public override void TakeDamage(float damage)
    {
        // 회피 판정
        if (Random.Range(0f, 100f) < heroData.dodgeChance)
        {
            Debug.Log($"[ElfArcher] Dodged!");
            
            // 회피 이펙트가 있다면 표시
            // TODO: 회피 이펙트 표시
            return;
        }
        
        base.TakeDamage(damage);
    }
    
    public override void ResetHero()
    {
        base.ResetHero();
        
        // ElfArcher 특유 리셋
        isAiming = false;
        currentAimTime = 0f;
        doubleShootChance = 20f;
        aimingTime = 0.5f;
    }
    
    // Inspector에서 사거리 표시
    private void OnDrawGizmosSelected()
    {
        if (heroData != null)
        {
            // 최대 사거리
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, heroData.attackRange);
            
            // 최적 사거리
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, heroData.attackRange * 0.8f);
            
            // 최소 거리
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, heroData.attackRange * 0.4f);
        }
    }
}