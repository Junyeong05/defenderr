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
    protected int attackIntervalFrames = 60;  // 1초 (60 FPS)
    protected int framesSinceLastAttack = 0;
    
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

    protected float orgSzie = 1.3f;
    
    // Hit reaction variables
    protected bool isHitReacting = true;
    protected int hitReactionFrame = 0;  // 현재 반응 프레임
    protected int hitReactionDurationFrames = 5; // 12프레임 동안 반응 (60fps 기준 0.2초)
    protected float hitReactionAngle = 23f; // 3도 회전
    protected Quaternion originalRotation;
    
    // Knockback variables
    protected float knockbackDistance = 0f; // 2픽셀 넉백
    
    private HeroFactory factory;
    protected string className;
    protected string sheetName = "atlases/Battle";
    private static Dictionary<string, TexturePackerFrameInfo> frameInfoCache = new Dictionary<string, TexturePackerFrameInfo>();
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
            
            // BattleController에서 Execute() 호출하도록 변경
        }
        else
        {
            Debug.LogError("SetData called with null HeroData!");
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
            Debug.LogWarning($"[BaseHero] No factory set for {className}, just deactivating");
            gameObject.SetActive(false);
        }
    }
    
    public virtual void ReturnToPool()
    {
        ResetState();
        hasData = false;
        
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
        // 공격 간격 체크
        if (framesSinceLastAttack >= attackIntervalFrames)
        {
            // 타겟이 없으면 찾기
            if (target == null)
            {
                target = FindNearestEnemy();
            }
            
            // 타겟이 있으면
            if (target != null)
            {
                BaseHero targetHero = target.GetComponent<BaseHero>();
                if (targetHero != null && targetHero.IsAlive)
                {
                    float distance = Vector2.Distance(transform.position, target.position);
                    
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
                else
                {
                    // 타겟이 죽었으면 null로
                    target = null;
                }
            }
        }
    }
    
    protected virtual void DoMove()
    {
        if (!isAlive) return;
        
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
        transform.position += (Vector3)(moveDirection * heroData.moveSpeed);
        
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
        }
        
        // 공격 애니메이션이 끝나면 자동으로 대기 상태로 전환됨 (OnAnimationComplete에서 처리)
    }
    
    protected virtual void DoSkill()
    {
        // 스킬 애니메이션 처리는 UpdateFrame에서 수행
        // 서브클래스에서 구체적인 스킬 로직 구현
    }
    
    protected virtual void DoDie()
    {
        // 죽음 애니메이션이 끝나면 제거 (OnAnimationComplete -> OnDie에서 처리)
        // 추가 처리가 필요한 경우 서브클래스에서 override
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
            Debug.LogError($"[BaseHero] Failed to load sprites for {spriteName} from sheet {sheetName}");
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
            healthBar.transform.localPosition = new Vector3(0, 1.5f, 0); // 머리 위
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
    
    public virtual GameObject AddEffect(GameObject effectPrefab, float duration = 0)
    {
        if (effectPrefab == null) return null;
        
        GameObject effect = Instantiate(effectPrefab, transform);
        effect.transform.localPosition = Vector3.zero;
        
        if (duration > 0)
        {
            Destroy(effect, duration);
        }
        
        return effect;
    }
    
    public virtual GameObject AddEffectAtPosition(GameObject effectPrefab, Vector3 worldPosition, float duration = 0)
    {
        if (effectPrefab == null) return null;
        
        GameObject effect = Instantiate(effectPrefab, transform);
        effect.transform.position = worldPosition;
        
        if (duration > 0)
        {
            Destroy(effect, duration);
        }
        
        return effect;
    }
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
        
        // 프레임 전처리
        PreFrameUpdate();
        
        // 상태에 따른 처리
        switch (state)
        {
            case STATE_WAIT:
                DoWait();
                break;
                
            case STATE_MOVE:
                DoMove();
                break;
                
            case STATE_ATTACK:
                DoAttack();
                break;
                
            case STATE_SKILL:
                DoSkill();
                break;
                
            case STATE_DIE:
                DoDie();
                break;
        }
        
        // 프레임 업데이트
        UpdateFrame();
        
        // 체력 체크
        CheckHealth();
    }
    
    protected virtual void OnGameStart()
    {
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
        
        // TODO: DOT 데미지 처리
        // TODO: DOT 힐 처리
        // TODO: CC 처리
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

    protected virtual void UpdateFrame()
    {
        if (spriteList == null || spriteList.Length == 0) 
        {
            return;
        }
        
        // 프레임 카운터 증가
        frameCounter += animationSpeed;
        
        
        // 1프레임 이상 지났으면 프레임 진행 (animationSpeed가 1보다 클 수 있음)
        while (frameCounter >= 1f)
        {
            frameCounter -= 1f;
            
            // 다음 프레임으로
            currentFrame++;
            
            // 공격 상태일 때 특정 프레임에서 공격 실행 (AS3.0 스타일 - Dictionary 최적화)
            // 중복 공격 방지: hasAttacked 플래그와 lastAttackFrame 체크
            if (state == STATE_ATTACK && !hasAttacked && attackFrameDict.ContainsKey(currentFrame) && currentFrame != lastAttackFrame)
            {
                AttackMain();
                hasAttacked = true;
                lastAttackFrame = currentFrame;
            }
            
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
                    break; // 애니메이션이 끝났으므로 while 루프 종료
                }
            }
        }
        
        // 스프라이트 업데이트 (while 루프 밖에서 한 번만)
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
        frameCounter = 0f;
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
        frameCounter = 0f;
        
        
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
                Debug.LogWarning($"[BaseHero] Unknown state: {state}");
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
        if (!isAlive || isDying) return;
        
        isAlive = false;
        isDying = true;
        Debug.Log($"[BaseHero] {heroData.heroName} died");
        
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
        Debug.Log($"[BaseHero] {heroData.heroName} health below 20%");
        // 서브클래스에서 특수 스킬 발동 등 구현
    }

    protected virtual void OnFriendDie(BaseHero friend)
    {
        Debug.Log($"[BaseHero] {heroData.heroName} friend {friend.HeroName} died");
        // 서브클래스에서 버프 등 구현
    }

    protected virtual void OnKillEnemy(BaseHero enemy)
    {
        Debug.Log($"[BaseHero] {heroData.heroName} killed {enemy.HeroName}");
        // 서브클래스에서 경험치 획득 등 구현
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
    }
    
    protected virtual void DoMeleeAttack()
    {
        if (target == null) return;
        
        BaseHero targetHero = target.GetComponent<BaseHero>();
        if (targetHero != null)
        {
            // 크리티컬 판정
            bool isCritical = UnityEngine.Random.Range(0f, 100f) < heroData.criticalChance;
            float damage = attackPower;
            
            if (isCritical)
            {
                damage *= heroData.criticalMultiplier;
                Debug.Log($"[{HeroName}] Critical melee hit!");
            }
            
            targetHero.TakeDamage(damage);
            
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
    
    // 넉백 적용
    protected virtual void ApplyKnockback(BaseHero targetHero)
    {
        if (targetHero == null) return;
        
        // 공격자에서 타겟으로의 방향 계산
        Vector2 knockbackDirection = (targetHero.transform.position - transform.position).normalized;
        
        // 타겟을 뒤로 밀기
        targetHero.transform.position += (Vector3)(knockbackDirection * knockbackDistance);
    }
    
    protected virtual void DoRangeAttack()
    {
        if (target == null) return;
        
        // 기본 원거리 공격은 즉시 데미지 (투사체 시스템은 서브클래스에서 구현)
        BaseHero targetHero = target.GetComponent<BaseHero>();
        if (targetHero != null)
        {
            // 크리티컬 판정
            bool isCritical = UnityEngine.Random.Range(0f, 100f) < heroData.criticalChance;
            float damage = attackPower;
            
            if (isCritical)
            {
                damage *= heroData.criticalMultiplier;
                Debug.Log($"[{HeroName}] Critical range hit!");
            }
            
            targetHero.TakeDamage(damage);
            
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
    protected virtual void InitializeStats()
    {
        maxHealth = heroData.GetMaxHealth(level);
        currentHealth = maxHealth;
        attackPower = heroData.GetAttackPower(level);
        defense = heroData.GetDefense(level);
        
        // 공격 인터벌 설정
        attackIntervalFrames = heroData.attackInterval;
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
        
        currentHealth = maxHealth;
        isAlive = true;
        isMoving = false;
        isAttacking = false;
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
    public float HealthPercent => currentHealth / maxHealth;
    public Transform Target => target;
    public int State => state;
    public HeroData Data => heroData;
    public int Level => level;
    
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
}