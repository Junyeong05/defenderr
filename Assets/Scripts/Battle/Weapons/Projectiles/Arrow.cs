using UnityEngine;

// 화살 무기 구현
public class Arrow : BaseWeapon
{
    [Header("Arrow Specific")]
    protected float gravityScale = 0.1f;  // 중력 영향 (SerializeField 제거)
    protected bool rotateInFlight = true;  // 비행 중 회전 (SerializeField 제거)
    
    protected override void InitProjectile()
    {
        base.InitProjectile();
        
        // 화살 특유의 포물선 궤적을 위한 초기 각도 조정
        if (target != null)
        {
            // 거리에 따라 발사 각도 조정 (포물선 궤적)
            float distance = Vector2.Distance(transform.position, target.transform.position);
            float angleAdjustment = Mathf.Min(distance * 0.1f, 15f);  // 최대 15도
            
            // 위쪽으로 약간 각도 추가
            shootAngle += angleAdjustment;
            transform.rotation = Quaternion.Euler(0, 0, shootAngle);
            
            // 속도 재계산
            velocity = new Vector2(
                Mathf.Cos(shootAngle * Mathf.Deg2Rad) * weaponData.initialSpeed,
                Mathf.Sin(shootAngle * Mathf.Deg2Rad) * weaponData.initialSpeed
            );
        }
    }
    
    protected override void UpdateProjectile()
    {
        // 기본 이동
        base.UpdateProjectile();
        
        // 중력 적용
        velocity.y -= gravityScale;
        
        // 비행 중 회전 (속도 방향으로)
        if (rotateInFlight && velocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        
        // 화면 밖으로 나가면 제거
        if (Mathf.Abs(transform.position.x) > 1000f || Mathf.Abs(transform.position.y) > 1000f)
        {
            Remove();
        }
    }
    
    protected override void OnHitTarget(BaseHero hitTarget)
    {
        base.OnHitTarget(hitTarget);
        
        // 화살이 타겟에 박히는 효과 (옵션)
        if (!weaponData.destroyOnHit)
        {
            // 타겟에 고정
            transform.SetParent(hitTarget.transform);
            velocity = Vector2.zero;
            
            // 일정 시간 후 제거
            Invoke("Remove", 2f);
        }
    }
}