using UnityEngine;

// 화살 무기 구현 - BaseWeapon의 포물선 시스템 사용
public class Arrow : BaseWeapon
{
    // BaseWeapon의 T 기반 포물선 시스템을 그대로 사용
    // weaponData.baseGravity로 중력 제어
    // weaponData.rotateToDirection으로 회전 제어
    
    // 필요한 경우에만 추가 기능 구현
    // protected override void OnHitTarget(BaseHero hitTarget)
    // {
    //     base.OnHitTarget(hitTarget);
        
    //     // 화살이 타겟에 박히는 효과 (옵션)
    //     if (!weaponData.destroyOnHit)
    //     {
    //         // 타겟에 고정
    //         transform.SetParent(hitTarget.transform);
            
    //         // 일정 시간 후 제거
    //         Invoke("Remove", 2f);
    //     }
    // }
}