# Unity 무기 시스템 사용 가이드

## 시스템 구조

### 1. 핵심 컴포넌트
- **WeaponType**: 무기 타입 열거형 (Projectile, Beam, Custom)
- **WeaponData**: ScriptableObject로 무기 설정 데이터
- **BaseWeapon**: 모든 무기의 기본 클래스
- **WeaponFactory**: 무기 생성 및 오브젝트 풀링 관리
- **WeaponCatalog**: 모든 무기 데이터 관리

## 사용 방법

### 1단계: WeaponData 생성
1. Project 창에서 우클릭
2. Create > Battle > WeaponData 선택
3. 생성된 ScriptableObject 설정:
   ```
   - weaponName: "Arrow"
   - weaponClass: "Arrow" (코드에서 참조할 이름)
   - weaponType: Projectile
   - weaponPrefab: Arrow 프리팹 연결
   - initialSpeed: 5
   - damageMultiplier: 1
   - hitRadius: 10
   ```

### 2단계: 무기 프리팹 생성
1. 빈 GameObject 생성
2. BaseWeapon 또는 커스텀 무기 스크립트 추가 (예: Arrow.cs)
3. SpriteRenderer 컴포넌트 추가
4. 프리팹으로 저장

### 3단계: WeaponCatalog 설정
1. Create > Battle > WeaponCatalog 생성
2. 생성한 WeaponData들을 리스트에 추가

### 4단계: HeroData에 무기 설정
1. 원거리 영웅의 HeroData 열기
2. isRanged = true 설정
3. weaponClass = "Arrow" (사용할 무기 클래스명)

### 5단계: 씬 설정
1. WeaponFactory 오브젝트 생성
2. WeaponCatalog 연결
3. BattleController 시작 시 WeaponFactory 초기화

## 무기 타입별 구현

### Projectile (투사체)
```csharp
public class Arrow : BaseWeapon
{
    protected override void InitProjectile()
    {
        base.InitProjectile();
        // 화살 특유 설정
    }
    
    protected override void UpdateProjectile()
    {
        base.UpdateProjectile();
        // 중력, 회전 등 추가
    }
}
```

### Beam (빔)
```csharp
public class Lightning : BaseWeapon
{
    protected override void InitBeam()
    {
        // 즉시 타겟에 연결
    }
    
    protected override void UpdateBeam()
    {
        // 빔 유지 로직
    }
}
```

### Custom (커스텀)
```csharp
public class Boomerang : BaseWeapon
{
    protected override void InitCustom()
    {
        // 부메랑 초기화
    }
    
    protected override void UpdateCustom()
    {
        // 돌아오는 로직
    }
}
```

## 프리팹 구조 예시

### 기본 Arrow 프리팹
```
Arrow (GameObject)
├── BaseWeapon 또는 Arrow 컴포넌트
├── SpriteRenderer
└── Collider2D (옵션)
```

### 이펙트가 있는 FireArrow 프리팹
```
FireArrow (GameObject)
├── Arrow 컴포넌트
├── SpriteRenderer
├── ParticleSystem (화염 효과)
└── TrailRenderer (궤적)
```

## 성능 최적화

### Lazy Loading 오브젝트 풀링
- **필요할 때만 생성**: 첫 요청 시 자동 생성
- **메모리 효율적**: 사용하지 않는 무기는 생성하지 않음
- maxPoolSize: 각 무기 타입별 최대 풀 크기 (기본 100)

### 특수 상황 대비 (선택사항)
```csharp
// 보스전 등 대량 사용이 예상되는 경우만
// 최소 풀 크기 보장 (일반적으로 불필요)
WeaponFactory.Instance.EnsureMinimumPool("Arrow", 10);
```

### Lazy Loading 동작 방식
1. 첫 무기 요청 시 → 새로 생성
2. 무기 반환 시 → 풀에 보관
3. 재요청 시 → 풀에서 재사용
4. 풀이 가득 찬 경우 → 초과분 자동 파괴

## 확장 예시

### 관통 화살
```csharp
WeaponData:
- penetration: 2  // 2번 관통
```

### 폭발 화살
```csharp
public class ExplodingArrow : Arrow
{
    [SerializeField] private float splashRadius = 100f;
    [SerializeField] private float splashDamagePercent = 0.5f;
    
    protected override void OnHitTarget(BaseHero target)
    {
        base.OnHitTarget(target);
        
        // owner의 enemyList 활용 (컨트롤러 독립적)
        if (owner == null) return;
        
        // owner가 보는 적군 목록에서 범위 내 적 검색
        BaseHero[] enemies = owner.GetEnemyList();  // BaseHero의 enemyList 참조
        
        if (enemies == null) return;
        
        foreach(var enemy in enemies)
        {
            if (enemy != null && enemy.IsAlive && enemy != target)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance <= splashRadius)
                {
                    // 거리에 따른 스플래시 데미지 적용
                    float damageRatio = 1f - (distance / splashRadius);
                    float splashDamage = damage * splashDamagePercent * damageRatio;
                    enemy.TakeDamage(splashDamage);
                }
            }
        }
    }
}
```

## 주의사항

1. **WeaponData의 weaponPrefab 필수**: 프리팹이 없으면 생성 실패
2. **weaponClass 중복 금지**: 각 무기는 고유한 클래스명 필요
3. **FrameController 의존성**: BaseWeapon은 FrameController 필요
4. **메모리 관리**: 사용 완료된 무기는 반드시 Remove() 호출

## 디버깅

```csharp
// 풀 상태 확인
WeaponFactory.Instance.PrintPoolStatus();

// 카탈로그 유효성 검사
weaponCatalog.ValidateCatalog();
```