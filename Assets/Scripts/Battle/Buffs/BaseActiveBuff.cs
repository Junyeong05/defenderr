using System;
using UnityEngine;

[Serializable]
public class BaseActiveBuff
{
    public int id;          // 버프 타입 ID (같은 ID = 같은 타입)
    public float value;     // 버프 효과값 (양수: 버프, 음수: 디버프)
    public int count;       // 남은 프레임 수 (60fps 기준)
    
    public BaseActiveBuff(int id, float value, int count)
    {
        this.id = id;
        this.value = value;
        this.count = count;
    }
    
    public BaseActiveBuff Clone()
    {
        return new BaseActiveBuff(id, value, count);
    }
}