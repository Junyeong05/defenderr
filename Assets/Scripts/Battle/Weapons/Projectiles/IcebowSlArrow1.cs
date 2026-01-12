using UnityEngine;

// 화살 무기 구현
public class IcebowSlArrow1 : Arrow
{

    protected override void OnHitTargetSub(BaseHero targetHero)
    {        
        // targetHero.Stun(30);
        // targetHero.Freeze( 30 );
        // targetHero.Sleep( 120 );
        // targetHero.Knockback( 30, new Vector2( -1, -1 ), 100 );
        // targetHero.KnockbackInstant( new Vector2( -1, -1 ), 100 );
        // targetHero.Silence( 120 );
        // targetHero.Root( 60 );
        targetHero.Poison( 30f, 3000, 60, targetHero, 1 );
        targetHero.DamageBuff1( 1, 30f, 3000, false );

    }
}