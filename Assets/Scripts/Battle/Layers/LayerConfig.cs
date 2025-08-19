using UnityEngine;

// 레이어별 Sorting Order 설정
public static class LayerConfig
{
    // 각 레이어의 기본 Sorting Order
    public const int BACKGROUND = 0;
    public const int GROUND_EFFECT = 100;
    public const int UNIT = 200;
    public const int WEAPON = 300;
    public const int EFFECT = 400;
    public const int UI = 500;
    public const int POPUP = 600;
    
    // 영웅에 Sorting Order 설정
    public static void SetUnitSortingOrder(GameObject unit, int offset = 0)
    {
        SpriteRenderer sr = unit.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Y 위치 기반으로 자동 정렬 (아래쪽이 앞에)
            float y = unit.transform.position.y;
            sr.sortingOrder = UNIT - Mathf.RoundToInt(y * 10) + offset;
        }
    }
    
    // 투사체에 Sorting Order 설정
    public static void SetProjectileSortingOrder(GameObject projectile, int offset = 0)
    {
        SpriteRenderer sr = projectile.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = WEAPON + offset;
        }
    }
    
    // 효과에 Sorting Order 설정
    public static void SetEffectSortingOrder(GameObject effect, int offset = 0)
    {
        SpriteRenderer sr = effect.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = EFFECT + offset;
        }
    }
}