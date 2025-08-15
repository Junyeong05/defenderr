using UnityEngine;
using System;

// 기본 무기 클래스
public class BaseWeapon : MonoBehaviour
{
    #region Fields
    [Header("Weapon Configuration")]
    protected WeaponData weaponData;
    protected string weaponClass;
    
    [Header("Runtime State")]
    protected BaseHero owner;
    protected BaseHero target;
    protected float damage;
    
    // Movement
    protected Vector2 velocity;
    protected float shootAngle;
    protected Vector2 targetOffset;
    
    // Animation
    protected SpriteRenderer spriteRenderer;
    protected Sprite[] sprites;
    protected int currentFrame = 0;
    protected float frameCounter = 0f;
    
    // State
    protected bool isActive = false;
    protected bool hasHit = false;
    protected int delayFrames = 0;
    protected int lifetimeFrames = 0;
    protected int maxLifetimeFrames = 300; // 5초 (60fps)
    
    // Factory reference
    private WeaponFactory factory;
    
    // Penetration
    protected int remainingPenetration = 0;
    #endregion
    
    #region Unity Lifecycle
    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }
    
    protected virtual void OnEnable()
    {
        // FrameController에 등록
        if (FrameController.Instance != null)
        {
            FrameController.Add(Execute, this);
        }
    }
    
    protected virtual void OnDisable()
    {
        // FrameController에서 제거
        if (FrameController.Instance != null)
        {
            FrameController.Remove(Execute, this);
        }
    }
    #endregion
    
    #region Initialization
    public virtual void Initialize(WeaponData data, string className)
    {
        weaponData = data;
        weaponClass = className;
        
        // 스프라이트 로드
        LoadSprites();
        
        // 최대 생존 프레임 설정 (60fps 기준)
        maxLifetimeFrames = Mathf.RoundToInt(weaponData.lifetime * 60f);
    }
    
    protected virtual void LoadSprites()
    {
        if (!string.IsNullOrEmpty(weaponData.textureName))
        {
            // TextureManager 사용
            sprites = TextureManager.GetSprites("atlases/Battle", weaponData.textureName);
        }
        
        if (sprites != null && sprites.Length > 0)
        {
            spriteRenderer.sprite = sprites[0];
        }
    }
    
    public void SetFactory(WeaponFactory factory)
    {
        this.factory = factory;
    }
    #endregion
    
    #region Setup
    public virtual void SetupWeapon(BaseHero owner, BaseHero target, float damagePercent = 1f)
    {
        this.owner = owner;
        this.target = target;
        
        // 데미지 계산
        if (owner != null)
        {
            damage = owner.Data.GetAttackPower(owner.Level) * damagePercent * weaponData.damageMultiplier;
        }
        
        // 상태 초기화
        Reset();
        
        // 위치 설정
        if (owner != null)
        {
            transform.position = owner.transform.position;
        }
        
        // 타입별 초기화
        switch (weaponData.weaponType)
        {
            case WeaponType.Projectile:
                InitProjectile();
                break;
            case WeaponType.Beam:
                InitBeam();
                break;
            case WeaponType.Custom:
                InitCustom();
                break;
        }
        
        isActive = true;
    }
    
    protected virtual void Reset()
    {
        currentFrame = 0;
        frameCounter = 0f;
        hasHit = false;
        lifetimeFrames = 0;
        delayFrames = weaponData.delayFrames;
        remainingPenetration = weaponData.penetration;
        velocity = Vector2.zero;
        targetOffset = Vector2.zero;
    }
    #endregion
    
    #region Frame Update
    public virtual void Execute()
    {
        if (!isActive) return;
        
        // 지연 처리
        if (delayFrames > 0)
        {
            delayFrames--;
            gameObject.SetActive(false);
            return;
        }
        else
        {
            gameObject.SetActive(true);
        }
        
        // 생존 시간 체크
        lifetimeFrames++;
        if (lifetimeFrames >= maxLifetimeFrames)
        {
            Remove();
            return;
        }
        
        // 타입별 업데이트
        switch (weaponData.weaponType)
        {
            case WeaponType.Projectile:
                UpdateProjectile();
                break;
            case WeaponType.Beam:
                UpdateBeam();
                break;
            case WeaponType.Custom:
                UpdateCustom();
                break;
        }
        
        // 애니메이션 업데이트
        UpdateAnimation();
        
        // 충돌 체크
        CheckCollision();
    }
    
    protected virtual void UpdateAnimation()
    {
        if (sprites == null || sprites.Length == 0) return;
        
        frameCounter += weaponData.animationSpeed;
        
        while (frameCounter >= 1f)
        {
            frameCounter -= 1f;
            currentFrame++;
            
            if (currentFrame >= sprites.Length)
            {
                currentFrame = 0;
            }
        }
        
        spriteRenderer.sprite = sprites[currentFrame];
    }
    #endregion
    
    #region Projectile Type
    protected virtual void InitProjectile()
    {
        if (target == null) return;
        
        // 타겟 오프셋 (약간의 랜덤성)
        targetOffset.x = UnityEngine.Random.Range(-20f, 20f);
        targetOffset.y = UnityEngine.Random.Range(-20f, 20f);
        
        // 타겟 방향 계산
        Vector2 targetPos = (Vector2)target.transform.position + targetOffset;
        Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
        
        // 발사 각도
        shootAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // 회전 설정
        if (weaponData.rotateToDirection)
        {
            transform.rotation = Quaternion.Euler(0, 0, shootAngle);
        }
        
        // 속도 설정
        velocity = direction * weaponData.initialSpeed;
    }
    
    protected virtual void UpdateProjectile()
    {
        // 이동
        transform.position += (Vector3)velocity * Time.fixedDeltaTime * 60f; // 60fps 기준으로 조정
        
        // 가속도 적용
        if (weaponData.acceleration != 0)
        {
            velocity *= (1f + weaponData.acceleration * Time.fixedDeltaTime);
        }
        
        // 회전
        if (weaponData.rotationSpeed != 0)
        {
            transform.Rotate(0, 0, weaponData.rotationSpeed * Time.fixedDeltaTime * 60f);
        }
    }
    #endregion
    
    #region Beam Type
    protected virtual void InitBeam()
    {
        // 빔 타입 초기화
        // 즉시 타겟에 라인 렌더러나 스프라이트로 연결
    }
    
    protected virtual void UpdateBeam()
    {
        // 빔 업데이트
        // 타겟 추적하며 빔 유지
    }
    #endregion
    
    #region Custom Type
    protected virtual void InitCustom()
    {
        // 커스텀 타입 초기화
        // 서브클래스에서 구현
    }
    
    protected virtual void UpdateCustom()
    {
        // 커스텀 업데이트
        // 서브클래스에서 구현
    }
    #endregion
    
    #region Collision
    protected virtual void CheckCollision()
    {
        if (hasHit && weaponData.destroyOnHit) return;
        if (target == null || !target.IsAlive) return;
        
        // 타겟과의 거리 체크
        float distance = Vector2.Distance(transform.position, target.transform.position);
        
        if (distance <= weaponData.hitRadius)
        {
            OnHitTarget(target);
        }
    }
    
    protected virtual void OnHitTarget(BaseHero hitTarget)
    {
        if (hitTarget == null || !hitTarget.IsAlive) return;
        
        // 데미지 적용
        hitTarget.TakeDamage(damage);
        
        // 타격 이펙트 표시
        if (weaponData.showHitEffect)
        {
            ShowHitEffect(hitTarget.transform.position);
        }
        
        // 관통 처리
        if (remainingPenetration > 0)
        {
            remainingPenetration--;
            // 다음 타겟 찾기 (옵션)
        }
        else if (weaponData.destroyOnHit)
        {
            hasHit = true;
            Remove();
        }
    }
    
    protected virtual void ShowHitEffect(Vector3 position)
    {
        // 카탈로그에서 이펙트 프리팹 가져오기
        if (!string.IsNullOrEmpty(weaponData.hitEffectName))
        {
            // WeaponFactory를 통해 이펙트 프리팹 가져오기
            if (factory != null)
            {
                GameObject effectPrefab = factory.GetHitEffectPrefab(weaponClass);
                if (effectPrefab != null)
                {
                    GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity);
                    Destroy(effect, weaponData.hitEffectDuration);
                }
            }
        }
    }
    #endregion
    
    #region Cleanup
    public virtual void Remove()
    {
        isActive = false;
        
        // Factory로 반환
        if (factory != null)
        {
            factory.ReturnWeapon(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    public virtual void ReturnToPool()
    {
        // 상태 초기화
        Reset();
        owner = null;
        target = null;
        damage = 0;
        
        // 위치 초기화
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        
        // 비활성화
        gameObject.SetActive(false);
    }
    #endregion
    
    #region Properties
    public bool IsActive => isActive;
    public string WeaponClass => weaponClass;
    public WeaponData Data => weaponData;
    #endregion
}