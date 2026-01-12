using UnityEngine;
using System.Collections.Generic;
using System;

// AS3.0/PixiJS 스타일의 프레임 기반 Hero 기본 클래스
public class BaseHero : MonoBehaviour
{
    #region Constants
    protected const float FRAME_RATE = 60f;  // 60 FPS
    
    // 상태 상수
    public const int STATE_WAIT = 0;
    public const int STATE_MOVE = 1;
    public const int STATE_ATTACK = 2;
    public const int STATE_SKILL = 3;
    public const int STATE_DIE = 4;
    #endregion

    #region Inspector Fields
    [Header("Hero Configuration")]
    protected HeroData heroData;
    protected int level = 1;
    protected bool isInitialized = false;
    protected bool hasData = false;
    
    [Header("Runtime Stats")]
    [SerializeField] protected float currentHealth;
    [SerializeField] protected float maxHealth;
    [SerializeField] protected float attackPower;
    [SerializeField] protected float defense;

    [Header("Team Settings")]
    [SerializeField] protected bool isPlayerTeam = true;  // true: 플레이어, false: 적
    
    [Header( "HealthBar" ) ]
    [SerializeField] GameObject healthBarPrefab;


    [Header("Debug Settings")]
    [SerializeField] protected bool showCrosshair = false;
    [SerializeField] protected Color crosshairColor = Color.green;
    [SerializeField] protected float crosshairSize = 20f;

    #endregion

    #region Private Fields
    // Components
    protected SpriteRenderer spriteRenderer;
    protected Transform spriteTransform;
    protected Vector2 footOffset = Vector2.zero;
    protected GameObject healthBar;
    
    // Shield System
    protected float shield = 0f;  // 영구 보호막
    protected float shieldWithDuration = 0f;  // 지속시간이 있는 보호막
    protected int shieldDurationFrames = 0;  // 보호막 남은 프레임
    
    protected Sprite[] spriteList;
    
    // Animation control
    public int currentFrame = 0;
    protected float frameCounter = 0f;
    protected float animationSpeed = 1f;
    
    // Animation state
    protected int animStartFrame;
    protected int animEndFrame;
    protected bool isLooping = true;
    protected int state = STATE_WAIT;
    
    // State flags
    protected bool isAlive = true;
    protected bool isMoving = false;
    protected bool isAttacking = false;
    protected bool health20Triggered = false;
    
    // Target
    protected Transform target;
    
    // Attack optimization
    protected Dictionary<int, bool> attackFrameDict = new Dictionary<int, bool>();
    protected int lastAttackFrame = -1;
    
    // Skill optimization  
    protected Dictionary<int, bool> skillFrameDict = new Dictionary<int, bool>();
    protected int lastSkillFrame = -1;
    protected bool hasSkilled = false;
    protected int attackIntervalFrames = 60;  // 1초 (60 FPS)
    protected int framesSinceLastAttack = 0;

    // HealthBar
    protected int framesSinceLastAttacked = 300;
    protected HealthBar healthBarScript;
    
    // Target & movement
    protected Vector2 targetPosition;
    protected Vector2 defaultTargetPosition;
    protected bool hasAttacked = false;
    protected bool hasAttackStarted = false;
    protected bool hasGameStarted = false;
    
    // Battle lists
    protected BaseHero[] friendList = null;
    protected BaseHero[] enemyList = null;
    protected bool isDying = false;
    protected bool isDeathAnimationComplete = false; // 죽는 애니메이션 완료 여부

    protected float orgSzie = 1.3f;
    
    // Hit reaction variables
    protected bool isHitReacting = true;
    protected int hitReactionFrame = 0;  // 현재 반응 프레임
    protected int hitReactionDurationFrames = 5; // 12프레임 동안 반응 (60fps 기준 0.2초)
    protected float hitReactionAngle = 23f; // 3도 회전
    protected Quaternion originalRotation;
    
    // Knockback variables  
    protected Vector2 knockbackDirection = Vector2.zero; // 넉백 방향
    protected float knockbackSpeed = 0f; // 프레임당 이동 속도
    protected float knockbackDistance = 0f; // 기존 근접 공격용 즉시 넉백 거리
    protected float knockbackResist = 0f; // 넉백 저항력 (0.0 ~ 1.0, 0.5 = 50% 감소)
    
    // Crowd Control (CC) variables
    protected int stunCount = 0;
    protected int freezeCount = 0;
    protected int sleepCount = 0;
    protected int knockbackCount = 0;
    protected int silenceCount = 0;
    protected int rootCount = 0;
    protected int tauntCount = 0;
    protected int blindCount = 0;
    protected int disarmCount = 0;
    protected int slowCount = 0;
    
    // CC immunity counters (프레임 기반)
    protected int immuneToStunCount = 0;
    protected int immuneToFreezeCount = 0;
    protected int immuneToKnockbackCount = 0;
    protected int immuneToSleepCount = 0;
    protected int immuneToSilenceCount = 0;
    protected int immuneToRootCount = 0;
    protected int immuneToTauntCount = 0;
    protected int immuneToBlindCount = 0;
    protected int immuneToDisarmCount = 0;
    protected int immuneToSlowCount = 0;
    
    private HeroFactory factory;
    protected string className;
    protected string sheetName = "atlases/Battle";
    private static Dictionary<string, TexturePackerFrameInfo> frameInfoCache = new Dictionary<string, TexturePackerFrameInfo>();
    
    // 이펙트 관리 (중복 방지)
    private Dictionary<EffectType, SimpleEffect> activeEffects = new Dictionary<EffectType, SimpleEffect>();
    
    // Active Buff System
    protected float orgAtkDmg;
    protected float orgDefense;
    protected float orgMaxHealth;
    protected float orgAtkDuration;
    protected float orgMoveSpd;
    protected float orgCritChance;
    protected float orgCritMultiplier;
    protected float orgDodgeChance;
    protected float orgDamageReduction;
    protected float orgPenetrate;
    protected float orgFinalDamageMultiplier = 1f;
    
    protected ActiveBuffManager activeDamageBuff;
    protected ActiveBuffManager activeDefenseBuff;
    protected ActiveBuffManager activeMaxHealthBuff;
    protected ActiveBuffManager activeAttackSpeedBuff;
    protected ActiveBuffManager activeMoveSpeedBuff;
    protected ActiveBuffManager activeCritChanceBuff;
    protected ActiveBuffManager activeCritMultiplierBuff;
    protected ActiveBuffManager activeDodgeChanceBuff;
    protected ActiveBuffManager activeDamageReductionBuff;
    protected ActiveBuffManager activePenetrateBuff;
    protected ActiveBuffManager activeFinalDamageBuff;
    
    // Buff System calculated stats
    protected float critChance;
    protected float critMultiplier;
    protected float damageReduction;
    protected float penetrate;
    protected float finalDamageMultiplier = 1f;
    protected float dodgeChance;
    
    // 누락된 필드들 추가
    protected int property = 0;  // 속성 (0: 없음, 1: 철, 2: 화, 3: 수, 4: 목)
    protected float ignoreDefensePercentageOnCritically = 0f;  // 치명타 시 방어 무시 확률
    protected int skillInterruptionCnt = 0;  // 스킬 차단 횟수 (신록의 장막 등)
    
    // DOT 시스템
    protected DotDamageManager dotDamager = new DotDamageManager();
    protected DotHealManager dotHeal = new DotHealManager();
    #endregion
    
    #region Public Properties
    // DamageManager와 외부 클래스에서 접근 가능한 프로퍼티들
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float AttackPower => attackPower;
    public float Defense => defense;
    public int AttackIntervalFrames => attackIntervalFrames;
    
    // 버프 시스템 스탯
    public float CritChance => critChance;
    public float CritMultiplier => critMultiplier;
    public float DamageReduction => damageReduction;
    public float Penetrate => penetrate;
    public float FinalDamageMultiplier => finalDamageMultiplier;
    public float DodgeChance => dodgeChance;
    
    // 속성 시스템
    public int Property => property;
    public float IgnoreDefensePercentageOnCritically => ignoreDefensePercentageOnCritically;
    public int SkillInterruptionCnt => skillInterruptionCnt;
    
    // 통계 시스템용 프로퍼티
    public int KindNum => heroData != null ? heroData.kindNum : 0;
    public string HeroClassName => heroData != null ? heroData.heroClass : className;
    public bool IsAlly => gameObject.tag == "Hero";  // Hero 태그가 아군, Enemy 태그가 적군
    
    // 애니메이션 관련
    public float AnimationSpeed 
    { 
        get => animationSpeed; 
        set => animationSpeed = value; 
    }
    public int CurrentFrame => currentFrame;
    public int State => state;
    public float FrameCounter => frameCounter;
    public int AnimEndFrame => animEndFrame;
    #endregion

    #region Unity Lifecycle
    protected virtual void Awake()
    {
        // Sprite setup with foot offset support
        SetupSpriteWithFootOffset();
        
        // AS3.0/PixiJS style - 클래스 이름 저장
        className = GetType().Name;
        
        // 기본 초기화는 SetData에서 수행
    }

    protected virtual void OnEnable()
    {
        // SetData에서 등록하므로 여기서는 등록하지 않음
        // 중복 등록 방지
    }

    protected virtual void OnDisable()
    {
        // BattleController에서 관리
    }
    #endregion

    #region Initialization Methods
    public virtual void Initialize()
    {
        if (isInitialized) return;
        
        // 기본 상태 초기화
        state = STATE_WAIT;
        isAlive = true;
        isMoving = false;
        isAttacking = false;
        
        // 텍스처 이름 초기화 (AS3.0/PixiJS 스타일)
        InitializeTextureName();
        
        // Hero specific initialization (서브클래스에서 구현)
        OnInitialize();
        
        // Buff System 초기화
        InitializeBuffSystem();
        
        isInitialized = true;

        SetSize(1f);
    }
    
