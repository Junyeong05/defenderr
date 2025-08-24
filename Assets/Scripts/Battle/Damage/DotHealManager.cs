using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 도트 힐 관리 클래스
/// TypeScript의 DotHealVO를 Unity로 포팅
/// </summary>
[System.Serializable]
public class DotHealManager
{
    [SerializeField]
    private List<BaseDotHealVO> list = new List<BaseDotHealVO>();
    
    public List<BaseDotHealVO> List => list;
    
    /// <summary>
    /// 도트 힐 추가
    /// </summary>
    public void AddHeal(float heal, int duration, int interval, BaseHero owner)
    {
        BaseDotHealVO vo = BaseDotHealVO.GetVO();
        
        vo.heal = heal;
        vo.duration = duration - 1;  // 첫 프레임 보정
        vo.interval = interval;
        vo.owner = owner;
        
        list.Add(vo);
    }
    
    /// <summary>
    /// 시간 진행 (매 프레임 호출)
    /// </summary>
    public void AdvanceTime()
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            BaseDotHealVO vo = list[i];
            vo.duration--;
            
            if (vo.duration < 0)
            {
                list.RemoveAt(i);
                vo.Remove();
            }
        }
    }
    
    /// <summary>
    /// 모든 도트 힐 제거
    /// </summary>
    public void Reset()
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            list[i].Remove();
        }
        list.Clear();
    }
}