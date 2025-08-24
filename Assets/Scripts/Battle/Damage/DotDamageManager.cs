using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 도트 데미지 관리 클래스
/// TypeScript의 DotDamageVO를 Unity로 포팅
/// </summary>
[System.Serializable]
public class DotDamageManager
{
    [SerializeField]
    private List<BaseDotDamageVO> list = new List<BaseDotDamageVO>();
    
    public List<BaseDotDamageVO> List => list;
    
    /// <summary>
    /// 도트 데미지 추가 또는 갱신
    /// </summary>
    public void AddDamage(float damage, int duration, int interval, BaseHero owner, int id)
    {
        BaseDotDamageVO vo = GetDot(owner, id);
        if (vo == null)
        {
            vo = BaseDotDamageVO.GetVO();
            list.Add(vo);
        }
        
        vo.damage = damage;
        vo.duration = duration - 1;  // 첫 프레임 보정
        vo.interval = interval;
        vo.owner = owner;
        vo.id = id;
    }
    
    /// <summary>
    /// 특정 owner와 id를 가진 도트 데미지 찾기
    /// </summary>
    public BaseDotDamageVO GetDot(BaseHero owner, int id)
    {
        if (id == -1) return null;
        
        for (int i = list.Count - 1; i >= 0; i--)
        {
            BaseDotDamageVO vo = list[i];
            if (vo.id == id && vo.owner == owner)
            {
                return vo;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 특정 ID의 도트 데미지 제거
    /// </summary>
    public void RemoveDot(int id)
    {
        if (id < 1) return;
        
        for (int i = list.Count - 1; i >= 0; i--)
        {
            BaseDotDamageVO vo = list[i];
            if (vo.id == id)
            {
                list.RemoveAt(i);
                vo.Remove();
                return;
            }
        }
    }
    
    /// <summary>
    /// 시간 진행 (매 프레임 호출)
    /// </summary>
    public void AdvanceTime()
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            BaseDotDamageVO vo = list[i];
            vo.duration--;
            
            if (vo.duration < 0)
            {
                list.RemoveAt(i);
                vo.Remove();
            }
        }
    }
    
    /// <summary>
    /// 모든 도트 데미지 제거
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