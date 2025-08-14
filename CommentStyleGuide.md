# Unity 주석 스타일 가이드

## 언제 XML 주석을 써야 하나?

### 꼭 필요한 경우 (IntelliSense가 필요할 때)
```csharp
/// <summary>
/// 프레임 기반 콜백을 등록합니다
/// </summary>
/// <param name="callback">매 프레임 호출될 함수</param>
/// <param name="context">콜백의 소유자 객체</param>
public static void Add(Action callback, UnityEngine.Object context)
```

### 불필요한 경우 (간단한 한 줄 주석으로 충분)
```csharp
// 프레임 카운터 증가
frameCounter += animationSpeed;

// 공격 쿨다운 체크
if (framesSinceLastAttack < attackIntervalFrames) return;
```

## 권장 주석 스타일

### 1. 클래스 설명
```csharp
// AS3/PixiJS Ticker와 유사한 프레임 기반 업데이트 시스템
public class FrameController : MonoBehaviour
```

### 2. 섹션 구분
```csharp
// === 공개 API ===
// === 내부 메서드 ===
// === 이벤트 핸들러 ===
```

### 3. 필드/변수
```csharp
private float frameInterval = 1f / 60f;  // 60 FPS
private bool isRunning = true;           // 실행 상태
```

### 4. 복잡한 로직 설명
```csharp
// 중복 공격 방지: hasAttacked 플래그와 lastAttackFrame 체크
if (state == STATE_ATTACK && !hasAttacked && currentFrame != lastAttackFrame)
{
    AttackMain();
}
```

## 피해야 할 패턴

### ❌ 과도한 XML 주석
```csharp
/// <summary>
/// 속도를 가져옵니다
/// </summary>
/// <returns>현재 속도</returns>
public float GetSpeed() => speed;  // 너무 명확해서 주석 불필요
```

### ❌ 당연한 내용 반복
```csharp
// i를 1 증가시킴
i++;  // 불필요

// 체력이 0 이하면 죽음
if (health <= 0) Die();  // 코드만으로 충분히 명확
```

### ✅ 좋은 예시
```csharp
// 프레임 누적 방식으로 가변 프레임레이트 대응
accumulatedTime += Time.deltaTime * speed;
while (accumulatedTime >= frameInterval)
{
    ExecuteHandlers();
    accumulatedTime -= frameInterval;
}
```

## 요약
- **공개 API에만** XML 주석 사용 (IntelliSense)
- **내부 코드**는 간단한 한 줄 주석
- **왜(Why)**를 설명하고, **무엇(What)**은 코드로 표현
- 주석은 **최소한으로**, 코드를 **자명하게**