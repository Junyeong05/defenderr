# BattleStatisticsManager 문제 해결 가이드

## 일반적인 문제와 해결법

### 1. 아군/적군 통계가 표시되지 않는 경우

**증상**: UI에 한쪽 팀만 표시되거나 일부 영웅 타입이 누락됨

**원인**:
- HeroData의 kindNum이 설정되지 않음 (0으로 되어 있음)
- heroData가 null이거나 제대로 초기화되지 않음

**해결법**:
1. Unity Editor에서 HeroData ScriptableObject 확인
   - Resources/HeroData/ 폴더의 각 HeroData 파일 선택
   - Inspector에서 kindNum 필드가 0이 아닌 값으로 설정되어 있는지 확인
   - ElfArcher는 2, FootMan은 1 등 고유한 번호 설정

2. 임시 해결책 (현재 적용됨)
   - BattleStatisticsManager가 className을 기반으로 임시 kindNum 할당
   - "Elf" 또는 "Archer" 포함 → kindNum = 2
   - "Foot" 또는 "Man" 포함 → kindNum = 1

### 2. UI 정렬이 이상한 경우

**증상**: 텍스트가 제대로 정렬되지 않음

**해결법**:
- 현재 수정됨: 일관된 들여쓰기 적용
  - 섹션 헤더: 들여쓰기 없음
  - 영웅 이름: 2칸 들여쓰기
  - 상세 정보: 4칸 들여쓰기

### 3. 통계가 전혀 기록되지 않는 경우

**확인 사항**:
1. Console 창에서 다음 로그 확인:
   ```
   [BattleController] Teams set in BattleStatisticsManager
   [BattleStatistics] New type registered: {영웅이름} ({팀}) - Type #{번호}
   [BattleStatistics] Hero spawned: {영웅이름} ({팀}) Instance: {ID}
   ```

2. kindNum=0 경고 메시지 확인:
   ```
   [BattleStatistics] Hero {이름} has kindNum=0
   [BattleStatistics] Assigned temporary kindNum={번호} for {이름}
   ```

### 4. 팀 구분 방식에 대한 고려사항

**현재 방식**:
- `{kindNum}_{team}` 형식의 키 사용 (예: "1_ally", "2_enemy")
- 같은 타입의 영웅이라도 팀별로 별도 통계 관리

**장점**:
- 팀별 성능 비교 가능
- 아군/적군 매치업 분석 용이

**단점**:
- 같은 영웅이 양쪽 팀에 있을 때 별도로 관리됨

**대안**:
- kindNum만으로 통계 관리하고 팀은 태그로만 구분
- 필요시 추가 개발 가능

## 디버그 모드 활성화

BattleStatisticsManager의 RegisterHero 메서드에 상세한 디버그 로그가 추가되어 있음:
- 영웅 등록 시 kindNum, className, heroName, team 정보 출력
- kindNum이 0인 경우 경고 및 임시 할당 정보 출력
- 각 영웅 스폰 시 인스턴스 ID 출력

## 권장 설정

1. **HeroData 설정**:
   - FootMan1: kindNum = 1
   - ElfArcher1: kindNum = 2
   - 이후 추가되는 영웅들도 고유 번호 할당

2. **BattleController 설정**:
   - playerUnitCount = 5
   - enemyUnitCount = 5

3. **UI 설정**:
   - Tab 키로 통계 UI 토글
   - R 키로 통계 리셋
   - P 키로 콘솔에 요약 출력
   - C 키로 CSV 내보내기