using UnityEngine;

// 간단한 이펙트 컴포넌트 - 스프라이트 애니메이션만 처리
public class SimpleEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Sprite[] sprites;
    private float currentFrame = 0f;
    private float animationSpeed = 0.5f;
    private bool loop = false;
    private int maxFrames = 60;
    private int frameCount = 0;
    private string textureName;
    
    // 직접 조작을 위한 프로퍼티 (로컬 좌표 사용 - Pixi.js처럼)
    public float x 
    { 
        get => transform.localPosition.x;
        set 
        {
            Vector3 pos = transform.localPosition;
            pos.x = value;
            transform.localPosition = pos;
        }
    }
    
    public float y 
    { 
        get => transform.localPosition.y;
        set 
        {
            Vector3 pos = transform.localPosition;
            pos.y = value;
            transform.localPosition = pos;
        }
    }
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.sortingOrder = 500;  // 이펙트 레이어 (영웅보다 위)
        spriteRenderer.sortingLayerName = "Default";  // 명시적 설정
    }
    
    void OnEnable()
    {
        if (FrameController.Instance != null)
        {
            FrameController.Add(OnFrame, this);
        }
    }
    
    void OnDisable()
    {
        if (FrameController.Instance != null)
        {
            FrameController.Remove(OnFrame, this);
        }
    }
    
    // 설정값 설정 (EffectFactory에서 호출)
    public void SetConfig(string texture, float speed, bool isLoop, int duration)
    {
        textureName = texture;
        animationSpeed = speed;
        loop = isLoop;
        maxFrames = duration;
    }
    
    // 이펙트 재생 
    public void Play(int duration = -1)
    {
        // 텍스처 로드
        sprites = TextureManager.GetSprites("atlases/Battle", textureName);
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"[SimpleEffect] Failed to load texture: {textureName}");
            Remove();
            return;
        }
        
        Debug.Log($"[SimpleEffect] Playing {textureName} with {sprites.Length} frames at pos ({transform.position.x}, {transform.position.y})");
        
        // duration이 제공되면 덮어쓰기
        if (duration > 0)
        {
            maxFrames = duration;
        }
        
        // 초기화
        currentFrame = 0f;
        frameCount = 0;
        spriteRenderer.sprite = sprites[0];
        
        gameObject.SetActive(true);
    }
    
    // 부모 설정
    public void SetParent(Transform parent)
    {
        if (parent != null)
        {
            transform.SetParent(parent);
            transform.localPosition = Vector3.zero;
        }
        else
        {
            transform.SetParent(EffectLayer.Instance.transform);
        }
    }
    
    void OnFrame()
    {
        if (sprites == null || sprites.Length == 0) return;
        
        // 프레임 카운트
        frameCount++;
        if (!loop && frameCount >= maxFrames)
        {
            Remove();
            return;
        }
        
        // 애니메이션 업데이트
        currentFrame += animationSpeed;
        if (currentFrame >= sprites.Length)
        {
            if (loop)
            {
                currentFrame = 0f;
            }
            else
            {
                Remove();
                return;
            }
        }
        
        spriteRenderer.sprite = sprites[Mathf.FloorToInt(currentFrame)];
        
        // 부모가 사라졌는지 체크 (영웅이 죽었을 때)
        if (transform.parent != null && transform.parent != EffectLayer.Instance.transform)
        {
            if (!transform.parent.gameObject.activeInHierarchy)
            {
                Remove();
            }
        }
    }
    
    public void Remove()
    {
        // 상태 초기화
        sprites = null;
        currentFrame = 0f;
        frameCount = 0;
        textureName = null;
        
        // 위치 초기화
        transform.SetParent(EffectLayer.Instance.transform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        
        // 비활성화 (OnDisable 호출됨)
        gameObject.SetActive(false);
        
        // 풀로 반환
        EffectFactory.ReturnEffect(this);
    }
}