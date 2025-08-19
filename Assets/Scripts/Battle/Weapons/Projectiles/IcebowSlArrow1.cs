using UnityEngine;

// 화살 무기 구현
public class IcebowSlArrow1 : Arrow
{

    protected override void OnHitTargetSub(BaseHero targetHero)
    {        
        targetHero.Stun(120);
    }
}