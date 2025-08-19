// 이펙트 타입 열거형
public enum EffectType
{
    PHYSICAL_HIT,
    MAGIC_HIT,
    
    // 힐/버프 효과
    HEAL,
    BUFF_DAMAGE_UP,
    BUFF_DEFENSE_UP,
    BUFF_SPEED_UP,
    SHIELD,
    
    // 상태이상 효과
    STUN,
    FREEZE,
    BURN,
    POISON,
    SLOW,
    SLEEP,
    
    // 타격 효과
    HIT_NORMAL,
    HIT_CRITICAL,
    HIT_MAGIC,
    HIT_ARROW,
    EXPLOSION,
    
    // 기타 효과
    LEVEL_UP,
    SUMMON,
    TELEPORT,
    DEATH
}