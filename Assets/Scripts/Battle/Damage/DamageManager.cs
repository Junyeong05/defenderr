using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 중앙화된 데미지 계산 시스템
/// TypeScript의 DamageManager를 Unity로 포팅
/// </summary>
public static class DamageManager
{
        // 속성 상성 테이블
        // 1: 철, 2: 화, 3: 수, 4: 목
        // Key: "공격자속성+피격자속성" (예: "23" = 화(2)가 수(3)를 공격)
        private static readonly Dictionary<string, float> propertyTable = new Dictionary<string, float>
        {
            // 철 속성 공격
            {"11", 1.0f}, {"12", 0.8f}, {"13", 1.0f}, {"14", 1.2f},
            // 화 속성 공격
            {"21", 1.2f}, {"22", 1.0f}, {"23", 0.8f}, {"24", 1.0f},
            // 수 속성 공격
            {"31", 1.0f}, {"32", 1.2f}, {"33", 1.0f}, {"34", 0.8f},
            // 목 속성 공격
            {"41", 0.8f}, {"42", 1.0f}, {"43", 1.2f}, {"44", 1.0f}
        };

        /// <summary>
        /// 일반 데미지 계산
        /// </summary>
        public static DamageVO GetDamage(float damage, BaseHero from, BaseHero to, DamageBuffVO buffVO = null)
        {
            DamageVO vo = DamageVO.GetVO();
            
            if (from == null || to == null) return vo;
            
            if (buffVO == null)
            {
                buffVO = DamageBuffVO.GetVO();
            }
            
            float dmg = damage;
            float critChance = from.CritChance + buffVO.critChanceUp;
            float critMultiplier = from.CritMultiplier + buffVO.critMultiplierUp;
            bool isCritical = false;
            
            // 0. 치명타 여부 결정
            if (Random.Range(0f, 1f) < critChance)
            {
                isCritical = true;
            }
            
            // 1. 방어력 및 관통 적용
            float pene = from.Penetrate + buffVO.penetrateUp;
            if (pene > 1f) pene = 1f;
            
            // 1-1. 치명타 시 방어 무시 처리 (화석 버프 등)
            if (isCritical && from.IgnoreDefensePercentageOnCritically > 0)
            {
                if (Random.Range(0f, 1f) < from.IgnoreDefensePercentageOnCritically * 0.01f)
                {
                    pene = 1f;
                }
            }
            
            dmg = dmg - to.Defense * (1f - pene);
            
            // 2. 속성 상성 처리
            if (to.Property != 0 && from.Property != 0)
            {
                string key = from.Property.ToString() + to.Property.ToString();
                if (propertyTable.ContainsKey(key))
                {
                    dmg *= propertyTable[key];
                }
            }
            
            // 3. 치명타 처리
            if (isCritical)
            {
                dmg *= critMultiplier;
                vo.isCritical = true;
            }
            
            // 4. 데미지 감소 처리
            if (to.DamageReduction != 0)
            {
                dmg = Mathf.Max(0, dmg / to.DamageReduction);
            }
            
            // 4-1. 스킬 차단 횟수가 있을 경우 추가 감소 (신록의 장막 등)
            if (to.SkillInterruptionCnt > 0)
            {
                dmg = dmg * 0.8f;
            }
            
            // 최소 데미지는 1
            if (dmg <= 0) dmg = 1;
            
            // 5. 최종 데미지 증폭 적용
            dmg = dmg * (1f + from.FinalDamageMultiplier);
            
            // 6-1. 지속시간이 있는 보호막 처리
            float shieldWithDuration = 0;
            if (buffVO.damageUpForShield > 0)
            {
                shieldWithDuration = to.GetShieldWithDuration() - dmg * (1f + buffVO.damageUpForShield);
            }
            else
            {
                shieldWithDuration = to.GetShieldWithDuration() - dmg;
            }
            
            if (shieldWithDuration > 0)
            {
                dmg = 0;
                vo.shieldWithDuration = shieldWithDuration;
            }
            else
            {
                if (buffVO.damageUpForShield > 0)
                {
                    dmg = -shieldWithDuration / (1f + buffVO.damageUpForShield);
                }
                else
                {
                    dmg = -shieldWithDuration;
                }
                vo.shieldWithDuration = 0;
                if (dmg < 0) dmg = 0;
            }
            
            // 6-2. 지속시간이 없는 보호막 처리
            float shield = 0;
            if (buffVO.damageUpForShield > 0)
            {
                shield = to.GetShield() - dmg * (1f + buffVO.damageUpForShield);
            }
            else
            {
                shield = to.GetShield() - dmg;
            }
            
            if (shield > 0)
            {
                dmg = 0;
                vo.shield = shield;
            }
            else
            {
                if (buffVO.damageUpForShield > 0)
                {
                    dmg = -shield / (1f + buffVO.damageUpForShield);
                }
                else
                {
                    dmg = -shield;
                }
                vo.shield = 0;
                if (dmg < 0) dmg = 0;
            }
            
            vo.damage = dmg;
            
            return vo;
        }
        
        /// <summary>
        /// 도트 데미지 계산 (방어력 무시, 보호막만 적용)
        /// </summary>
        public static DamageVO GetDotDamage(float damage, BaseHero from, BaseHero to)
        {
            DamageVO vo = DamageVO.GetVO();
            
            if (from == null || to == null) return vo;
            
            float dmg = damage;
            
            // 1-1. 지속시간이 있는 보호막 처리
            float shieldWithDuration = to.GetShieldWithDuration() - dmg;
            if (shieldWithDuration > 0)
            {
                dmg = 0;
                vo.shieldWithDuration = shieldWithDuration;
            }
            else
            {
                dmg = -shieldWithDuration;
                vo.shieldWithDuration = 0;
                if (dmg < 0) dmg = 0;
            }
            
            // 1-2. 지속시간이 없는 보호막 처리
            float shield = to.GetShield() - dmg;
            if (shield > 0)
            {
                dmg = 0;
                vo.shield = shield;
            }
            else
            {
                dmg = -shield;
                vo.shield = 0;
                if (dmg < 0) dmg = 0;
            }
            
            vo.damage = dmg;
            
            return vo;
        }
    }