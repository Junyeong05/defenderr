using UnityEngine;

// 무기 데이터 ScriptableObject (엑셀 데이터 매핑용)
[CreateAssetMenu(fileName = "WeaponData", menuName = "Battle/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Basic Info")]
    public string weaponName = "Arrow";
    public string weaponClass = "Arrow";  // 클래스 이름 (카탈로그에서 프리팹 매칭용)
    public WeaponType weaponType = WeaponType.Projectile;
    
    [Header("Visual")]
    // 프리팹 참조 제거 - WeaponCatalog에서 관리
    public string textureName = "";  // 스프라이트 이름 (TextureManager 사용시)
    public int startFrame = 0;
    public int endFrame = 0;
    public float animationSpeed = 1f;
    
    [Header("Movement")]
    public float initialSpeed = 5f;  // 초기 속도
    public float acceleration = 0f;  // 가속도 (구글시트 호환용 보관)
    public float baseGravity = 0.5f;  // 기본 중력값 (0=직선, >0=포물선)
    public float minFlightTime = 10f;  // 최소 비행 시간 (프레임)
    public float rotationSpeed = 0f;  // 회전 속도
    public bool rotateToDirection = true;  // 이동 방향으로 회전
    public float lifetime = 5f;  // 생존 시간 (초)
    
    [Header("Combat")]
    public float damageMultiplier = 1f;  // 데미지 배율
    public float critChanceBonus = 0f;  // 크리티컬 확률 보너스
    public float critMultiplierBonus = 0f;  // 크리티컬 배율 보너스
    public int penetration = 0;  // 관통 횟수
    
    [Header("Hit Detection")]
    public float hitRadius = 10f;  // 충돌 판정 반경
    public bool destroyOnHit = true;  // 타격시 파괴 여부
    
    [Header("Effects")]
    public string hitEffectName = "";  // 타격 이펙트 이름
    // 이펙트 프리팹도 카탈로그에서 관리
    public bool showHitEffect = true;
    public float hitEffectDuration = 1f;
    
    [Header("Delay")]
    public int delayFrames = 0;  // 발사 지연 프레임
    
    // 데미지 계산
    public float CalculateDamage(float baseDamage)
    {
        return baseDamage * damageMultiplier;
    }
}