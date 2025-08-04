using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 모든 Hero가 상속받는 기본 클래스
/// AS3.0/PixiJS 스타일의 프레임 기반 애니메이션 시스템
/// </summary>
public abstract class BaseHero : MonoBehaviour
{
    #region Constants
    protected const float FRAME_RATE = 60f; // 60 FPS 기준
    
    // 상태 상수
    protected const int STATE_WAIT = 0;
    protected const int STATE_MOVE = 1;
    protected const int STATE_ATTACK = 2;
    protected const int STATE_SKILL = 3;
    protected const int STATE_DIE = 4;
    #endregion

    #region Inspector Fields
    [Header("Hero Configuration")]
    [SerializeField] protected HeroData heroData;  // ScriptableObject 데이터
    [SerializeField] protected int level = 1;      // 영웅 레벨
    
    [Header("Runtime Stats")]
    [SerializeField] protected float currentHealth;
    [SerializeField] protected float maxHealth;     // 레벨 적용된 최대 체력
    [SerializeField] protected float attackPower;   // 레벨 적용된 공격력
    [SerializeField] protected float defense;       // 레벨 적용된 방어력
    
    [Header("Team Settings")]
    [SerializeField] protected bool isPlayerTeam = true;  // true: 플레이어 팀, false: 적 팀
    #endregion

    #region Private Fields
    // Components
    protected SpriteRenderer spriteRenderer;
    
    // Static sprite cache - 같은 타입의 영웅은 스프라이트 공유
    private static Dictionary<string, Sprite[]> spriteCache = new Dictionary<string, Sprite[]>();
    
    // 이 영웅의 스프라이트 리스트
    protected Sprite[] spriteList;
    
    // Animation control
    protected int currentFrame = 0;
    protected float frameCounter = 0f;
    protected float animationSpeed = 1f; // 애니메이션 재생 속도 배수
    
    // Current animation state
    protected int animStartFrame;
    protected int animEndFrame;
    protected bool isLooping = true;
    protected int state = STATE_WAIT;  // 상태 변수
    
    // State flags
    protected bool isAlive = true;
    protected bool isMoving = false;
    protected bool isAttacking = false;
    protected bool health20Triggered = false;
    
    // Target
    protected Transform target;
    
    // Attack frame optimization (AS3.0 style)
    protected Dictionary<int, bool> attackFrameDict = new Dictionary<int, bool>();
    protected int lastAttackFrame = -1;  // 마지막으로 공격한 프레임 (중복 방지)
    
    // Attack interval (프레임 기반)
    protected int attackIntervalFrames = 60;  // 60프레임 = 1초 (60 FPS 기준)
    protected int framesSinceLastAttack = 0;  // 마지막 공격 후 경과한 프레임
    
    // AS3.0 style fields
    protected Vector2 targetPosition;         // 이동 목표 위치
    protected Vector2 defaultTargetPosition;  // 기본 목표 위치 (적이 없을 때 가야할 최종 목적지)
    protected bool hasAttacked = false;       // 현재 프레임에서 공격했는지 여부
    protected bool hasAttackStarted = false;  // 공격 시작 처리 여부
    protected bool hasGameStarted = false;    // 게임 시작 처리 여부
    
    // AS3.0 style battle lists - 모든 영웅이 공유
    protected static BaseHero[] friendList = null;  // 아군 목록
    protected static BaseHero[] enemyList = null;   // 적군 목록
    
    // AS3.0/PixiJS style class name
    protected string className;                      // 클래스 이름 (텍스처 이름으로 사용)
    protected string sheetName = "atlases/Battle";   // 기본 시트 이름 (실제 존재하는 경로)
    #endregion