    public virtual void SetData(HeroData data, int heroLevel)
    {
        if (!isInitialized)
        {
            Initialize();
        }
        
        // 이전 상태 정리
        ResetState();
        
        // 새로운 데이터 설정
        heroData = data;
        level = heroLevel;
        hasData = true;
        
        // UnitLayer에 추가 (PixiJS의 addChild처럼)
        transform.SetParent(UnitLayer.Instance.transform);
        
        if (heroData != null)
        {
            // 레벨에 따른 스탯 초기화
            InitializeStats();
            
            // Load sprites
            LoadSprites();
            
            // Attack frame dictionary 초기화
            InitializeAttackFrames();
            
            // 초기 애니메이션 설정 - 강제로 상태를 다른 값으로 변경 후 다시 설정하여 업데이트 보장
            state = -1; // 임시로 invalid state 설정
            SetState(STATE_WAIT);

            // Sprite 정보 로드 후 HealthBar 
            ShowHealthBar( healthBarPrefab );

            
            // BattleController에서 Execute() 호출하도록 변경
        }
        else
        {
            // SetData called with null HeroData!
        }
    }
    
    public void SetFactory(HeroFactory factory)
    {
        this.factory = factory;
    }
    
    public void Remove()
    {
        // 상태 초기화
        ReturnToPool();
        
        // 팩토리로 반환
        if (factory != null)
        {
            factory.ReturnHero(this);
        }
        else
        {
            // 팩토리가 없으면 그냥 비활성화
            gameObject.SetActive(false);
        }
    }
    
    public virtual void ReturnToPool()
    {
        // 타입 기반 통계에서 등록 해제 (풀 반환)
        if (BattleStatisticsManager.Instance != null)
        {
            BattleStatisticsManager.Instance.UnregisterHero(this);
        }
        
        ResetState();
        hasData = false;
        
        // 레이어에서 제거 (PixiJS의 removeChild처럼)
        transform.SetParent(null);
        
        // BattleController에서 관리
    }
    
    protected virtual void ResetState()
    {
        // 모든 Invoke 취소
        CancelInvoke();
        
        // 상태 초기화
        state = STATE_WAIT;
        isAlive = true;
        isMoving = false;
        isAttacking = false;
        isDying = false;
        health20Triggered = false;
        
        // 스탯 초기화
        currentHealth = 0;
        maxHealth = 0;
        attackPower = 0;
        defense = 0;
        
        // 전투 상태 초기화
        target = null;
        lastAttackFrame = -1;
        framesSinceLastAttack = 0;
        hasAttacked = false;
        hasAttackStarted = false;
        hasGameStarted = false;
        
        // 애니메이션 초기화
        currentFrame = 0;
        frameCounter = 0;
        animationSpeed = 1f;
        
        // 스프라이트 초기화
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = null;
            spriteRenderer.flipX = false;
        }
        
        // UI 초기화
        if (healthBar != null)
        {
            healthBar.SetActive(false);
        }
        
        // 위치 초기화
        targetPosition = Vector2.zero;
        defaultTargetPosition = Vector2.zero;
        
        // CC 상태 초기화
        ClearAllCC();
        
