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
    
    // Rotation offset for sprite orientation
    protected float rotationOffset = 0f; // 0 = right-facing sprite
    
    // Projectile physics (TypeScript style)
    protected float T; // Total frames to reach target
    protected float t; // Current frame
    protected float vx, vy; // Velocity components
    protected float g; // Gravity
    
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
        
        // 화살류 무기의 경우 회전 오프셋 설정
        // 텍스처가 오른쪽을 바라보는 경우 기본값 0
        // 필요 시 다른 각도로 조정 (예: 위쪽 = -90, 왼쪽 = 180, 아래 = 90)
        if (className.Contains("Arrow"))
        {
            rotationOffset = 0f; // 오른쪽 방향 텍스처
        }
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
        
        // 이미 WeaponLayer에 있으므로 parent 변경 불필요
        // Unity는 비활성화된 오브젝트도 parent 유지
        
        // 데미지 계산
        if (owner != null)
        {
            damage = owner.Data.GetAttackPower(owner.Level) * damagePercent * weaponData.damageMultiplier;
        }
        
        // 상태 초기화
        ResetWeapon();
        
        // 위치 설정 - BaseHero에서 이미 설정하지 않았으면 owner 위치 사용
        if (transform.position == Vector3.zero && owner != null)
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
    
    protected virtual void ResetWeapon()
    {
        currentFrame = 0;
        frameCounter = 0f;
        hasHit = false;
        lifetimeFrames = 0;
        
        // weaponData null 체크 추가 (Unity Editor에서 Reset 호출 시 null일 수 있음)
        if (weaponData != null)
        {
            delayFrames = weaponData.delayFrames;
            remainingPenetration = weaponData.penetration;
        }
        else
        {
            delayFrames = 0;
            remainingPenetration = 0;
        }
        
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
        
        // 영웅의 타격 영역 크기 기반 타격 지점 계산 (픽셀 좌표 사용)
        float targetHeight = target.TargetHeight;
        float targetWidth = target.TargetWidth;
        
        // 발끝(0,0)으로부터 높이의 절반 지점을 중심으로
        // 높이: targetHeight/2 지점을 중심으로 -0.3*targetHeight ~ +0.3*targetHeight
        float centerY = targetHeight * 0.5f;
        float rangeY = targetHeight * 0.3f;
        targetOffset.y = centerY + UnityEngine.Random.Range(-rangeY, rangeY);
        
        // 너비: -0.3*targetWidth ~ +0.3*targetWidth
        float rangeX = targetWidth * 0.3f;
        targetOffset.x = UnityEngine.Random.Range(-rangeX, rangeX);
        
        // Calculate distance and height to target
        float h = target.transform.position.y + targetOffset.y - transform.position.y;
        float d = target.transform.position.x + targetOffset.x - transform.position.x;
        
        // Ensure minimum distance
        if (Mathf.Abs(d) < 20f) d = d >= 0 ? 20f : -20f;
        
        // Calculate flight time based on distance
        T = Mathf.Min(30f, Mathf.Max(weaponData.minFlightTime, Mathf.Abs(d) / weaponData.initialSpeed));
        
        // Calculate gravity based on flight time
        float ratio = 0.047f * (T - 1f);
        g = weaponData.baseGravity * (0.8f + 0.2f * UnityEngine.Random.value) * ratio;
        
        // Calculate initial velocities
        vx = d / T;
        vy = h / T - g * (T - 1f) * 0.5f;
        
        // Reset frame counter
        t = 0;
        
        // Set initial rotation
        if (weaponData.rotateToDirection)
        {
            float angle = Mathf.Atan2(vy, vx) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
        }
    }
    
    protected virtual void UpdateProjectile()
    {
        // TypeScript style projectile physics
        
        // Adjust trajectory to track moving target
        if (t < T && target != null && target.IsAlive)
        {
            float d = target.transform.position.x + targetOffset.x - transform.position.x;
            vx = d / (T - t);
        }
        
        t++;
        
        // Update position
        Vector3 oldPos = transform.position;
        transform.position += new Vector3(vx, vy, 0);
        vy += g; // Apply gravity
        
        // Update rotation to follow trajectory
        if (weaponData.rotateToDirection)
        {
            float angle = Mathf.Atan2(vy, vx) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
        }
        
        // Check if reached target time
        if (t >= T)
        {
            // Force hit at target frame
            if (target != null && target.IsAlive)
            {
                OnHitTarget(target);
            }
            else
            {
                Remove();
            }
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
        // TypeScript style: Only check collision with designated target at T frame
        // Collision is handled in UpdateProjectile when t >= T
        // This method is kept empty for compatibility
    }
    
    protected void OnHitTarget(BaseHero hitTarget)
    {
        if (hitTarget == null || !hitTarget.IsAlive) return;
        
        // 데미지 적용
        hitTarget.TakeDamage(damage);
        
        // 타격 이펙트 표시
        // if (weaponData.showHitEffect)
        // {
        //     ShowHitEffect(hitTarget.transform.position);
        // }
        ShowHitEffect(hitTarget, EffectType.PHYSICAL_HIT);
        
        OnHitTargetSub(hitTarget);

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

    protected virtual void OnHitTargetSub(BaseHero targetHero)
    {
    }
    
    protected void ShowHitEffect(BaseHero targetHero, EffectType effectType)
    {
        SimpleEffect hitEffect = EffectFactory.PlayEffect(effectType);
        if (hitEffect != null)
        {
            // 무기(화살)의 월드 좌표를 타겟 영웅의 로컬 좌표로 변환
            Vector3 worldPos = this.transform.position;
            Vector3 localPos = targetHero.transform.InverseTransformPoint(worldPos);
            
            // 타겟 영웅의 자식으로 설정
            hitEffect.SetParent(targetHero.transform);
            
            // 변환된 로컬 좌표로 설정 (화살이 맞은 정확한 위치에 이펙트 표시)
            hitEffect.x = localPos.x;
            hitEffect.y = localPos.y;
            hitEffect.Play();
        }
    }

    // protected virtual void ShowHitEffect(Vector3 position)
    // {
    //     // 카탈로그에서 이펙트 프리팹 가져오기
    //     if (!string.IsNullOrEmpty(weaponData.hitEffectName))
    //     {
    //         // WeaponFactory를 통해 이펙트 프리팹 가져오기
    //         if (factory != null)
    //         {
    //             GameObject effectPrefab = factory.GetHitEffectPrefab(weaponClass);
    //             if (effectPrefab != null)
    //             {
    //                 GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity);
    //                 // EffectLayer에 추가
    //                 effect.transform.SetParent(EffectLayer.Instance.transform);
    //                 Destroy(effect, weaponData.hitEffectDuration);
    //             }
    //         }
    //     }
    // }
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
        ResetWeapon();
        owner = null;
        target = null;
        damage = 0;
        
        // WeaponLayer에 그대로 둠 (PixiJS의 removeChild 대신 비활성화)
        // Unity는 SetActive(false)로 화면에서 사라짐
        
        // 위치 초기화
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        
        // 비활성화 (화면에서 사라짐)
        gameObject.SetActive(false);
    }
    #endregion
    
    #region Properties
    public bool IsActive => isActive;
    public string WeaponClass => weaponClass;
    public WeaponData Data => weaponData;
    #endregion
}