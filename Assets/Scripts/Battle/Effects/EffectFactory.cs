using UnityEngine;
using System.Collections.Generic;

// 간단한 이펙트 팩토리 - 정적 메서드로 사용
public static class EffectFactory
{
    // 이펙트 타입별 설정
    private static readonly Dictionary<EffectType, EffectConfig> effectConfigs = new Dictionary<EffectType, EffectConfig>
    {
        // 힐/버프 효과
        { EffectType.HEAL, new EffectConfig("Heal", 0.5f, false, 60, 1.2f) },
        { EffectType.BUFF_DAMAGE_UP, new EffectConfig("BuffDamageUp", 0.3f, true, 180, 1f) },
        { EffectType.BUFF_DEFENSE_UP, new EffectConfig("BuffDefenseUp", 0.3f, true, 180, 1f) },
        { EffectType.BUFF_SPEED_UP, new EffectConfig("BuffSpeedUp", 0.3f, true, 180, 1f) },
        { EffectType.SHIELD, new EffectConfig("Shield", 0.4f, true, 120, 1.3f) },
        
        // 상태이상 효과
        { EffectType.STUN, new EffectConfig("Stun", 0.4f, true, 90, 1f) },
        { EffectType.FREEZE, new EffectConfig("Freeze", 0.3f, false, 60, 1.2f) },
        { EffectType.BURN, new EffectConfig("Burn", 0.5f, true, 120, 0.8f) },
        { EffectType.POISON, new EffectConfig("Poison", 0.3f, true, 150, 0.9f) },
        { EffectType.SLOW, new EffectConfig("Slow", 0.3f, true, 90, 1f) },
        
        // 타격 효과
        { EffectType.PHYSICAL_HIT, new EffectConfig("PhysicallHitEffect", 0.8f, false, 20, 2.9f) },
        { EffectType.MAGIC_HIT, new EffectConfig("MagicHitEffect", 0.6f, false, 30, 2.9f) },

        { EffectType.HIT_NORMAL, new EffectConfig("HitNormal", 0.8f, false, 20, 1f) },
        { EffectType.HIT_CRITICAL, new EffectConfig("HitCritical", 0.6f, false, 30, 1.5f) },
        { EffectType.HIT_MAGIC, new EffectConfig("HitMagic", 0.5f, false, 40, 1.2f) },
        { EffectType.HIT_ARROW, new EffectConfig("HitArrow", 0.7f, false, 25, 0.8f) },
        { EffectType.EXPLOSION, new EffectConfig("Explosion", 0.6f, false, 50, 2f) },
        
        // 기타 효과
        { EffectType.LEVEL_UP, new EffectConfig("LevelUp", 0.4f, false, 90, 1.5f) },
        { EffectType.SUMMON, new EffectConfig("Summon", 0.5f, false, 60, 1.3f) },
        { EffectType.TELEPORT, new EffectConfig("Teleport", 0.6f, false, 40, 1f) },
        { EffectType.DEATH, new EffectConfig("Death", 0.4f, false, 60, 1f) }
    };
    
    // 오브젝트 풀
    private static Queue<SimpleEffect> effectPool = new Queue<SimpleEffect>();
    private static int maxPoolSize = 50;
    
    // 이펙트 생성 및 반환 (설정값 자동 적용)
    public static SimpleEffect PlayEffect(EffectType type)
    {
        if (!effectConfigs.TryGetValue(type, out EffectConfig config))
        {
            Debug.LogWarning($"[EffectFactory] Unknown effect type: {type}");
            return null;
        }
        
        SimpleEffect effect = GetEffectFromPool();
        effect.transform.localScale = Vector3.one * config.scale;
        effect.SetConfig(config.textureName, config.speed, config.loop, config.duration);
        return effect;
    }
    
    // 이펙트 생성 (위치 지정 버전)
    public static SimpleEffect PlayEffect(EffectType type, Vector3 position)
    {
        SimpleEffect effect = PlayEffect(type);
        if (effect != null)
        {
            effect.transform.position = position;
        }
        return effect;
    }
    
    // 이펙트 생성 (타겟 지정 버전)
    public static SimpleEffect PlayEffect(EffectType type, Transform target)
    {
        SimpleEffect effect = PlayEffect(type);
        if (effect != null && target != null)
        {
            effect.transform.position = target.position;
        }
        return effect;
    }
    
    // 풀에서 이펙트 가져오기
    private static SimpleEffect GetEffectFromPool()
    {
        SimpleEffect effect;
        
        if (effectPool.Count > 0)
        {
            effect = effectPool.Dequeue();
            effect.gameObject.SetActive(true);
        }
        else
        {
            GameObject effectObj = new GameObject("Effect");
            effect = effectObj.AddComponent<SimpleEffect>();
            effectObj.transform.SetParent(EffectLayer.Instance.transform);
        }
        
        return effect;
    }
    
    // 이펙트 반환 (모든 이펙트를 풀로 재사용)
    public static void ReturnEffect(SimpleEffect effect)
    {
        if (effect == null) return;
        
        // 풀로 반환 (SimpleEffect.Remove()에서 이미 초기화함)
        if (effectPool.Count < maxPoolSize)
        {
            effectPool.Enqueue(effect);
        }
        else
        {
            GameObject.Destroy(effect.gameObject);
        }
    }
    
    // 이펙트 설정 구조체
    private struct EffectConfig
    {
        public string textureName;
        public float speed;
        public bool loop;
        public int duration;
        public float scale;
        
        public EffectConfig(string name, float spd, bool lp, int dur, float scl)
        {
            textureName = name;
            speed = spd;
            loop = lp;
            duration = dur;
            scale = scl;
        }
    }
}