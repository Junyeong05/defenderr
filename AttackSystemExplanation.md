# MergeDefender 공격 시스템 설명

## 문제점 분석

### 원인 1: 전투 리스트 미설정
```csharp
// 문제: SetBattleLists()를 호출하지 않음
// 결과: friendList와 enemyList가 null
// 영향: FindNearestEnemy()가 항상 null 반환
```

### 원인 2: 같은 팀 설정
```csharp
// 기본값: isPlayerTeam = true (모두 아군)
// 결과: 서로를 적으로 인식하지 못함
```

### 원인 3: 타겟 설정 순서
```csharp
// 잘못된 순서:
footMan.SetState(STATE_MOVE);  // 먼저 이동 상태로
footMan.SetTarget(elfArcher);  // 나중에 타겟 설정
// STATE_MOVE는 target이 null이면 defaultTargetPosition으로 이동
```

## 해결 방법

### 1. 전투 리스트 설정
```csharp
BaseHero[] playerTeam = new BaseHero[] { footMan };
BaseHero[] enemyTeam = new BaseHero[] { elfArcher };
BaseHero.SetBattleLists(playerTeam, enemyTeam);
```

### 2. 타겟 설정 후 상태 변경
```csharp
footMan.SetTarget(elfArcher);      // 먼저 타겟 설정
footMan.SetState(STATE_MOVE);      // 그 다음 이동 상태로
```

## 공격 시스템 작동 방식

### 상태 흐름
```
1. STATE_WAIT (대기)
   ↓ target 발견
2. STATE_MOVE (이동)
   ↓ 사거리 내 도달
3. STATE_ATTACK (공격)
   ↓ 애니메이션 완료
4. STATE_WAIT (대기)
```

### DoMove() 로직
```csharp
protected virtual void DoMove()
{
    // 1. 타겟 유효성 체크
    if (target == null || !target.GetComponent<BaseHero>().IsAlive)
        target = FindNearestEnemy();
    
    // 2. 타겟이 있으면 타겟 위치로
    if (target != null)
        targetPosition = target.position ± attackRange;
    else
        targetPosition = defaultTargetPosition;  // 타겟 없으면 기본 위치로
    
    // 3. 사거리 체크
    if (distanceToTarget < attackRange)
        GotoAttackState(targetHero);  // 공격!
}
```

### DoWait() 로직
```csharp
protected virtual void DoWait()
{
    // 1. 타겟이 없으면 찾기
    if (target == null)
        target = FindNearestEnemy();
    
    // 2. 타겟과의 거리 체크
    if (distance <= attackRange)
        GotoAttackState(targetHero);  // 사거리 내면 공격
    else
        GotoMoveState();  // 사거리 밖이면 이동
}
```

## 테스트 방법

### 1. 직접 타겟팅 테스트
```csharp
// 특정 적을 타겟으로 지정
footMan.SetTarget(elfArcher);
footMan.SetState(BaseHero.STATE_MOVE);
```

### 2. 자동 타겟팅 테스트
```csharp
// 전투 리스트만 설정하면 자동으로 가장 가까운 적 공격
BaseHero.SetBattleLists(playerTeam, enemyTeam);
// FindNearestEnemy()가 자동으로 작동
```

### 3. 팀 전투 테스트
```csharp
// 여러 영웅 팀 전투
BaseHero[] playerTeam = new BaseHero[] { footMan1, footMan2 };
BaseHero[] enemyTeam = new BaseHero[] { elfArcher1, elfArcher2 };
BaseHero.SetBattleLists(playerTeam, enemyTeam);
```

## 주요 메서드 설명

### SetTarget()
- `SetTarget(Transform)`: Transform을 직접 전달
- `SetTarget(BaseHero)`: BaseHero를 전달 (자동으로 transform 추출)

### SetBattleLists()
- 정적 메서드로 모든 영웅의 아군/적군 리스트 설정
- 각 영웅이 FindNearestEnemy()로 적을 찾을 수 있게 함

### FindNearestEnemy()
- enemyList에서 가장 가까운 살아있는 적 반환
- SetBattleLists()가 호출되지 않으면 작동 안함

## 디버깅 팁

### 1. 타겟 확인
```csharp
Debug.Log($"Current target: {footMan.Target}");
Debug.Log($"Current state: {footMan.State}");
```

### 2. 전투 리스트 확인
```csharp
// BaseHero.cs에 디버그 메서드 추가
public void DebugBattleLists()
{
    Debug.Log($"Friend list: {friendList?.Length ?? 0} heroes");
    Debug.Log($"Enemy list: {enemyList?.Length ?? 0} heroes");
}
```

### 3. 거리 체크
```csharp
if (target != null)
{
    float distance = Vector2.Distance(transform.position, target.position);
    Debug.Log($"Distance to target: {distance}, Attack range: {heroData.attackRange}");
}
```