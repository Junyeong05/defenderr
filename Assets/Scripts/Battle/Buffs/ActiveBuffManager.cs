using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActiveBuffManager
{
    [SerializeField] private List<BaseActiveBuff> buffList = new List<BaseActiveBuff>();
    [SerializeField] private float initValue = 0f;
    [SerializeField] private float currentValue = 0f;
    [SerializeField] private float minValue = -1f;
    [SerializeField] private float maxValue = 10f;
    
    // 중복 판정용 Dictionary (재사용)
    private Dictionary<int, float> buffGroupMaxValues = new Dictionary<int, float>();
    private List<int> activeBuffIds = new List<int>();
    
    public float Value => currentValue;
    
    public ActiveBuffManager(float min = -1f, float max = 10f, float init = 0f)
    {
        minValue = min;
        maxValue = max;
        initValue = init;
        currentValue = init;
    }
    
    public void AddBuff(int id, float value, int durationFrames, bool overrideBuff = false)
    {
        BaseActiveBuff newBuff = new BaseActiveBuff(id, value, durationFrames);
        AddBuff(newBuff, overrideBuff);
    }
    
    public void AddBuff(BaseActiveBuff newBuff, bool overrideBuff = false)
    {
        bool canAdd = true;
        
        // 같은 ID의 버프가 이미 있는지 확인
        for (int i = buffList.Count - 1; i >= 0; i--)
        {
            BaseActiveBuff existingBuff = buffList[i];
            
            if (existingBuff.id == newBuff.id)
            {
                // 강제 덮어쓰기
                if (overrideBuff)
                {
                    existingBuff.value = newBuff.value;
                    existingBuff.count = newBuff.count;
                    canAdd = false;
                    break;
                }
                
                // 값이 같으면 더 긴 지속시간 적용
                if (Mathf.Approximately(existingBuff.value, newBuff.value))
                {
                    if (newBuff.count > existingBuff.count)
                    {
                        existingBuff.count = newBuff.count;
                    }
                    canAdd = false;
                    break;
                }
                
                // 양수 버프 (버프)
                if (newBuff.value > 0)
                {
                    // 더 강하고 더 긴 버프면 덮어쓰기
                    if (newBuff.value >= existingBuff.value && newBuff.count >= existingBuff.count)
                    {
                        existingBuff.value = newBuff.value;
                        existingBuff.count = newBuff.count;
                        canAdd = false;
                        break;
                    }
                    // 더 약하고 더 짧으면 무시
                    else if (newBuff.value <= existingBuff.value && newBuff.count <= existingBuff.count)
                    {
                        canAdd = false;
                        break;
                    }
                }
                // 음수 버프 (디버프)
                else if (newBuff.value < 0)
                {
                    // 더 강한 디버프(더 작은 값)이고 더 길면 덮어쓰기
                    if (newBuff.value <= existingBuff.value && newBuff.count >= existingBuff.count)
                    {
                        existingBuff.value = newBuff.value;
                        existingBuff.count = newBuff.count;
                        canAdd = false;
                        break;
                    }
                    // 더 약한 디버프이고 더 짧으면 무시
                    else if (newBuff.value >= existingBuff.value && newBuff.count <= existingBuff.count)
                    {
                        canAdd = false;
                        break;
                    }
                }
            }
        }
        
        if (canAdd)
        {
            buffList.Add(newBuff);
        }
        
        // 즉시 값 재계산
        AdvanceTime(0);
    }
    
    public void RemoveBuff(int id)
    {
        for (int i = buffList.Count - 1; i >= 0; i--)
        {
            if (buffList[i].id == id)
            {
                buffList.RemoveAt(i);
            }
        }
        
        AdvanceTime(0);
    }
    
    public bool HasBuff(int id)
    {
        foreach (var buff in buffList)
        {
            if (buff.id == id)
                return true;
        }
        return false;
    }
    
    public void RemoveAllPositiveBuffs()
    {
        for (int i = buffList.Count - 1; i >= 0; i--)
        {
            if (buffList[i].value > 0)
            {
                buffList.RemoveAt(i);
            }
        }
        
        AdvanceTime(0);
    }
    
    public void RemoveAllNegativeBuffs()
    {
        for (int i = buffList.Count - 1; i >= 0; i--)
        {
            if (buffList[i].value < 0)
            {
                buffList.RemoveAt(i);
            }
        }
        
        AdvanceTime(0);
    }
    
    public void AdvanceTime(int deltaFrames = 1)
    {
        currentValue = initValue;
        
        // Dictionary 초기화
        buffGroupMaxValues.Clear();
        activeBuffIds.Clear();
        
        // 버프 처리 및 시간 감소
        for (int i = buffList.Count - 1; i >= 0; i--)
        {
            BaseActiveBuff buff = buffList[i];
            buff.count -= deltaFrames;
            
            if (buff.count >= 0)
            {
                // 같은 ID 그룹의 최대값(버프) 또는 최소값(디버프) 적용
                if (!buffGroupMaxValues.ContainsKey(buff.id))
                {
                    activeBuffIds.Add(buff.id);
                    buffGroupMaxValues[buff.id] = buff.value;
                    currentValue += buff.value;
                }
                else
                {
                    float currentGroupValue = buffGroupMaxValues[buff.id];
                    
                    // 양수 버프는 최대값
                    if (buff.value > 0)
                    {
                        if (buff.value > currentGroupValue)
                        {
                            currentValue += (buff.value - currentGroupValue);
                            buffGroupMaxValues[buff.id] = buff.value;
                        }
                    }
                    // 음수 디버프는 최소값
                    else
                    {
                        if (buff.value < currentGroupValue)
                        {
                            currentValue += (buff.value - currentGroupValue);
                            buffGroupMaxValues[buff.id] = buff.value;
                        }
                    }
                }
            }
            else
            {
                // 시간이 끝난 버프 제거
                buffList.RemoveAt(i);
            }
        }
        
        // 최대/최소값 제한
        currentValue = Mathf.Clamp(currentValue, minValue, maxValue);
    }
    
    public void Reset()
    {
        buffList.Clear();
        currentValue = initValue;
        buffGroupMaxValues.Clear();
        activeBuffIds.Clear();
    }
    
    // 디버깅용
    public int GetActiveBuffCount() => buffList.Count;
    public List<BaseActiveBuff> GetActiveBuffs() => new List<BaseActiveBuff>(buffList);
}