        // 활성 이펙트 정리
        ClearAllEffects();
    }
    #endregion
    
    #region Abstract Methods
    protected virtual void OnInitialize()
    {
        // 서브클래스에서 필요시 override
    }
    
    protected virtual void InitializeTextureName()
    {
        // 기본적으로 클래스 이름을 텍스처 이름으로 사용
        // 필요하면 서브클래스에서 override하여 변경 가능
    }
    
    private void SetupSpriteWithFootOffset()
    {
        // 기존 SpriteRenderer가 있는지 확인
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            // 이미 있으면 자식으로 이동
            GameObject spriteObj = spriteRenderer.gameObject;
            if (spriteObj == gameObject)
            {
                // 같은 GameObject에 있으면 새로 만들어야 함
                spriteRenderer = null;
            }
        }
        
        if (spriteRenderer == null)
        {
            // 스프라이트용 자식 GameObject 생성
            GameObject spriteObj = new GameObject("Sprite");
            spriteObj.transform.SetParent(transform);
            spriteObj.transform.localPosition = Vector3.zero;
            spriteObj.transform.localRotation = Quaternion.identity;
            spriteObj.transform.localScale = Vector3.one;
            
            spriteRenderer = spriteObj.AddComponent<SpriteRenderer>();
        }
        
        // spriteTransform 참조 저장
        spriteTransform = spriteRenderer.transform;
        
        // 디버그 십자선 생성 (AS3.0 스타일)
        // if (showCrosshair)
        // {
        //     CreateDebugCrosshair();
        // }
    }
    
    private void CreateDebugCrosshair()
    {
        // 가로선
        GameObject horizontalLine = new GameObject("Debug_Horizontal");
        horizontalLine.transform.SetParent(transform);
        horizontalLine.transform.localPosition = Vector3.zero;
        
        LineRenderer hLine = horizontalLine.AddComponent<LineRenderer>();
        hLine.startWidth = 1f;
        hLine.endWidth = 1f;
        hLine.material = new Material(Shader.Find("Sprites/Default"));
        hLine.startColor = crosshairColor;
        hLine.endColor = crosshairColor;
        hLine.positionCount = 2;
        hLine.SetPosition(0, new Vector3(-crosshairSize, 0, 0));
        hLine.SetPosition(1, new Vector3(crosshairSize, 0, 0));
        hLine.sortingOrder = 100; // 스프라이트 위에 표시
        hLine.useWorldSpace = false; // 로컬 좌표 사용
        
        // 세로선
        GameObject verticalLine = new GameObject("Debug_Vertical");
        verticalLine.transform.SetParent(transform);
        verticalLine.transform.localPosition = Vector3.zero;
        
        LineRenderer vLine = verticalLine.AddComponent<LineRenderer>();
        vLine.startWidth = 1f;
        vLine.endWidth = 1f;
        vLine.material = new Material(Shader.Find("Sprites/Default"));
        vLine.startColor = crosshairColor;
        vLine.endColor = crosshairColor;
        vLine.positionCount = 2;
        vLine.SetPosition(0, new Vector3(0, -crosshairSize, 0));
        vLine.SetPosition(1, new Vector3(0, crosshairSize, 0));
        vLine.sortingOrder = 100; // 스프라이트 위에 표시
        vLine.useWorldSpace = false; // 로컬 좌표 사용
    }
    
     public void SetSize(float size)
     {
        transform.localScale = new Vector3(size * orgSzie, size * orgSzie, 0);
     }


    protected virtual void UpdateLogic()
    {
        // 레거시 메서드 - 삭제 예정
    }
    #endregion
    
    #region State Transition Methods (AS3.0 Style)
    protected virtual void GotoWaitState()
    {
        if (!isAlive) return;
        if (state == STATE_WAIT) return;
        
        SetState(STATE_WAIT);
        
        // 타겟이 없으면 기본 목적지 방향 보기
        if (target == null)
        {
            UpdateFacing(defaultTargetPosition.x - transform.position.x);
        }
    }
    
    protected virtual void GotoMoveState()
    {
        if (state == STATE_MOVE) return;
        SetState(STATE_MOVE);
    }
    
    protected virtual void GotoAttackState(BaseHero targetHero)
    {
        if (framesSinceLastAttack < attackIntervalFrames) return;
        if (targetHero == null || !targetHero.IsAlive) return;
        
        target = targetHero.transform;
        SetState(STATE_ATTACK, false);
        framesSinceLastAttack = 0;
        
        // 공격 플래그 초기화
        hasAttacked = false;
        hasAttackStarted = false;
    }
    
    protected virtual void GotoSkillState(BaseHero targetHero)
    {
        if (framesSinceLastAttack < attackIntervalFrames) return;
        if (targetHero == null || !targetHero.IsAlive) return;
        
        target = targetHero.transform;
        SetState(STATE_SKILL, false);
        framesSinceLastAttack = 0;
    }
    
    protected virtual void GotoDieState()
    {
        if (state == STATE_DIE || isDying) return;
        
        isAlive = false;
        isDying = true;
        target = null;
        SetState(STATE_DIE, false);
    }
    #endregion
    
    #region State Methods (AS3.0 Style)
    protected virtual void DoWait()
    {
        // 도발 중이면 타겟 변경 불가 (도발한 대상만 공격)
        if (tauntCount > 0)
        {
            // 도발한 대상이 죽었으면 도발 해제
            if (target == null || target.GetComponent<BaseHero>()?.IsAlive != true)
            {
                tauntCount = 0;
            }
        }
        
        // 타겟이 없거나 죽었으면 즉시 새 타겟 찾기 (도발 중이 아닐 때만)
        if (tauntCount <= 0 && (target == null || target.GetComponent<BaseHero>()?.IsAlive != true))
        {
            target = FindNearestEnemy();
            
            // 새 타겟을 찾았으면
            if (target != null)
            {
                BaseHero targetHero = target.GetComponent<BaseHero>();
                if (targetHero != null && targetHero.IsAlive)
                {
                    float distance = Vector2.Distance(transform.position, target.position);
                    
                    // 사거리 내에 있고 공격 가능하면 공격
                    if (distance <= heroData.attackRange && framesSinceLastAttack >= attackIntervalFrames)
                    {
                        GotoAttackState(targetHero);
                        return;
                    }
                    // 사거리 밖이면 이동
                    else if (distance > heroData.attackRange)
                    {
                        GotoMoveState();
                        return;
                    }
                    // 사거리 내에 있지만 공격 쿨다운 중이면 대기
                }
            }
        }
        // 타겟이 있고 살아있으면
        else if (target != null)
        {
            BaseHero targetHero = target.GetComponent<BaseHero>();
            if (targetHero != null && targetHero.IsAlive)
            {
                float distance = Vector2.Distance(transform.position, target.position);
                
                // 공격 쿨다운이 끝났으면
                if (framesSinceLastAttack >= attackIntervalFrames)
                {
                    // 사거리 내에 있으면 공격
                    if (distance <= heroData.attackRange)
                    {
                        GotoAttackState(targetHero);
                        return;
                    }
                    // 사거리 밖이면 이동
                    else
                    {
                        GotoMoveState();
                        return;
                    }
                }
            }
            else
            {
                // 타겟이 죽었으면 null로
                target = null;
            }
        }
    }
    
    protected virtual void DoMove()
    {
        if (!isAlive) return;
        
        // 도발 중이면 타겟 변경 불가
        if (tauntCount > 0)
        {
            // 도발한 대상이 죽었으면 도발 해제
            if (target == null || target.GetComponent<BaseHero>()?.IsAlive != true)
            {
                tauntCount = 0;
                target = null;
            }
        }
        else
        {
            // 타겟이 사거리 밖에 있으면 타겟 초기화
            if (target != null)
            {
                float distance = Vector2.Distance(transform.position, target.position);
                if (distance > heroData.attackRange * 2f) // 여유를 둬서 체크
                {
                    target = null;
                }
            }
            
            // 타겟이 없거나 죽었으면 새로 찾기
            if (target == null || target.GetComponent<BaseHero>()?.IsAlive != true)
            {
                target = FindNearestEnemy();
            }
        }
        
        // ★★★ 핵심 수정: 타겟이 있으면 먼저 공격 범위 체크! ★★★
        if (target != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, target.position);
            
            // 공격 범위 내에 있으면 이동하지 않고 공격만!
            if (distanceToTarget <= heroData.attackRange)
            {
                // 타겟 방향으로 회전만 하고
                UpdateFacing(target.position.x - transform.position.x);
                
                // 공격 쿨다운 확인 후 공격
                if (framesSinceLastAttack >= attackIntervalFrames)
                {
                    BaseHero targetHero = target.GetComponent<BaseHero>();
                    if (targetHero != null)
                    {
                        GotoAttackState(targetHero);
                    }
                }
                // ★ 공격 범위 내에 있으면 여기서 즉시 리턴! 이동 코드 실행 안함!
                return;
            }
            
            // ★ 공격 범위 밖에 있을 때만 목표 위치 계산
            Vector2 directionToTarget = (target.position - transform.position).normalized;
            float optimalDistance = heroData.attackRange * 0.85f; // 공격 범위의 85% 거리
            targetPosition = (Vector2)target.position - directionToTarget * optimalDistance;
        }
        else
        {
            // 타겟이 없으면 기본 목적지로
            targetPosition = defaultTargetPosition;
        }
        
        // 목적지까지의 거리 체크
        float distanceToTargetPosition = Vector2.Distance(transform.position, targetPosition);
        
        // 목적지 도착 체크
        if (distanceToTargetPosition < 1.0f)
        {
            // 타겟이 없을 때만 대기 상태로
            if (target == null)
            {
                transform.position = targetPosition;
                GotoWaitState();
            }
            // 타겟이 있으면 그냥 멈춤 (대기 상태로 전환하지 않음)
            return;
        }
        
        // ★ 이동 실행 (공격 범위 밖에 있을 때만 실행됨)
        Vector2 moveDirection = (targetPosition - (Vector2)transform.position).normalized;
        float currentSpeed = heroData.moveSpeed;
        
        // 둔화 효과 적용 (50% 속도 감소)
        if (slowCount > 0)
        {
            currentSpeed *= 0.5f;
        }
        
        transform.position += (Vector3)(moveDirection * currentSpeed);
        
        // 방향 업데이트
        if (target != null)
        {
            UpdateFacing(target.position.x - transform.position.x);
        }
        else
        {
            UpdateFacing(moveDirection.x);
        }
    }
    
    protected virtual void DoAttack()
    {
        // 타겟이 없거나 죽었으면
        if (target == null || target.GetComponent<BaseHero>()?.IsAlive != true)
        {
            target = null;
            hasAttacked = false;
            hasAttackStarted = false;
            // 대기 상태로 전환
            GotoWaitState();
            return;
        }
        
        // 공격 시작 처리 (AS3.0 style)
        if (!hasAttackStarted)
        {
            hasAttackStarted = true;
            // 타겟 방향 보기
            UpdateFacing(target.position.x - transform.position.x);
            OnAttackStart();
        }
        
        // 공격 프레임 체크 및 실행 (개선: lastAttackFrame만으로 중복 방지)
        if (attackFrameDict.ContainsKey(currentFrame) && currentFrame != lastAttackFrame)
        {
            AttackMain();
            lastAttackFrame = currentFrame;
            
            // 프레임 속도에 관계없이 각 공격 프레임마다 실행 보장
            // hasAttacked 제거로 여러 공격 프레임 지원
        }
        
        // 공격 애니메이션 종료 체크
        if (currentFrame >= animEndFrame)
        {
            // 대기 상태로 전환
            GotoWaitState();
            hasAttacked = false;
            hasAttackStarted = false;
            lastAttackFrame = -1;
        }
    }
    
    protected virtual void DoSkill()
    {
        // 스킬 시작 처리
        if (!hasAttackStarted)
        {
            hasAttackStarted = true;
            OnSkillStart();
        }
        
        // 스킬 프레임 체크 및 실행 (개선: lastSkillFrame만으로 중복 방지)
        if (skillFrameDict.ContainsKey(currentFrame) && currentFrame != lastSkillFrame)
        {
            SkillMain();
            lastSkillFrame = currentFrame;
            
            // 프레임 속도에 관계없이 각 스킬 프레임마다 실행 보장
            // hasSkilled 제거로 여러 스킬 프레임 지원
        }
        
        // 스킬 애니메이션 종료 체크
        if (currentFrame >= animEndFrame)
        {
            // 대기 상태로 전환
            GotoWaitState();
            hasSkilled = false;
            hasAttackStarted = false;
            lastSkillFrame = -1;
        }
    }
    
    protected virtual void DoDie()
    {
        // 죽음 애니메이션이 끝나면 제거 (OnAnimationComplete -> OnDie에서 처리)
        // 추가 처리가 필요한 경우 서브클래스에서 override
        HideHealthBar();
        RemoveAllDots();
        ClearAllEffects();
    }
    #endregion

    #region Sprite Loading
    protected virtual void LoadSprites()
    {
        // AS3.0/PixiJS 스타일 - heroData.heroClass 사용, 없으면 클래스 이름 사용
        string spriteName = (heroData != null && !string.IsNullOrEmpty(heroData.heroClass)) 
            ? heroData.heroClass 
            : className;
        
        // TextureManager를 통해 로드 (TextureManager가 내부적으로 캐싱함)
        spriteList = TextureManager.GetSprites(sheetName, spriteName);
        
        if (spriteList == null || spriteList.Length == 0)
        {
            // Failed to load sprites
            return;
        }
        
        
        // 첫 프레임 설정
        if (spriteList.Length > 0)
        {
            currentFrame = 0;
            spriteRenderer.sprite = spriteList[currentFrame];
        }
    }
    #endregion

    #region UI and Effects Management
    public virtual void ShowHealthBar(GameObject healthBarPrefab = null)
    {
        if (healthBar == null && healthBarPrefab != null)
        {
            healthBar = Instantiate(healthBarPrefab, transform);
            healthBarScript = healthBar.GetComponent< HealthBar >();
            healthBarScript.Draw( true );
            healthBar.transform.localPosition = new Vector3( TargetWidth * -1f, TargetHeight * 1.2f , 0); // 머리 위
            // 체력바는 flipX 영향 받지 않음
        }
        
        if (healthBar != null)
        {
            healthBar.SetActive(true);
        }
    }
    
    public virtual void HideHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.SetActive(false);
        }
    }
    
    // public virtual GameObject AddEffect(GameObject effectPrefab, float duration = 0)
    // {
    //     if (effectPrefab == null) return null;
        
    //     GameObject effect = Instantiate(effectPrefab, transform);
    //     effect.transform.localPosition = Vector3.zero;
        
    //     if (duration > 0)
    //     {
    //         Destroy(effect, duration);
    //     }
        
    //     return effect;
    // }
    
    // public virtual GameObject AddEffectAtPosition(GameObject effectPrefab, Vector3 worldPosition, float duration = 0)
    // {
    //     if (effectPrefab == null) return null;
        
    //     GameObject effect = Instantiate(effectPrefab, transform);
    //     effect.transform.position = worldPosition;
        
    //     if (duration > 0)
    //     {
    //         Destroy(effect, duration);
    //     }
        
    //     return effect;
    // }
    #endregion

    #region Frame Update
    public virtual void Execute()
    {
        // 게임 시작 처리 (한 번만)
        if (!hasGameStarted)
        {
            hasGameStarted = true;
            OnGameStart();
        }
        
        // 살아있지 않으면 죽음 처리만
        if (!isAlive && state != STATE_DIE)
        {
            SetState(STATE_DIE, false);
        }
        
        // 개별 영웅 가속 처리 (frameCounter 기반 반복 실행)
        // animationSpeed가 1보다 클 때 Execute 로직을 여러 번 실행
        frameCounter += animationSpeed;
        
        while (frameCounter >= 1f)
        {
            frameCounter -= 1f;
            
            // === 프레임당 실행되어야 할 모든 로직 ===
            
            // 프레임 전처리 (CC 처리 포함)
            PreFrameUpdate();
            
            // Active Buff System 업데이트 (매 프레임)
            UpdateActiveBuffs();
            
            // Shield 지속시간 업데이트
            UpdateShieldDuration();
            
            // DOT 시스템 업데이트
            UpdateDotDamage();
            UpdateDotHeal();
            
            // CC 상태 체크 - 기절, 빙결, 수면 중이면 행동 불가
            bool isStunned = stunCount > 0 || freezeCount > 0 || sleepCount > 0;
            
            if (!isStunned)
            {
                // 상태에 따른 처리
                switch (state)
                {
                    case STATE_WAIT:
                        DoWait();
                        break;
                        
                    case STATE_MOVE:
                        // 넉백이나 속박 중이면 이동 불가
                        if (knockbackCount <= 0 && rootCount <= 0)
                        {
                            DoMove();
                        }
                        break;
                        
                    case STATE_ATTACK:
                        // 무장해제 중이면 일반 공격 불가
                        if (disarmCount <= 0)
                        {
                            DoAttack();
                        }
                        break;
                        
                    case STATE_SKILL:
                        // 침묵 중이면 스킬 사용 불가
                        if (silenceCount <= 0)
                        {
                            DoSkill();
                        }
                        break;
                        
                    case STATE_DIE:
                        DoDie();
                        break;
                }
            }
            
            // 프레임 업데이트 (애니메이션 진행)
            UpdateFrameSingle();
            
            // 체력 체크
            CheckHealth();
        }
    }
    
    protected virtual void OnGameStart()
    {
        // BattleStatisticsManager 등록은 BattleController.SetTeams()에서 처리
        // 여기서는 영웅별 초기화만 수행
    }
    
    protected virtual void PreFrameUpdate()
    {
        // 공격 인터벌 카운트
        if (framesSinceLastAttack < int.MaxValue) // 오버플로우 방지
        {
            framesSinceLastAttack++;
        }
        
        // 타격 반응 업데이트
        if (isHitReacting)
        {
            UpdateHitReaction();
        }
        
        // CC 처리 - CC가 활성화되어 있으면 true를 반환
        if (UpdateCrowdControl())
        {
            // CC 상태에서는 대부분의 행동이 제한됨
            return;
        }
        
        // TODO: DOT 데미지 처리
        // TODO: DOT 힐 처리
    }
    
    // 타격 반응 애니메이션 업데이트
    protected virtual void UpdateHitReaction()
    {
        hitReactionFrame++; // 프레임 증가
        
        if (hitReactionFrame >= hitReactionDurationFrames)
        {
            // 반응 종료 - 원래 회전으로 복귀
            spriteTransform.localRotation = originalRotation;
            isHitReacting = false;
        }
        else
        {
            // 회전 애니메이션 (sin 커브로 부드럽게)
            float progress = (float)hitReactionFrame / (float)hitReactionDurationFrames;
            float curve = Mathf.Sin(progress * Mathf.PI); // 0 -> 1 -> 0 커브
            
            // 타격 방향에 따라 회전 (flipX 상태에 따라)
            float rotationDirection = spriteRenderer.flipX ? -1f : 1f;
            float currentAngle = hitReactionAngle * curve * rotationDirection;
            
            spriteTransform.localRotation = originalRotation * Quaternion.Euler(0, 0, currentAngle);
        }
    }
    
    // Crowd Control 업데이트 - CC가 활성화되어 있으면 true 반환
    protected virtual bool UpdateCrowdControl()
    {
        // 모든 CC 카운터 무조건 감소 (분기 없이 직접 감소 - 성능 최적화)
        stunCount--;
        freezeCount--;
        sleepCount--;
        knockbackCount--;
        silenceCount--;
        rootCount--;
        tauntCount--;
        blindCount--;
        disarmCount--;
        slowCount--;
        
        // 모든 면역 카운터도 무조건 감소
        immuneToStunCount--;
        immuneToFreezeCount--;
        immuneToKnockbackCount--;
        immuneToSleepCount--;
        immuneToSilenceCount--;
        immuneToRootCount--;
        immuneToTauntCount--;
        immuneToBlindCount--;
        immuneToDisarmCount--;
        immuneToSlowCount--;
        
        // 살아있지 않으면 처리 안함
        if (!isAlive) return false;
        
        // 넉백 처리 - 매 프레임 점진적으로 밀림
        if (knockbackCount > 0)
        {
            transform.position += (Vector3)(knockbackDirection * knockbackSpeed);
        }
        
        // 행동 불가 상태 체크 (기절, 빙결, 수면)
        bool isStunned = (stunCount > 0 || freezeCount > 0 || sleepCount > 0);
        
        // CC 상태일 때 WAIT 애니메이션 유지
        if (isStunned && state != STATE_WAIT && state != STATE_DIE)
        {
            SetState(STATE_WAIT);
        }
        
        return isStunned;
    }

    // 기존 UpdateFrame() - 더 이상 사용하지 않음 (레거시 호환성을 위해 남겨둠)
    protected virtual void UpdateFrame()
    {
        // Execute()에서 frameCounter 처리로 이동
        // 이 메서드는 더 이상 직접 호출되지 않음
        UpdateFrameSingle();
    }
    
    // 단일 프레임 업데이트 (Execute의 while 루프 내에서 호출)
    protected virtual void UpdateFrameSingle()
    {
        if (spriteList == null || spriteList.Length == 0) 
        {
            return;
        }
        
        // 빙결 상태면 애니메이션 정지 (프레임 진행 안함)
        if (freezeCount > 0)
        {
            return;
        }
        
        // 다음 프레임으로 (frameCounter는 Execute()에서 처리)
        currentFrame++;
        framesSinceLastAttacked ++;

        
        // 애니메이션 범위 체크
        if (currentFrame > animEndFrame)
        {
            if (isLooping)
            {
                currentFrame = animStartFrame;
            }
            else
            {
                currentFrame = animEndFrame;
                OnAnimationComplete();
            }
        }

        if( framesSinceLastAttacked > 60 ) {
            HideHealthBar();
        } else {
            ShowHealthBar( healthBarPrefab );
        }
        

        // 스프라이트 업데이트
        UpdateSprite();
    }

    protected virtual void UpdateSprite()
    {
        if (currentFrame >= 0 && currentFrame < spriteList.Length)
        {
            spriteRenderer.sprite = spriteList[currentFrame];
        }
    }
    #endregion

    #region Animation Control
    public virtual void GotoAndPlay(int frame)
    {
        if (spriteList == null || spriteList.Length == 0) return;
        
        currentFrame = Mathf.Clamp(frame, 0, spriteList.Length - 1);
        // frameCounter는 Execute()에서 관리하므로 여기서 초기화하지 않음
        UpdateSprite();
    }

    public virtual void SetState(int newState, bool loop = true)
    {
        // 상태가 같더라도 heroData가 새로 설정된 경우 애니메이션 프레임 재설정 필요
        bool needsUpdate = (state != newState) || (isLooping != loop) || (animStartFrame == 0 && animEndFrame == 0);
        
        if (!needsUpdate && state == newState && isLooping == loop) 
        {
            return;
        }
        
        state = newState;
        isLooping = loop;
        // frameCounter는 Execute()에서 관리하므로 여기서 초기화하지 않음
        
        
        // 상태가 변경되면 마지막 공격 프레임 리셋
        if (state != STATE_ATTACK)
        {
            lastAttackFrame = -1;
        }
        
        switch (state)
        {
            case STATE_WAIT:
                if (heroData != null)
                {
                    animStartFrame = heroData.startWait;
                    animEndFrame = heroData.endWait;
                }
                else
                {
                    animStartFrame = 0;
                    animEndFrame = 3; // 기본값
                }
                break;
            case STATE_MOVE:
                if (heroData != null)
                {
                    animStartFrame = heroData.startMove;
                    animEndFrame = heroData.endMove;
                }
                else
                {
                    animStartFrame = 0;
                    animEndFrame = 3;
                }
                break;
            case STATE_ATTACK:
                if (heroData != null)
                {
                    animStartFrame = heroData.startAttack;
                    animEndFrame = heroData.endAttack;
                    isAttacking = true;
                }
                else
                {
                    animStartFrame = 0;
                    animEndFrame = 3;
                }
                break;
            case STATE_SKILL:
                if (heroData != null)
                {
                    animStartFrame = heroData.startSkill;
                    animEndFrame = heroData.endSkill;
                }
                else
                {
                    animStartFrame = 0;
                    animEndFrame = 3;
                }
                break;
            case STATE_DIE:
                if (heroData != null)
                {
                    animStartFrame = heroData.startDie;
                    animEndFrame = heroData.endDie;
                    isLooping = false;
                }
                else
                {
                    animStartFrame = 0;
                    animEndFrame = 3;
                    isLooping = false;
                }
                break;
            default:
                // Unknown state
                return;
        }
        
        currentFrame = animStartFrame;
        UpdateSprite();
    }

    protected virtual void OnAnimationComplete()
    {
        if (state == STATE_ATTACK)
        {
            isAttacking = false;
            hasAttacked = false;
            hasAttackStarted = false;
            OnAttackComplete();
            GotoWaitState();
        }
        else if (state == STATE_DIE)
        {
            OnDie();
        }
        else if (state == STATE_SKILL)
        {
            SetState(STATE_WAIT);
            OnSkillComplete();
        }
    }
    #endregion

    #region Event Handlers
    protected virtual void OnDie()
    {
        // 이미 죽음 처리 중이면 중복 호출 방지
        if (!isAlive) return;
        
        isAlive = false;
        isDying = true;
        isDeathAnimationComplete = true; // 죽는 애니메이션 완료 표시
        
        // 타입 기반 통계 기록 - 죽음 (킬은 공격자 측에서 기록)
        // RecordKill에서 자동으로 처리되므로 여기서는 생략
        
        // 아군들에게 죽음 알림 (AS3.0 style)
        if (friendList != null)
        {
            foreach (BaseHero friend in friendList)
            {
                if (friend != null && friend != this && friend.IsAlive)
                {
                    friend.OnFriendDie(this);
                }
            }
        }
        
        // 여기서 오브젝트 풀로 반환하거나 Destroy
        CancelInvoke("ReturnToPool"); // 기존 Invoke 취소
        Invoke("ReturnToPool", 2f); // 2초 후 제거
    }

    protected virtual void OnHealth20()
    {
        // 서브클래스에서 특수 스킬 발동 등 구현
    }

    protected virtual void OnFriendDie(BaseHero friend)
    {
        // 서브클래스에서 버프 등 구현
    }

    protected virtual void OnKillEnemy(BaseHero enemy)
    {
        // 서브클래스에서 경험치 획득 등 구현
        // 예: 레벨업 이펙트
        // EffectFactory.PlayEffect(EffectType.LEVEL_UP, transform.position);
    }

    protected virtual void OnAttackStart()
    {
        // 공격 시작 시 호출 (서브클래스에서 override)
    }
    
    protected virtual void OnSkillStart()
    {
        // 스킬 시작 시 호출 (서브클래스에서 override)
    }
    
    protected virtual void AttackMain()
    {
        if (target == null || !isAlive) return;
        
        float distance = Vector2.Distance(transform.position, target.position);
        
        // 사거리 내에 있는지 확인
        if (distance <= heroData.attackRange)
        {
            if (heroData.isRanged)
            {
                DoRangeAttack();
            }
            else
            {
                DoMeleeAttack();
            }
        }
        else
        {
            // Target out of range
        }
    }
    
    protected virtual void SkillMain()
    {
        // 스킬 실행 (서브클래스에서 override)
        // 기본적으로는 일반 공격과 동일하게 처리
        AttackMain();
    }
    
    protected virtual void DoMeleeAttack()
    {
        if (target == null) return;
        
        BaseHero targetHero = target.GetComponent<BaseHero>();
        if (targetHero != null)
        {
            // 실명 상태면 50% 확률로 빗나감
            if (blindCount > 0 && UnityEngine.Random.Range(0f, 1f) < 0.5f)
            {
                // 빗나감 이펙트 (옵션)
                // ShowMissEffect(targetHero);
                return;
            }
            
            // 타격 이펙트 표시
            ShowHitEffect(targetHero, EffectType.PHYSICAL_HIT);
            
            // DamageManager를 통한 데미지 계산 및 적용
            DoDamage(targetHero, attackPower);
            
            // 넉백 적용 (타겟이 살아있을 때만)
            if (targetHero.IsAlive)
            {
                ApplyKnockback(targetHero);
            }
            // 적을 죽였는지 확인
            else
            {
                OnKillEnemy(targetHero);
            }
        }
    }

    protected void ShowHitEffect(BaseHero targetHero, EffectType effectType)
    {
        SimpleEffect hitEffect = EffectFactory.PlayEffect(effectType);
        if (hitEffect != null)
        {           
            // 타겟 영웅의 자식으로 설정
            hitEffect.SetParent(targetHero.transform);
            
            // 변환된 로컬 좌표로 설정 (화살이 맞은 위치에 이펙트 표시)
            hitEffect.x = 0;
            hitEffect.y = targetHero.TargetHeight * .5f;
            hitEffect.Play();
        }
    }
    
    // CC 이펙트 표시 (중복 방지)
    protected void ShowCCEffect(EffectType effectType, int duration, float x, float y)
    {
        SimpleEffect ccEffect = null;
        
        // 기존 이펙트가 있는지 확인
        if (activeEffects.TryGetValue(effectType, out ccEffect))
        {
            // 기존 이펙트가 있고 활성화되어 있으면 재사용
            if (ccEffect != null && ccEffect.gameObject.activeInHierarchy)
            {
                // 기존 이펙트의 duration만 갱신하고 재생
                ccEffect.Play(duration);
                return;
            }
            else
            {
                // 비활성화되어 있거나 null이면 딕셔너리에서 제거
                activeEffects.Remove(effectType);
            }
        }
        
        // 새로운 이펙트 생성
        ccEffect = EffectFactory.PlayEffect(effectType);
        if (ccEffect != null)
        {
            // 자신에게 이펙트 표시
            ccEffect.SetParent(transform);
            ccEffect.x = x;
            ccEffect.y = y;
            ccEffect.Play(duration);
            
            // 딕셔너리에 저장
            activeEffects[effectType] = ccEffect;
        }
    }
    
    // 모든 활성 이펙트 제거
    protected void ClearAllEffects()
    {
        foreach (var kvp in activeEffects)
        {
            if (kvp.Value != null && kvp.Value.gameObject.activeInHierarchy)
            {
                kvp.Value.Remove();
            }
        }
        activeEffects.Clear();
    }
    
    // 특정 이펙트 제거
    protected void RemoveEffect(EffectType effectType)
    {
        SimpleEffect effect;
        if (activeEffects.TryGetValue(effectType, out effect))
        {
            if (effect != null && effect.gameObject.activeInHierarchy)
            {
                effect.Remove();
            }
            activeEffects.Remove(effectType);
        }
    }
    
    // 즉시 넉백 적용 (근접 공격용)
    protected virtual void ApplyKnockback(BaseHero targetHero)
    {
        if (targetHero == null) return;
        
        // 공격자에서 타겟으로의 방향 계산
        Vector2 knockbackDir = (targetHero.transform.position - transform.position).normalized;
        
        // 즉시 넉백 적용 (기존 방식)
        targetHero.KnockbackInstant(knockbackDir, knockbackDistance);
    }
    
    protected virtual void DoRangeAttack()
    {
        if (target == null) return;
        
        BaseHero targetHero = target.GetComponent<BaseHero>();
        if (targetHero != null)
        {
            // 실명 상태면 50% 확률로 빗나감
            if (blindCount > 0 && UnityEngine.Random.Range(0f, 1f) < 0.5f)
            {
                // 빗나감 - 화살은 발사하지 않음
                return;
            }
            
            // 무기 발사 (heroData에 weaponClass가 있다면 사용)
            if (!string.IsNullOrEmpty(heroData.weaponClass) && WeaponFactory.Instance != null)
            {
                // WeaponFactory를 통해 무기 생성 및 발사
                BaseWeapon weapon = WeaponFactory.Instance.GetWeapon(heroData.weaponClass, this, targetHero, 1f);
                if (weapon != null)
                {
                    // 무기 발사 위치 설정 (weaponX, weaponY 오프셋 적용)
                    Vector3 weaponPos = transform.position;
                    
                    // flip 상태에 따라 X 오프셋 반전
                    float xOffset = heroData.weaponX;
                    if (spriteRenderer != null && spriteRenderer.flipX)
                    {
                        xOffset = -xOffset;
                    }
                    
                    weaponPos.x += xOffset;
                    weaponPos.y += heroData.weaponY;
                    weapon.transform.position = weaponPos;
                    
                    // 무기가 자체적으로 데미지 처리
                    return;
                }
                else
                {
                    // Failed to create weapon
                }
            }
            
            // 무기가 없으면 즉시 데미지 (폴백)
            // DamageManager를 통한 데미지 계산 및 적용
            DoDamage(targetHero, attackPower);
            
            // 적을 죽였는지 확인
            if (!targetHero.IsAlive)
            {
                OnKillEnemy(targetHero);
            }
        }
    }
    
    protected virtual void OnAttackComplete()
    {
        // 공격 애니메이션이 끝났을 때의 처리
        // AttackMain과는 별개로, 애니메이션 종료 시점 처리용
    }

    protected virtual void OnSkillComplete()
    {
        // 스킬 효과 처리 등
    }
    #endregion

    #region Initialization
    // 영웅 재사용 시 자식 이펙트 정리
    protected virtual void CleanupChildEffects()
    {
        // 모든 자식 오브젝트 중 SimpleEffect 컴포넌트를 가진 것들 제거
        SimpleEffect[] childEffects = GetComponentsInChildren<SimpleEffect>();
        foreach (SimpleEffect effect in childEffects)
        {
            if (effect != null && effect.gameObject != gameObject)
            {
                effect.Remove();
            }
        }
    }
    
    protected virtual void InitializeStats()
    {
        maxHealth = heroData.GetMaxHealth(level);
        currentHealth = maxHealth;
        attackPower = heroData.GetAttackPower(level);
        defense = heroData.GetDefense(level);
        
        // 공격 인터벌 설정
        attackIntervalFrames = heroData.attackInterval;
        
        // 저항력 설정
        knockbackResist = heroData.knockbackResist / 100f;
        
        // 속성 및 특수 능력 설정
        property = heroData.property;  // 속성 (1: 철, 2: 화, 3: 수, 4: 목)
        ignoreDefensePercentageOnCritically = heroData.ignoreDefensePercentageOnCritically;  // 치명타 시 방어 무시 확률
        skillInterruptionCnt = 0;  // 스킬 차단 카운트는 0으로 초기화 (버프로 증가)
        
        // 원본 스탯 저장 (버프 시스템용)
        SaveOriginalStats();
    }
    
    protected virtual void InitializeAttackFrames()
    {
        attackFrameDict.Clear();
        
        if (heroData != null && heroData.attackTriggerFrames != null)
        {
            foreach (int frame in heroData.attackTriggerFrames)
            {
                // 공격 프레임이 애니메이션 범위 내에 있는지 검증
                if (frame >= heroData.startAttack && frame <= heroData.endAttack)
                {
                    attackFrameDict[frame] = true;
                }
                else
                {
                    // Debug.LogWarning($"[BaseHero] Attack trigger frame {frame} is outside attack animation range ({heroData.startAttack}-{heroData.endAttack})");
                }
            }
        }
    }
    #endregion

    #region Combat & Health
    public virtual void TakeDamage(float damage)
    {
        if (!isAlive) return;
        
        float actualDamage = Mathf.Max(0, damage - defense);
        currentHealth -= actualDamage;
        
        // 수면 상태에서 깨어남
        if (sleepCount > 0)
        {
            sleepCount = 0;
        }
        
        // 타격 반응 시작 (죽지 않았을 때만)
        if (currentHealth > 0 && !isHitReacting)
        {
            StartHitReaction();
        }
        
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            GotoDieState();
        }
        
        healthBarScript.ChangeHp( currentHealth / maxHealth );
        framesSinceLastAttacked = 0;

    }
    
    // 타격 반응 시작
    protected virtual void StartHitReaction()
    {
        isHitReacting = true;
        hitReactionFrame = 0;
        originalRotation = spriteTransform.localRotation;
    }

    protected virtual void CheckHealth()
    {
        if (!health20Triggered && currentHealth <= maxHealth * 0.2f)
        {
            health20Triggered = true;
            OnHealth20();
        }
    }

    public virtual void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    public virtual void SetTarget(BaseHero targetHero)
    {
        if (targetHero != null)
        {
            target = targetHero.transform;
        }
        else
        {
            target = null;
        }
    }
    
    #region Crowd Control Methods
    // 기절 - 모든 행동 불가
    public virtual void Stun(int frames)
    {
        if (immuneToStunCount > 0) return;        
        if (stunCount > frames) return;
        
        stunCount = frames;  // 실제로 stunCount를 설정해야 함!
        if( state != STATE_DIE ) SetState(STATE_WAIT);  // CC 상태에서는 WAIT 애니메이션
        ShowCCEffect(EffectType.STUN, frames, 0, TargetHeight * 1f);
    }
    
    // 빙결 - 모든 행동 불가, 애니메이션 정지
    public virtual void Freeze(int frames)
    {
        if (immuneToFreezeCount > 0) return;        
        if (freezeCount > frames) return;
        
        freezeCount = frames;  // 실제로 freezeCount를 설정
        if( state != STATE_DIE ) SetState(STATE_WAIT);  // CC 상태에서는 WAIT 애니메이션
        ShowCCEffect(EffectType.FREEZE, frames, 0, 0);
    }
    
    // 수면 - 모든 행동 불가, 데미지 받으면 깨어남
    public virtual void Sleep(int frames)
    {
        if (immuneToSleepCount > 0) return;        
        if (sleepCount > frames) return;
        
        sleepCount = frames;  // 실제로 sleepCount를 설정
        if( state != STATE_DIE ) SetState(STATE_WAIT);  // CC 상태에서는 WAIT 애니메이션
        ShowCCEffect(EffectType.SLEEP, frames, 0, TargetHeight * 1.1f);
    }
    
    // 넉백 - 점진적으로 밀림 (프레임에 걸쳐서)
    public virtual void Knockback(int frames, Vector2 direction, float totalDistance)
    {
        if (immuneToKnockbackCount > 0) return;
        if (!isAlive) return;
        if (knockbackCount > frames) return; // 이미 더 강한 넉백 중이면 무시
        
        // 넉백 저항력 적용
        float actualFrames = frames * (1f - knockbackResist);
        float actualDistance = totalDistance * (1f - knockbackResist);
        
        if (actualFrames <= 0) return; // 완전 저항
        
        knockbackCount = Mathf.RoundToInt(actualFrames);
        knockbackDirection = direction.normalized;
        // 프레임당 이동 속도 = 총 거리 / 프레임 수
        knockbackSpeed = actualDistance / knockbackCount;
    }
    
    // 즉시 넉백 (근접 공격용 - 기존 방식)
    public virtual void KnockbackInstant(Vector2 direction, float distance)
    {
        if (immuneToKnockbackCount > 0) return;
        if (!isAlive) return;
        
        // 넉백 저항력 적용
        float actualDistance = distance * (1f - knockbackResist);
        transform.position += (Vector3)(direction.normalized * actualDistance);
    }
    
    // 침묵 - 스킬 사용 불가
    public virtual void Silence(int frames)
    {
        if (immuneToSilenceCount <= 0 && isAlive)
        {
            silenceCount = Mathf.Max(silenceCount, frames);
        }

        silenceCount = frames;  // 실제로 sleepCount를 설정
        if( state != STATE_DIE ) SetState(STATE_WAIT);  // CC 상태에서는 WAIT 애니메이션
        // ShowCCEffect(EffectType.POISON, frames, 0, TargetHeight * 1.1f);
    }
    
    // 속박 - 이동 불가
    public virtual void Root(int frames)
    {
        if (immuneToRootCount <= 0 && isAlive)
        {
            rootCount = Mathf.Max(rootCount, frames);
        }

        rootCount = frames;  // 실제로 sleepCount를 설정
        if( state != STATE_DIE ) SetState(STATE_WAIT);  // CC 상태에서는 WAIT 애니메이션
        // ShowCCEffect(EffectType.Root, frames, 0, 0 );
    }
    
    // 도발 - 특정 대상만 공격하도록 강제
    public virtual void Taunt(int frames, BaseHero taunter)
    {
        if (immuneToTauntCount <= 0 && isAlive)
        {
            tauntCount = Mathf.Max(tauntCount, frames);
            // 도발한 대상을 타겟으로 설정
            if (taunter != null && taunter.IsAlive)
            {
                SetTarget(taunter);
            }
        }
    }
    
    // 실명 - 명중률 감소
    public virtual void Blind(int frames)
    {
        if (immuneToBlindCount <= 0 && isAlive)
        {
            blindCount = Mathf.Max(blindCount, frames);
        }
    }
    
    // 무장해제 - 일반 공격 불가
    public virtual void Disarm(int frames)
    {
        if (immuneToDisarmCount <= 0 && isAlive)
        {
            disarmCount = Mathf.Max(disarmCount, frames);
        }
    }
    
    // 둔화 - 이동속도 감소
    public virtual void Slow(int frames)
    {
        if (immuneToSlowCount <= 0 && isAlive)
        {
            slowCount = Mathf.Max(slowCount, frames);
        }
    }
    
    // CC 상태 확인 메서드들
    public bool IsStunned => stunCount > 0;
    public bool IsFrozen => freezeCount > 0;
    public bool IsAsleep => sleepCount > 0;
    public bool IsKnockedBack => knockbackCount > 0;
    public bool IsSilenced => silenceCount > 0;
    public bool IsRooted => rootCount > 0;
    public bool IsTaunted => tauntCount > 0;
    public bool IsBlinded => blindCount > 0;
    public bool IsDisarmed => disarmCount > 0;
    public bool IsSlowed => slowCount > 0;
    
    // 모든 CC 해제
    public virtual void ClearAllCC()
    {
        stunCount = 0;
        freezeCount = 0;
        sleepCount = 0;
        knockbackCount = 0;
        silenceCount = 0;
        rootCount = 0;
        tauntCount = 0;
        blindCount = 0;
        disarmCount = 0;
        slowCount = 0;
        animationSpeed = 1f; // 애니메이션 속도 복구
    }
    
    // CC 면역 부여 메서드들
    public virtual void SetStunImmunity(int frames) { immuneToStunCount = frames; }
    public virtual void SetFreezeImmunity(int frames) { immuneToFreezeCount = frames; }
    public virtual void SetKnockbackImmunity(int frames) { immuneToKnockbackCount = frames; }
    public virtual void SetSleepImmunity(int frames) { immuneToSleepCount = frames; }
    public virtual void SetSilenceImmunity(int frames) { immuneToSilenceCount = frames; }
    public virtual void SetRootImmunity(int frames) { immuneToRootCount = frames; }
    public virtual void SetTauntImmunity(int frames) { immuneToTauntCount = frames; }
    public virtual void SetBlindImmunity(int frames) { immuneToBlindCount = frames; }
    public virtual void SetDisarmImmunity(int frames) { immuneToDisarmCount = frames; }
    public virtual void SetSlowImmunity(int frames) { immuneToSlowCount = frames; }
    
    // 모든 CC에 대한 면역 부여
    public virtual void SetAllCCImmunity(int frames)
    {
        immuneToStunCount = frames;
        immuneToFreezeCount = frames;
        immuneToKnockbackCount = frames;
        immuneToSleepCount = frames;
        immuneToSilenceCount = frames;
        immuneToRootCount = frames;
        immuneToTauntCount = frames;
        immuneToBlindCount = frames;
        immuneToDisarmCount = frames;
        immuneToSlowCount = frames;
    }
    
    // 면역 상태 확인
    public bool HasStunImmunity => immuneToStunCount > 0;
    public bool HasFreezeImmunity => immuneToFreezeCount > 0;
    public bool HasKnockbackImmunity => immuneToKnockbackCount > 0;
    #endregion

    #endregion

    #region Movement
    protected virtual void MoveTowardsTarget()
    {
        if (!isAlive || target == null) return;
        
        // 상태가 이동이 아니면 변경
        if (state != STATE_MOVE)
        {
            SetState(STATE_MOVE);
        }
        
        // 타겟까지의 방향 계산
        Vector3 direction = (target.position - transform.position).normalized;
        
        // 이동
        transform.position += direction * heroData.moveSpeed;
        
        // 좌/우 방향 업데이트 (x축만 체크)
        UpdateFacing(target.position.x - transform.position.x);
    }
    
    public virtual void UpdateFacing(float xDifference)
    {
        // x 차이가 음수면 타겟이 왼쪽에 있음
        if (xDifference < 0)
        {
            spriteRenderer.flipX = true;  // 왼쪽 보기
        }
        else if (xDifference > 0)
        {
            spriteRenderer.flipX = false; // 오른쪽 보기 (기본)
        }
        // xDifference == 0일 때는 현재 방향 유지
    }
    
    protected virtual Transform FindNearestEnemy()
    {
        if (enemyList == null || enemyList.Length == 0)
            return null;
        
        float closestDistance = float.MaxValue;
        Transform closestEnemy = null;
        
        foreach (BaseHero enemy in enemyList)
        {
            // null 체크, 살아있는지 확인
            if (enemy != null && enemy.IsAlive && enemy.gameObject.activeInHierarchy)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy.transform;
                }
            }
        }
        
        return closestEnemy;
    }
    
    protected virtual Transform FindNearestFriend()
    {
        if (friendList == null || friendList.Length == 0)
            return null;
        
        float closestDistance = float.MaxValue;
        Transform closestFriend = null;
        
        foreach (BaseHero friend in friendList)
        {
            // null 체크, 살아있는지 확인, 자기 자신 제외
            if (friend != null && friend != this && friend.IsAlive && friend.gameObject.activeInHierarchy)
            {
                float distance = Vector2.Distance(transform.position, friend.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestFriend = friend.transform;
                }
            }
        }
        
        return closestFriend;
    }
    
    
    public virtual void StartMove()
    {
        if (!isAlive) return;
        
        isMoving = true;
        SetState(STATE_MOVE);
    }

    public virtual void StopMove()
    {
        isMoving = false;
        if (isAlive && !isAttacking)
        {
            SetState(STATE_WAIT);
        }
    }
    #endregion

    #region Utility
    // ReturnToPool은 이미 Initialization Methods 섹션에 정의됨

    public virtual void ResetHero()
    {
        // 모든 Invoke 취소
        CancelInvoke();
        
        // 영웅 재사용 시 자식 이펙트 정리
        CleanupChildEffects();
        
        currentHealth = maxHealth;
        isAlive = true;
        isMoving = false;
        isAttacking = false;
        isDying = false;
        isDeathAnimationComplete = false;  // 죽음 애니메이션 완료 플래그 초기화
        health20Triggered = false;
        target = null;
        animationSpeed = 1f;
        lastAttackFrame = -1;
        framesSinceLastAttack = attackIntervalFrames;  // 즉시 공격 가능하도록
        hasAttacked = false;
        hasAttackStarted = false;
        hasGameStarted = false;  // 재사용 시 다시 게임 시작 처리를 받을 수 있도록
        targetPosition = defaultTargetPosition;
        SetState(STATE_WAIT);
        
        // UI 초기화
        if (healthBar != null)
        {
            healthBar.SetActive(false);
        }
        
        // 방향 초기화
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
        }
    }
    #endregion

    #region Properties
    public string HeroName => heroData != null ? heroData.heroName : className;
    public bool IsAlive => isAlive;
    public bool IsReadyToRemove => isDying && isDeathAnimationComplete; // 제거 준비 완료
    public float HealthPercent => currentHealth / maxHealth;
    public Transform Target => target;
    // State 프로퍼티는 이미 위에 정의되어 있음 (line 212)
    public HeroData Data => heroData;
    public int Level => level;
    public float TargetHeight => heroData != null ? heroData.targetHeight : 100f;
    public float TargetWidth => heroData != null ? heroData.targetWidth : 50f;
    
    /// <summary>
    /// 정렬된 순서에 따라 렌더링 순서 설정
    /// </summary>
    /// <param name="order">렌더링 순서 (낮을수록 뒤에, 높을수록 앞에)</param>
    public void SetSortingOrder(int order)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = order;
        }
    }
    
    public void SetDefaultTargetPosition(Vector2 position)
    {
        defaultTargetPosition = position;
    }
    
    public void SetDefaultTargetPosition(float x, float y)
    {
        defaultTargetPosition.x = x;
        defaultTargetPosition.y = y;
    }
    
    public bool IsPlayerTeam => isPlayerTeam;
    
    // 적군/아군 리스트 접근자 (무기 등에서 참조용)
    public BaseHero[] GetEnemyList() => enemyList;
    public BaseHero[] GetFriendList() => friendList;
    #endregion
    
    #region Static Battle Management (AS3.0 Style)
    public static void SetBattleLists(BaseHero[] playerTeam, BaseHero[] enemyTeam)
    {
        // 플레이어 팀 설정
        if (playerTeam != null)
        {
            foreach (BaseHero hero in playerTeam)
            {
                if (hero != null)
                {
                    hero.isPlayerTeam = true;
                }
            }
        }
        
        // 적 팀 설정
        if (enemyTeam != null)
        {
            foreach (BaseHero hero in enemyTeam)
            {
                if (hero != null)
                {
                    hero.isPlayerTeam = false;
                }
            }
        }
        
        // 각 영웅에게 적절한 리스트 할당
        foreach (BaseHero hero in playerTeam)
        {
            if (hero != null)
            {
                hero.SetTeamLists(playerTeam, enemyTeam);
            }
        }
        
        foreach (BaseHero hero in enemyTeam)
        {
            if (hero != null)
            {
                hero.SetTeamLists(enemyTeam, playerTeam);
            }
        }
    }
    
    protected virtual void SetTeamLists(BaseHero[] friends, BaseHero[] enemies)
    {
        friendList = friends;
        enemyList = enemies;
    }
    
    public static void ClearBattleLists()
    {
        // static이 아니므로 각 영웅의 리스트는 개별적으로 관리됨
        // 필요시 모든 영웅을 순회하여 초기화
    }
    #endregion
    
    #region Buff System Methods
    protected void InitializeBuffSystem()
    {
        // 버프 매니저 초기화
        activeDamageBuff = new ActiveBuffManager(-1f, 10f, 0f);
        activeDefenseBuff = new ActiveBuffManager(-1f, 10f, 0f);
        activeMaxHealthBuff = new ActiveBuffManager(-1f, 10f, 0f);
        activeAttackSpeedBuff = new ActiveBuffManager(-1f, 10f, 0f);
        activeMoveSpeedBuff = new ActiveBuffManager(-1f, 10f, 0f);
        activeCritChanceBuff = new ActiveBuffManager(-1f, 1f, 0f);
        activeCritMultiplierBuff = new ActiveBuffManager(-10f, 10f, 0f);
        activeDodgeChanceBuff = new ActiveBuffManager(-1f, 1f, 0f);
        activeDamageReductionBuff = new ActiveBuffManager(-1f, 1f, 0f);
        activePenetrateBuff = new ActiveBuffManager(0f, 2f, 0f);
        activeFinalDamageBuff = new ActiveBuffManager(0f, 10f, 0f);

        buffManagerList.Add( activeDamageBuff );
        buffManagerList.Add( activeDefenseBuff );
        buffManagerList.Add( activeMaxHealthBuff );
        buffManagerList.Add( activeAttackSpeedBuff );
        buffManagerList.Add( activeMoveSpeedBuff );
        buffManagerList.Add( activeCritChanceBuff );
        buffManagerList.Add( activeCritMultiplierBuff );
        buffManagerList.Add( activeDodgeChanceBuff );
        buffManagerList.Add( activeDamageReductionBuff );
        buffManagerList.Add( activePenetrateBuff );
        buffManagerList.Add( activeFinalDamageBuff );
    }
    
    protected void SaveOriginalStats()
    {
        // 현재 스탯을 원본으로 저장
        orgAtkDmg = attackPower;
        orgDefense = defense;
        orgMaxHealth = maxHealth;
        orgAtkDuration = attackIntervalFrames;
        orgMoveSpd = heroData.moveSpeed;
        orgCritChance = heroData.criticalChance / 100f;
        orgCritMultiplier = heroData.criticalMultiplier;
        orgDodgeChance = heroData.dodgeChance / 100f;
        orgDamageReduction = heroData.damageReduction / 100f;
        orgPenetrate = heroData.penetrate;
        orgFinalDamageMultiplier = 1f;
    }
    
    protected void UpdateActiveBuffs()
    {
        // 모든 버프 시간 진행 (1프레임)
        activeDamageBuff.AdvanceTime();
        activeDefenseBuff.AdvanceTime();
        activeMaxHealthBuff.AdvanceTime();
        activeAttackSpeedBuff.AdvanceTime();
        activeMoveSpeedBuff.AdvanceTime();
        activeCritChanceBuff.AdvanceTime();
        activeCritMultiplierBuff.AdvanceTime();
        activeDodgeChanceBuff.AdvanceTime();
        activeDamageReductionBuff.AdvanceTime();
        activePenetrateBuff.AdvanceTime();
        activeFinalDamageBuff.AdvanceTime();
        
        // 최대 체력 변경 전 저장
        float prevMaxHealth = maxHealth;
        
        // 버프 적용된 스탯 계산
        attackPower = orgAtkDmg * (1f + activeDamageBuff.Value);
        defense = orgDefense * (1f + activeDefenseBuff.Value);
        maxHealth = orgMaxHealth * (1f + activeMaxHealthBuff.Value);
        attackIntervalFrames = Mathf.RoundToInt(orgAtkDuration / (1f + activeAttackSpeedBuff.Value));
        // moveSpeed = orgMoveSpd * (1f + activeMoveSpeedBuff.Value);
        critChance = orgCritChance + activeCritChanceBuff.Value;
        critMultiplier = orgCritMultiplier + activeCritMultiplierBuff.Value;
        dodgeChance = orgDodgeChance + activeDodgeChanceBuff.Value;
        damageReduction = orgDamageReduction + activeDamageReductionBuff.Value;
        penetrate = orgPenetrate + activePenetrateBuff.Value;
        finalDamageMultiplier = orgFinalDamageMultiplier + activeFinalDamageBuff.Value;
        
        // 최대 HP 변경 처리
        if (maxHealth > prevMaxHealth)
        {
            // 최대 HP 증가 시 현재 HP도 증가
            currentHealth += (maxHealth - prevMaxHealth);
            if (healthBar != null)
            {
                // UI 업데이트
            }
        }
        else if (maxHealth < prevMaxHealth)
        {
            // 최대 HP 감소 시 현재 HP가 최대 HP를 초과하지 않도록
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
            if (healthBar != null)
            {
                // UI 업데이트
            }
        }
    }
    
    // 버프 추가 메서드들
    public void AddDamageBuff(int buffId, float value, int durationFrames, bool overrideBuff = false)
    {
        activeDamageBuff.AddBuff(buffId, value, durationFrames, overrideBuff);
    }
    
    public void AddDefenseBuff(int buffId, float value, int durationFrames, bool overrideBuff = false)
    {
        activeDefenseBuff.AddBuff(buffId, value, durationFrames, overrideBuff);
    }
    
    public void AddAttackSpeedBuff(int buffId, float value, int durationFrames, bool overrideBuff = false)
    {
        activeAttackSpeedBuff.AddBuff(buffId, value, durationFrames, overrideBuff);
    }
    
    public void AddMoveSpeedBuff(int buffId, float value, int durationFrames, bool overrideBuff = false)
    {
        activeMoveSpeedBuff.AddBuff(buffId, value, durationFrames, overrideBuff);
    }
    
    public void AddMaxHealthBuff(int buffId, float value, int durationFrames, bool overrideBuff = false)
    {
        activeMaxHealthBuff.AddBuff(buffId, value, durationFrames, overrideBuff);
    }
    
    // 모든 버프 제거
    public void RemoveAllBuffs()
    {
        activeDamageBuff.Reset();
        activeDefenseBuff.Reset();
        activeMaxHealthBuff.Reset();
        activeAttackSpeedBuff.Reset();
        activeMoveSpeedBuff.Reset();
        activeCritChanceBuff.Reset();
        activeCritMultiplierBuff.Reset();
        activeDodgeChanceBuff.Reset();
        activeDamageReductionBuff.Reset();
        activePenetrateBuff.Reset();
        activeFinalDamageBuff.Reset();
        
        // 스탯 재계산
        UpdateActiveBuffs();
    }
    
    // 모든 디버프만 제거
    public void RemoveAllDebuffs()
    {
        activeDamageBuff.RemoveAllNegativeBuffs();
        activeDefenseBuff.RemoveAllNegativeBuffs();
        activeMaxHealthBuff.RemoveAllNegativeBuffs();
        activeAttackSpeedBuff.RemoveAllNegativeBuffs();
        activeMoveSpeedBuff.RemoveAllNegativeBuffs();
        activeCritChanceBuff.RemoveAllNegativeBuffs();
        activeCritMultiplierBuff.RemoveAllNegativeBuffs();
        activeDodgeChanceBuff.RemoveAllNegativeBuffs();
        activeDamageReductionBuff.RemoveAllNegativeBuffs();
        activePenetrateBuff.RemoveAllNegativeBuffs();
        activeFinalDamageBuff.RemoveAllNegativeBuffs();
        
        // 스탯 재계산
        UpdateActiveBuffs();
    }
    #endregion
    
    #region Damage System Methods
    /// <summary>
    /// TypeScript의 doDamage 메서드를 Unity로 포팅
    /// DamageManager를 사용하여 중앙화된 데미지 계산
    /// </summary>
    /// <summary>
    /// DamageVO를 직접 받아서 데미지를 입히는 메서드 (투사체용)
    /// </summary>
    public virtual void DoDamage(BaseHero targetHero, DamageVO damageVO)
    {
        if (targetHero == null || !targetHero.IsAlive || damageVO == null) return;
        
        // 보호막 처리
        targetHero.SetShield(damageVO.shield);
        targetHero.SetShieldWithDuration(damageVO.shieldWithDuration);
        
        // 실제 데미지 적용
        if (damageVO.damage > 0)
        {
            float prevHealth = targetHero.CurrentHealth;
            targetHero.TakeDamage(damageVO.damage);
            
            // 타입 기반 통계 기록
            if (BattleStatisticsManager.Instance != null)
            {
                BattleStatisticsManager.Instance.RecordDamage(
                    this, 
                    targetHero, 
                    damageVO.damage, 
                    false,  // isDot = false
                    damageVO.isCritical,
                    -1      // skillId = -1 for normal attack
                );
                
                // 타겟이 죽었다면 킬 기록
                if (prevHealth > 0 && targetHero.CurrentHealth <= 0)
                {
                    BattleStatisticsManager.Instance.RecordKill(this, targetHero);
                }
            }
            
            // 크리티컬 데미지 이펙트
            if (damageVO.isCritical)
            {
                OnCriticalHit(targetHero, damageVO.damage);
            }
        }
    }
    
    public virtual void DoDamage(BaseHero targetHero, float damage, DamageBuffVO buffVO = null)
    {
        if (targetHero == null || !targetHero.IsAlive) return;
        
        // DamageManager를 통한 데미지 계산
        DamageVO damageVO = DamageManager.GetDamage(damage, this, targetHero, buffVO);
        
        // 보호막 업데이트
        targetHero.SetShield(damageVO.shield);
        targetHero.SetShieldWithDuration(damageVO.shieldWithDuration);
        
        // 실제 데미지 적용
        if (damageVO.damage > 0)
        {
            float prevHealth = targetHero.CurrentHealth;
            targetHero.TakeDamage(damageVO.damage);
            
            // 타입 기반 통계 기록 (일반 공격)
            if (BattleStatisticsManager.Instance != null)
            {
                BattleStatisticsManager.Instance.RecordDamage(
                    this, 
                    targetHero, 
                    damageVO.damage, 
                    false,  // isDot = false
                    damageVO.isCritical,
                    -1      // skillId = -1 for normal attack
                );
                
                // 타겟이 죽었다면 킬 기록
                if (prevHealth > 0 && targetHero.CurrentHealth <= 0)
                {
                    BattleStatisticsManager.Instance.RecordKill(this, targetHero);
                }
            }
            
            // 크리티컬 데미지 이펙트 (필요시 구현)
            if (damageVO.isCritical)
            {
                OnCriticalHit(targetHero, damageVO.damage);
            }
        }
    }
    
    /// <summary>
    /// 도트 데미지 적용 (방어력 무시)
    /// </summary>
    public virtual void DoDotDamage(BaseHero targetHero, float damage)
    {
        if (targetHero == null || !targetHero.IsAlive) return;
        
        DamageVO damageVO = DamageManager.GetDotDamage(damage, this, targetHero);
        
        // 보호막 업데이트
        targetHero.SetShield(damageVO.shield);
        targetHero.SetShieldWithDuration(damageVO.shieldWithDuration);
        
        // 실제 데미지 적용
        if (damageVO.damage > 0)
        {
            targetHero.TakeDamage(damageVO.damage);
        }
    }
    
    /// <summary>
    /// 크리티컬 히트 이벤트 (서브클래스에서 오버라이드 가능)
    /// </summary>
    protected virtual void OnCriticalHit(BaseHero target, float damage)
    {
        // 크리티컬 이펙트, 사운드 등 처리
        Debug.Log($"Critical Hit! {damage} damage to {target.name}");
    }
    
    // Shield 관련 메서드들
    public float GetShield()
    {
        return shield;
    }
    
    public void SetShield(float value)
    {
        shield = Mathf.Max(0, value);
    }
    
    public void AddShield(float value)
    {
        shield = Mathf.Max(0, shield + value);
    }
    
    public float GetShieldWithDuration()
    {
        return shieldWithDuration;
    }
    
    public void SetShieldWithDuration(float value)
    {
        shieldWithDuration = Mathf.Max(0, value);
    }
    
    public void AddShieldWithDuration(float value, int durationFrames)
    {
        shieldWithDuration = Mathf.Max(0, shieldWithDuration + value);
        shieldDurationFrames = Mathf.Max(shieldDurationFrames, durationFrames);
    }
    
    // Shield 시간 경과 처리
    protected void UpdateShieldDuration()
    {
        if (shieldDurationFrames > 0)
        {
            shieldDurationFrames--;
            if (shieldDurationFrames <= 0)
            {
                shieldWithDuration = 0;
            }
        }
    }
    #endregion
    
    #region DOT System Methods
    /// <summary>
    /// 도트 데미지 추가
    /// </summary>
    public void AddDotDamage(float damage, int duration, int interval, BaseHero owner, int id)
    {
        dotDamager.AddDamage(damage, duration, interval, owner, id);
    }
    
    /// <summary>
    /// 도트 힐 추가
    /// </summary>
    public void AddDotHeal(float heal, int duration, int interval, BaseHero owner)
    {
        dotHeal.AddHeal(heal, duration, interval, owner);
    }
    
    /// <summary>
    /// 도트 데미지 업데이트 (매 프레임 호출)
    /// </summary>
    protected virtual void UpdateDotDamage()
    {
        var list = dotDamager.List;
        for (int i = 0; i < list.Count; i++)
        {
            BaseDotDamageVO dvo = list[i];
            if (dvo.duration < 0) continue;
            
            // interval 프레임마다 데미지 적용
            if (dvo.duration % dvo.interval == 0)
            {
                // 도트 데미지 처리
                DoDotDamageInternal(dvo);
                
                if (!isAlive) return;
            }
        }
        
        // 시간이 지난 도트 데미지 제거
        dotDamager.AdvanceTime();
    }
    
    /// <summary>
    /// 도트 데미지 실제 적용
    /// </summary>
    protected virtual void DoDotDamageInternal(BaseDotDamageVO dvo)
    {
        if (!isAlive) return;
        
        // 무적 체크 (필요시 구현)
        // if (IsInvincible()) return;
        
        // DamageManager를 통한 도트 데미지 계산
        DamageVO vo = DamageManager.GetDotDamage(dvo.damage, dvo.owner, this);
        
        // 보호막 업데이트
        SetShieldWithDuration(vo.shieldWithDuration);
        SetShield(vo.shield);
        
        // 실제 데미지 적용
        if (vo.damage > 0)
        {
            float prevHealth = currentHealth;
            currentHealth -= vo.damage;
            healthBarScript.ChangeHp( currentHealth / maxHealth );
            
            // 타입 기반 통계 기록 (DOT 데미지)
            if (BattleStatisticsManager.Instance != null && dvo.owner != null)
            {
                BattleStatisticsManager.Instance.RecordDamage(
                    dvo.owner,
                    this,
                    vo.damage,
                    true,   // isDot = true
                    vo.isCritical,
                    dvo.id  // DOT ID를 스킬 ID로 사용
                );
            }
            
            // 데미지 이펙트 표시 (옵션)
            // DisplayHitEffect();
            
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                
                // 킬 기록
                if (BattleStatisticsManager.Instance != null && dvo.owner != null && prevHealth > 0)
                {
                    BattleStatisticsManager.Instance.RecordKill(dvo.owner, this);
                }
                
                GotoDieState();
            }

            framesSinceLastAttacked = 0;
        }
    }

    /// <summary>
    /// 도트 힐 업데이트 (매 프레임 호출)
    /// </summary>
    protected virtual void UpdateDotHeal()
    {
        if (!isAlive) return;
        
        var list = dotHeal.List;
        for (int i = 0; i < list.Count; i++)
        {
            BaseDotHealVO hvo = list[i];
            if (hvo.duration < 0) continue;
            
            // interval 프레임마다 힐 적용
            if (hvo.duration % hvo.interval == 0)
            {
                // 소수점 버림 (오차 방지)
                float dotHealAmount = Mathf.Floor(hvo.heal);
                if (dotHealAmount == 0) dotHealAmount = 1;
                
                // 도트 힐 처리
                Heal(dotHealAmount, hvo.owner, false);
                
                // 통계 추적 (향후 구현)
                // if (hvo.owner != null)
                // {
                //     hvo.owner.RecordHealingDone(dotHealAmount, this, true); // true = DOT
                // }
            }
        }
        
        // 시간이 지난 도트 힐 제거
        dotHeal.AdvanceTime();
    }

    protected List< ActiveBuffManager > buffManagerList = new List< ActiveBuffManager >();  
    protected List< ActiveBuffManager > activebuffManagerList = new List< ActiveBuffManager >();

    public virtual void Poison( float damage, int duration, int interval, BaseHero owner, int id ) {
        ShowCCEffect( EffectType.POISON, duration, 0, TargetHeight * 1.1f );
        AddDotDamage( damage, duration, interval, owner, id );

    }


    public virtual void DamageBuff1( int buffId, float value, int durationFrames, bool isOverride ) {
        ShowCCEffect( EffectType.BUFF_DAMAGE_UP, durationFrames, 0, TargetHeight * 1.1f );
        AddDamageBuff( buffId, value, durationFrames, isOverride );
    }

    protected virtual void ShowBuff( EffectType effectType, int duration, float x, float y ) {
        
    }

    protected virtual void ArrangeActiveBuff() {
        int activeBuffCount = 0;
        for( int i = 0; i < buffManagerList.Count; i ++ ) {
            ActiveBuffManager buffManager = buffManagerList[ i ];
            
            if( buffManager.GetActiveBuffCount() > 0 ) activeBuffCount += 1 ;
        }

    }
    
    
    /// <summary>
    /// 힐 처리
    /// </summary>
    public virtual void Heal(float amount, BaseHero healer, bool isDirectHeal = true)
    {
        if (!isAlive) return;
        
        float prevHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        float actualHealed = currentHealth - prevHealth;
        
        // 힐 이펙트 표시 (옵션)
        // DisplayHealEffect();
        
        // 타입 기반 통계 기록
        if (BattleStatisticsManager.Instance != null && healer != null && actualHealed > 0)
        {
            BattleStatisticsManager.Instance.RecordHealing(
                healer,
                this,
                actualHealed,
                !isDirectHeal  // isDot = !isDirectHeal
            );
        }
    }
    
    /// <summary>
    /// 특정 ID의 도트 데미지 제거
    /// </summary>
    public void RemoveDotDamage(int id)
    {
        dotDamager.RemoveDot(id);
    }
    
    /// <summary>
    /// 모든 DOT 효과 제거
    /// </summary>
    public void RemoveAllDots()
    {
        dotDamager.Reset();
        dotHeal.Reset();
    }
    #endregion
}