# BattleStatisticsManager 통합 가이드

## 개선된 통계 시스템 개요

BattleStatisticsManager는 동적 팀 할당을 지원하는 개선된 전투 통계 시스템입니다.

## 주요 개선 사항

### 1. 동적 팀 할당
- GameObject 태그에 의존하지 않음
- `SetTeams()` 메서드를 통한 명시적 팀 설정
- 영웅이 아군/적군 어느 쪽이든 될 수 있도록 유연한 구조

### 2. 사용법

```csharp
// BattleController에서 팀 설정
BattleStatisticsManager.Instance.SetTeams(playerUnits, enemyUnits);

// 전투 시작
BattleStatisticsManager.Instance.StartBattle();

// 전투 종료 및 요약
BattleStatisticsManager.Instance.EndBattle();
```

## 통합 완료 사항

### BattleController.cs
- ✅ `SetupBattleLists()`에서 `SetTeams()` 호출 추가
- ✅ `StartBattle()`에서 통계 시스템 시작
- ✅ `StopBattle()`에서 통계 시스템 종료 및 요약

### GameMain.cs  
- ✅ BattleStatisticsManager 초기화
- ✅ BattleStatisticsUI 컴포넌트 추가

### BaseHero.cs
- ✅ TypeBasedStatisticsManager → BattleStatisticsManager 변경
- ✅ 데미지/힐/킬 기록 연동

## UI 사용법

### 키보드 단축키
- **Tab**: 통계 UI 표시/숨기기
- **R**: 통계 리셋
- **P**: 전투 요약 출력 (콘솔)
- **C**: CSV 파일로 내보내기

## 시스템 흐름

1. **전투 초기화 시**
   - BattleController가 아군/적군 유닛 생성
   - SetupBattleLists()에서 SetTeams() 호출
   - 팀 정보가 BattleStatisticsManager에 저장

2. **전투 시작 시**
   - BattleController.StartBattle() 호출
   - BattleStatisticsManager.StartBattle() 자동 호출
   - 등록된 팀의 영웅들이 통계 시스템에 등록

3. **전투 중**
   - BaseHero에서 데미지/힐/킬 발생 시 자동 기록
   - heroData의 kindNum + 팀 정보로 통계 집계

4. **전투 종료 시**
   - BattleController.StopBattle() 호출
   - BattleStatisticsManager.EndBattle() 자동 호출
   - 전투 요약 콘솔 출력

## 문제 해결

### 통계가 나타나지 않는 경우
1. HeroData의 kindNum이 설정되어 있는지 확인
2. BattleController가 SetTeams()를 호출하는지 확인
3. 콘솔에서 다음 로그 확인:
   - `[BattleController] Teams set in BattleStatisticsManager`
   - `[BattleStatistics] Teams set - Allies: X, Enemies: Y`

### CSV 내보내기 위치
- Windows: `%userprofile%\AppData\LocalLow\{CompanyName}\{ProductName}\`
- Mac: `~/Library/Application Support/{CompanyName}/{ProductName}/`

## 클래스 구조

```
BattleStatisticsManager (싱글톤)
├── SetTeams(allyHeroes, enemyHeroes) - 팀 설정
├── StartBattle() - 전투 시작
├── EndBattle() - 전투 종료
├── RecordDamage() - 데미지 기록
├── RecordHealing() - 힐 기록
├── RecordKill() - 킬 기록
└── PrintBattleSummary() - 요약 출력

HeroTypeStatistics
├── kindNum - 영웅 타입 번호
├── team - "ally" 또는 "enemy"
├── totalDamageDealt - 총 입힌 데미지
├── totalDamageTaken - 총 받은 데미지
├── kills - 킬 수
└── totalDeathCount - 사망 수
```

## 향후 개선 사항

- [ ] 실시간 그래프 표시
- [ ] 스킬별 데미지 통계
- [ ] 버프/디버프 통계
- [ ] 전투 리플레이 기능