    #region Unity Lifecycle
    protected virtual void Awake()
    {
        // Component setup
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // AS3.0/PixiJS style - 클래스 이름 저장
        className = GetType().Name;
        
        // HeroData 검증
        if (heroData == null)
        {
            Debug.LogWarning($"[BaseHero] HeroData is not assigned on {gameObject.name}, using class name for sprite loading");
            // heroData가 없어도 클래스 이름으로 스프라이트는 로드 가능
        }
        else
        {
            // 레벨에 따른 스탯 초기화
            InitializeStats();
        }
        
        // Hero specific initialization
        Initialize();
        
        // 텍스처 이름 초기화 (AS3.0/PixiJS 스타일)
        InitializeTextureName();
        
        // Load sprites (클래스 이름과 텍스처 이름이 설정된 후)
        LoadSprites();
        
        // Attack frame dictionary 초기화 (AS3.0 style optimization)
        InitializeAttackFrames();
    }

    protected virtual void OnEnable()
    {
        // FrameController에 등록
        if (FrameController.Instance != null)
        {
            FrameController.Instance.Add(Execute, this);
        }
    }

    protected virtual void OnDisable()
    {
        // FrameController에서 제거
        if (FrameController.Instance != null)
        {
            FrameController.Instance.Remove(Execute, this);
        }
    }
    #endregion

    #region Abstract Methods
    /// <summary>
    /// Hero별 초기화 - 각 영웅 클래스에서 구현
    /// </summary>
    protected abstract void Initialize();
    
    /// <summary>
    /// 텍스처 이름 초기화 (AS3.0/PixiJS 스타일) - 필요시 override
    /// </summary>
    protected virtual void InitializeTextureName()
    {
        // 기본적으로 클래스 이름을 텍스처 이름으로 사용
        // 필요하면 서브클래스에서 override하여 변경 가능
    }
    
    /// <summary>
    /// Hero별 로직 업데이트 - Execute()에서 매 프레임 호출 (레거시 - 삭제 예정)
    /// </summary>
    protected abstract void UpdateLogic();
    #endregion
    
    #region State Transition Methods (AS3.0 Style)
    /// <summary>
    /// 대기 상태로 전환
    /// </summary>
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
    
    /// <summary>
    /// 이동 상태로 전환
    /// </summary>
    protected virtual void GotoMoveState()
    {
        if (state == STATE_MOVE) return;
        SetState(STATE_MOVE);
    }
    
    /// <summary>
    /// 공격 상태로 전환
    /// </summary>
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
    
    /// <summary>
    /// 스킬 상태로 전환
    /// </summary>
    protected virtual void GotoSkillState(BaseHero targetHero)
    {
        if (framesSinceLastAttack < attackIntervalFrames) return;
        if (targetHero == null || !targetHero.IsAlive) return;
        
        target = targetHero.transform;
        SetState(STATE_SKILL, false);
        framesSinceLastAttack = 0;
    }
    
    /// <summary>
    /// 죽음 상태로 전환
    /// </summary>
    protected virtual void GotoDieState()
    {
        if (state == STATE_DIE) return;
        
        isAlive = false;
        target = null;
        SetState(STATE_DIE, false);
    }
    #endregion
    
    #region State Methods (AS3.0 Style)
    /// <summary>
    /// 대기 상태 처리
    /// </summary>
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
    
    /// <summary>
    /// 이동 상태 처리
    /// </summary>
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
        
        // 타겟 위치 설정
        if (target != null)
        {
            BaseHero targetHero = target.GetComponent<BaseHero>();
            if (targetHero != null)
            {
                // 타겟의 좌/우에 위치하도록 설정 (사거리의 90%)
                float offset = heroData.attackRange * 0.9f;
                if (transform.position.x <= target.position.x)
                    targetPosition = new Vector2(target.position.x - offset, target.position.y);
                else
                    targetPosition = new Vector2(target.position.x + offset, target.position.y);
            }
        }
        else
        {
            // 타겟이 없으면 기본 목적지로
            targetPosition = defaultTargetPosition;
        }
        
