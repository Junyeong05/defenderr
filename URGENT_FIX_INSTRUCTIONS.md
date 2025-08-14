# 🚨 긴급 해결 방법 - Unity 실행 오류

## 문제: Screen position out of view frustum 오류로 실행 불가

### 즉시 해결 방법 (순서대로 시도)

## 방법 1: EmergencyFix 스크립트 사용 ✅
1. **Unity 에디터에서 main.unity 씬 열기**
2. **Hierarchy에서 빈 GameObject 생성** (GameObject → Create Empty)
3. **EmergencyFix.cs 스크립트 추가**
4. **Play 버튼 다시 시도**

## 방법 2: Unity 에디터 설정 초기화 🔧
1. **Unity 완전 종료**
2. **터미널에서 실행:**
```bash
cd /Users/smy/Unity/MergeDefender
rm -rf Library
rm -rf Temp
rm -rf obj
rm UserSettings/Layouts/default-2022.3.51f1.dwlt
```
3. **Unity 재시작** (Library 재생성에 시간 소요)

## 방법 3: Safe Mode로 Scene 정리 🛡️
1. **Unity를 Safe Mode로 시작**
   - Unity Hub → 프로젝트 옆 ⋮ 클릭 → "Open in Safe Mode"
2. **main.unity 씬 열기**
3. **Hierarchy에서 삭제:**
   - EventSystem (중복된 것들)
   - Canvas (불필요한 UI)
   - 이름에 "Touch", "Mobile", "Joystick" 포함된 오브젝트
4. **Main Camera 확인:**
   - Position: (0, 0, -10)
   - Orthographic: ✅ 체크
   - Orthographic Size: 180
5. **저장 후 Normal Mode로 재시작**

## 방법 4: 새 Scene으로 테스트 🆕
1. **File → New Scene**
2. **Basic 2D 선택**
3. **필요한 GameObject만 복사:**
   - HeroFactory
   - FrameController
   - 영웅 Prefab들
4. **Scene1.cs 스크립트 추가**
5. **새 씬 저장 후 테스트**

## 방법 5: Unity 프로젝트 설정 리셋 ⚙️
1. **Edit → Project Settings**
2. **Input Manager → Reset**
3. **Player → Resolution and Presentation:**
   - Run In Background: ✅ 체크 해제
   - Default Is Native Resolution: ✅ 체크
4. **Graphics → Reset**

## 임시 회피 방법 (테스트용) 🏃
1. **Game View 탭 클릭하여 포커스**
2. **Game View 크기를 작게 조정**
3. **Maximize on Play 해제**
4. **Free Aspect → 16:9 고정**

## 근본 원인
- Unity의 SendMouseEvents 시스템이 (0,0) 좌표를 Ray로 변환하려다 실패
- 주로 다음 경우 발생:
  - EventSystem 충돌
  - Canvas/Camera 설정 오류
  - Unity 에디터 캐시 손상
  - 멀티 디스플레이 환경

## 완전 해결됨 확인 방법
✅ Play Mode 정상 진입
✅ Console에 빨간 에러 없음
✅ 영웅들이 정상 생성/이동
✅ Game View에 정상 렌더링

## 여전히 안 되면
1. **Unity 버전 확인** (2022.3 LTS 권장)
2. **프로젝트 백업 후 Unity 재설치**
3. **새 프로젝트 생성 후 Assets만 이전**

---
⚠️ **중요**: EmergencyFix.cs는 문제 해결 후 제거 가능합니다.