        // 목적지 도착 체크
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);
        
        if (target != null)
        {
            // 공격 범위 내에 있으면
            if (distanceToTarget < heroData.attackRange)
            {
                if (framesSinceLastAttack >= attackIntervalFrames)
                {
                    BaseHero targetHero = target.GetComponent<BaseHero>();
                    if (targetHero != null)
                    {
                        GotoAttackState(targetHero);
                    }
                }
                else
                {
                    GotoWaitState();
                }
                return;
            }
        }
        else
        {
            // 목적지 도착
            if (distanceToTarget < heroData.moveSpeed * Time.deltaTime)
            {
                transform.position = targetPosition;
                GotoWaitState();
                return;
            }
        }
        
        // 이동
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        transform.position += (Vector3)(direction * heroData.moveSpeed * Time.deltaTime);
        
        // 방향 업데이트
        UpdateFacing(direction.x);
    }
    
    /// <summary>
    /// 공격 상태 처리
    /// </summary>
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
    
    /// <summary>
    /// 스킬 상태 처리
    /// </summary>
    protected virtual void DoSkill()
    {
        // 스킬 애니메이션 처리는 UpdateFrame에서 수행
        // 서브클래스에서 구체적인 스킬 로직 구현
    }
    
    /// <summary>
    /// 죽음 상태 처리
    /// </summary>
    protected virtual void DoDie()
    {
        // 죽음 애니메이션이 끝나면 제거 (OnAnimationComplete -> OnDie에서 처리)
        // 추가 처리가 필요한 경우 서브클래스에서 override
    }
    #endregion

    #region Sprite Loading
    /// <summary>
    /// 스프라이트를 로드하고 캐싱
    /// </summary>
    protected virtual void LoadSprites()
    {
        // AS3.0/PixiJS 스타일 - heroData.heroClass 사용, 없으면 클래스 이름 사용
        string spriteName = (heroData != null && !string.IsNullOrEmpty(heroData.heroClass)) 
            ? heroData.heroClass 
            : className;
        
        // 이미 캐시에 있으면 재사용
        if (spriteCache.ContainsKey(spriteName))
        {
            spriteList = spriteCache[spriteName];
            Debug.Log($"[BaseHero] {spriteName} sprites loaded from cache");
            return;
        }
        
        // AS3.0/PixiJS 스타일 - 항상 기본 시트 사용 ("atlases/Battle")
        string sheet = sheetName;
        
        // TextureManager를 통해 로드
        spriteList = TextureManager.GetSprites(sheet, spriteName);
        
        if (spriteList == null || spriteList.Length == 0)
        {
            Debug.LogError($"[BaseHero] Failed to load sprites for {spriteName} from sheet {sheet}");
            return;
        }
        
        // 캐시에 저장
        spriteCache[spriteName] = spriteList;
        Debug.Log($"[BaseHero] {spriteName} loaded {spriteList.Length} sprites from {sheet}");
        
        // 첫 프레임 설정
        if (spriteList.Length > 0)
        {
            currentFrame = 0;
            spriteRenderer.sprite = spriteList[currentFrame];
        }
    }
    #endregion

    #region Frame Update
    /// <summary>
    /// FrameController에서 매 프레임 호출 (60fps) - AS3.0 스타일
    /// </summary>
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
    
    /// <summary>
    /// 게임 시작 시 한 번 호출
    /// </summary>
    protected virtual void OnGameStart()
    {
        Debug.Log($"[BaseHero] {HeroName} entered the battle!");
    }
    
    /// <summary>
    /// 프레임 전처리 - 쿨다운, DOT 등
    /// </summary>
    protected virtual void PreFrameUpdate()
    {
        // 공격 인터벌 카운트
        if (framesSinceLastAttack < int.MaxValue) // 오버플로우 방지
        {
            framesSinceLastAttack++;
        }
        
        // TODO: DOT 데미지 처리
        // TODO: DOT 힐 처리
        // TODO: CC 처리
    }

    /// <summary>
    /// 프레임 업데이트 로직
    /// </summary>
    protected virtual void UpdateFrame()
    {
        if (spriteList == null || spriteList.Length == 0) return;
        
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

    /// <summary>
    /// 현재 프레임의 스프라이트로 업데이트
    /// </summary>
    protected virtual void UpdateSprite()
    {
        if (currentFrame >= 0 && currentFrame < spriteList.Length)
        {
            spriteRenderer.sprite = spriteList[currentFrame];
        }
    }
    #endregion

    #region Animation Control
    /// <summary>
    /// AS3 스타일 프레임 이동
    /// </summary>
    public virtual void GotoAndPlay(int frame)
    {
        if (spriteList == null || spriteList.Length == 0) return;
        
        currentFrame = Mathf.Clamp(frame, 0, spriteList.Length - 1);
        frameCounter = 0f;
        UpdateSprite();
    }

    /// <summary>
    /// 상태로 애니메이션 재생
    /// </summary>
    public virtual void SetState(int newState, bool loop = true)
    {
        if (state == newState && isLooping == loop) return;
        
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
                animStartFrame = heroData.startWait;
                animEndFrame = heroData.endWait;
                break;
            case STATE_MOVE:
                animStartFrame = heroData.startMove;
                animEndFrame = heroData.endMove;
                break;
            case STATE_ATTACK:
                animStartFrame = heroData.startAttack;
                animEndFrame = heroData.endAttack;
                isAttacking = true;
                break;
            case STATE_SKILL:
                animStartFrame = heroData.startSkill;
                animEndFrame = heroData.endSkill;
                break;
            case STATE_DIE:
                animStartFrame = heroData.startDie;
                animEndFrame = heroData.endDie;
                isLooping = false;
                break;
            default:
                Debug.LogWarning($"[BaseHero] Unknown state: {state}");
                return;
        }
        
        currentFrame = animStartFrame;
        UpdateSprite();
    }

    /// <summary>
    /// 애니메이션 완료 시 호출
    /// </summary>
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
    /// <summary>
    /// 죽을 때 호출 - Override 가능
    /// </summary>
    protected virtual void OnDie()
    {
        isAlive = false;
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

    /// <summary>
    /// 체력이 20% 이하로 떨어질 때 호출 - Override 가능
    /// </summary>
    protected virtual void OnHealth20()
    {
        Debug.Log($"[BaseHero] {heroData.heroName} health below 20%");
        // 서브클래스에서 특수 스킬 발동 등 구현
    }

    /// <summary>
    /// 아군이 죽을 때 호출 - Override 가능
    /// </summary>
    protected virtual void OnFriendDie(BaseHero friend)
    {
        Debug.Log($"[BaseHero] {heroData.heroName} friend {friend.HeroName} died");
        // 서브클래스에서 버프 등 구현
    }

    /// <summary>
    /// 적을 죽일 때 호출 - Override 가능
    /// </summary>
    protected virtual void OnKillEnemy(BaseHero enemy)
    {
        Debug.Log($"[BaseHero] {heroData.heroName} killed {enemy.HeroName}");
        // 서브클래스에서 경험치 획득 등 구현
    }

    /// <summary>
    /// 공격 메인 함수 - AS3.0 스타일로 특정 프레임에서 호출됨
    /// </summary>
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
    
    /// <summary>
    /// 근거리 공격 처리 - 서브클래스에서 재정의 가능
    /// </summary>
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
            
            // 적을 죽였는지 확인
            if (!targetHero.IsAlive)
            {
                OnKillEnemy(targetHero);
            }
        }
    }
    
    /// <summary>
    /// 원거리 공격 처리 - 서브클래스에서 재정의 가능
    /// </summary>
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
    
    /// <summary>
    /// 공격 애니메이션 완료 시 호출
    /// </summary>
    protected virtual void OnAttackComplete()
    {
        // 공격 애니메이션이 끝났을 때의 처리
        // AttackMain과는 별개로, 애니메이션 종료 시점 처리용
    }

    /// <summary>
    /// 스킬 애니메이션 완료 시 호출
    /// </summary>
    protected virtual void OnSkillComplete()
    {
        // 스킬 효과 처리 등
    }
    #endregion

    #region Initialization
    /// <summary>
    /// 레벨에 따른 스탯 초기화
    /// </summary>
    protected virtual void InitializeStats()
    {
        maxHealth = heroData.GetMaxHealth(level);
        currentHealth = maxHealth;
        attackPower = heroData.GetAttackPower(level);
        defense = heroData.GetDefense(level);
        
        // 공격 인터벌 설정
        attackIntervalFrames = heroData.attackInterval;
    }
    
    /// <summary>
    /// 공격 프레임 Dictionary 초기화 (AS3.0 style optimization)
    /// </summary>
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
                    Debug.LogWarning($"[BaseHero] Attack trigger frame {frame} is outside attack animation range ({heroData.startAttack}-{heroData.endAttack})");
                }
            }
        }
    }
    #endregion

    #region Combat & Health
    /// <summary>
    /// 데미지 받기
    /// </summary>
    public virtual void TakeDamage(float damage)
    {
        if (!isAlive) return;
        
        float actualDamage = Mathf.Max(0, damage - defense);
        currentHealth -= actualDamage;
        
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            GotoDieState();
        }
    }

    /// <summary>
    /// 체력 체크
    /// </summary>
    protected virtual void CheckHealth()
    {
        if (!health20Triggered && currentHealth <= maxHealth * 0.2f)
        {
            health20Triggered = true;
            OnHealth20();
        }
    }

    /// <summary>
    /// 타겟 설정
    /// </summary>
    public virtual void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    #endregion

    #region Movement
    /// <summary>
    /// 타겟으로 이동 (좌/우 방향만 처리)
    /// </summary>
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
        transform.position += direction * heroData.moveSpeed * Time.deltaTime;
        
        // 좌/우 방향 업데이트 (x축만 체크)
        UpdateFacing(target.position.x - transform.position.x);
    }
    
    /// <summary>
    /// 좌/우 방향 업데이트 (기본: 오른쪽)
    /// </summary>
    protected virtual void UpdateFacing(float xDifference)
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
    
    /// <summary>
    /// 가장 가까운 적 찾기 (AS3.0 style)
    /// </summary>
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
    
    /// <summary>
    /// 가장 가까운 아군 찾기 (AS3.0 style)
    /// </summary>
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
    
    
    /// <summary>
    /// 이동 시작
    /// </summary>
    public virtual void StartMove()
    {
        if (!isAlive) return;
        
        isMoving = true;
        SetState(STATE_MOVE);
    }

    /// <summary>
    /// 이동 정지
    /// </summary>
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
    /// <summary>
    /// 오브젝트 풀로 반환
    /// </summary>
    protected virtual void ReturnToPool()
    {
        // 오브젝트 풀 시스템이 있다면 여기서 처리
        gameObject.SetActive(false);
        // Destroy(gameObject);
    }

    /// <summary>
    /// 영웅 초기화 (재사용 시)
    /// </summary>
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
    /// 기본 목적지 설정 (적이 없을 때 가야할 위치)
    /// </summary>
    public void SetDefaultTargetPosition(Vector2 position)
    {
        defaultTargetPosition = position;
    }
    
    public bool IsPlayerTeam => isPlayerTeam;
    #endregion
    
    #region Static Battle Management (AS3.0 Style)
    /// <summary>
    /// 전투 목록 설정 (BattleController에서 호출)
    /// </summary>
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
    
    /// <summary>
    /// 각 영웅에게 팀 리스트 설정
    /// </summary>
    protected virtual void SetTeamLists(BaseHero[] friends, BaseHero[] enemies)
    {
        friendList = friends;
        enemyList = enemies;
    }
    
    /// <summary>
    /// 전투 리스트 초기화
    /// </summary>
    public static void ClearBattleLists()
    {
        friendList = null;
        enemyList = null;
    }
    #endregion
}