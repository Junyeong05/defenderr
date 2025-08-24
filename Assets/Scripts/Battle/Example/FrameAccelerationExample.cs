using UnityEngine;

/// <summary>
/// 개별 영웅 가속 시스템 테스트
/// frameCounter를 통한 개별 영웅 속도 조절이 프레임을 스킵하지 않는지 확인
/// </summary>
public class FrameAccelerationExample : MonoBehaviour
{
    private BaseHero testHero;
    private int lastAttackFrame = -1;
    
    void Start()
    {
        // 테스트용 영웅 찾기
        GameObject heroObj = GameObject.FindGameObjectWithTag("Hero");
        if (heroObj != null)
        {
            testHero = heroObj.GetComponent<BaseHero>();
        }
    }
    
    void Update()
    {
        // 1. 게임 전체 배속 테스트 (FrameController)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestGameSpeed(1f);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TestGameSpeed(2f);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            TestGameSpeed(5f);
        }
        
        // 2. 개별 영웅 가속 테스트 (animationSpeed)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TestHeroSpeed(0.5f);  // 0.5배속 (느리게)
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            TestHeroSpeed(1f);    // 1배속 (정상)
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            TestHeroSpeed(2f);    // 2배속 (빠르게)
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            TestHeroSpeed(2.5f);  // 2.5배속 (매우 빠르게)
        }
        
        // 3. 프레임 스킵 체크
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckFrameSkipping();
        }
    }
    
    // 게임 전체 배속 조절
    void TestGameSpeed(float speed)
    {
        FrameController.SetSpeed(speed);
        Debug.Log($"=== Game Speed Test ===");
        Debug.Log($"Game speed set to: {speed}x");
        Debug.Log($"All heroes will execute {speed} times per Unity frame");
        Debug.Log($"No frames should be skipped");
    }
    
    // 개별 영웅 속도 조절
    void TestHeroSpeed(float speed)
    {
        if (testHero == null) return;
        
        testHero.AnimationSpeed = speed;
        Debug.Log($"=== Hero Speed Test ===");
        Debug.Log($"Hero animation speed set to: {speed}x");
        Debug.Log($"Hero will execute {speed} logical frames per game frame");
        
        if (speed > 1f)
        {
            Debug.Log($"With speed {speed}:");
            Debug.Log($"- Execute() will run multiple times per frame");
            Debug.Log($"- DoAttack() will check every logical frame");
            Debug.Log($"- No attack frames will be skipped");
        }
        else if (speed < 1f)
        {
            Debug.Log($"With speed {speed}:");
            Debug.Log($"- Frames will accumulate over multiple game frames");
            Debug.Log($"- Hero moves slower but still processes all frames");
        }
    }
    
    // 프레임 스킵 검증
    void CheckFrameSkipping()
    {
        if (testHero == null) return;
        
        Debug.Log($"=== Frame Skip Check ===");
        Debug.Log($"Current animation speed: {testHero.AnimationSpeed}");
        Debug.Log($"Current frame: {testHero.CurrentFrame}");
        
        // 실제 프레임 진행 모니터링
        StartCoroutine(MonitorFrames());
    }
    
    // 프레임 진행 모니터링 (5초간)
    System.Collections.IEnumerator MonitorFrames()
    {
        Debug.Log("Starting frame monitoring for 5 seconds...");
        
        int[] frameHistory = new int[300]; // 5초 * 60fps
        int historyIndex = 0;
        float startTime = Time.time;
        
        while (Time.time - startTime < 5f)
        {
            if (testHero != null && historyIndex < frameHistory.Length)
            {
                frameHistory[historyIndex] = testHero.CurrentFrame;
                
                // 공격 프레임 체크
                if (testHero.State == BaseHero.STATE_ATTACK)
                {
                    if (testHero.CurrentFrame != lastAttackFrame)
                    {
                        Debug.Log($"Attack frame executed: {testHero.CurrentFrame}");
                        lastAttackFrame = testHero.CurrentFrame;
                    }
                }
                
                historyIndex++;
            }
            
            yield return null; // 다음 프레임까지 대기
        }
        
        // 결과 분석
        AnalyzeFrameHistory(frameHistory, historyIndex);
    }
    
    // 프레임 히스토리 분석
    void AnalyzeFrameHistory(int[] history, int count)
    {
        Debug.Log($"=== Frame Analysis Results ===");
        Debug.Log($"Total frames recorded: {count}");
        
        // 프레임 점프 검사
        int skippedFrames = 0;
        int maxJump = 0;
        
        for (int i = 1; i < count; i++)
        {
            int jump = history[i] - history[i-1];
            
            // 음수는 루프백을 의미
            if (jump < 0)
            {
                jump = history[i] + (testHero.AnimEndFrame - history[i-1] + 1);
            }
            
            if (jump > 1)
            {
                skippedFrames += (jump - 1);
                if (jump > maxJump) maxJump = jump;
            }
        }
        
        if (skippedFrames > 0)
        {
            Debug.LogWarning($"WARNING: {skippedFrames} frames were skipped!");
            Debug.LogWarning($"Maximum jump: {maxJump} frames");
            Debug.LogWarning($"This may cause attacks or skills to be missed!");
        }
        else
        {
            Debug.Log($"SUCCESS: No frames were skipped!");
            Debug.Log($"All attack and skill frames will be executed correctly.");
        }
        
        // 평균 프레임 진행 속도 계산
        float avgProgress = (float)count / 300f;
        Debug.Log($"Average frame progress rate: {avgProgress:F2}x");
    }
    
    void OnGUI()
    {
        // UI 표시
        if (testHero != null)
        {
            GUI.Box(new Rect(10, 10, 300, 150), "Frame Acceleration Test");
            
            GUI.Label(new Rect(20, 40, 280, 20), 
                $"Game Speed: {FrameController.GetSpeed()}x");
            GUI.Label(new Rect(20, 60, 280, 20), 
                $"Hero Animation Speed: {testHero.AnimationSpeed}x");
            GUI.Label(new Rect(20, 80, 280, 20), 
                $"Current Frame: {testHero.CurrentFrame}");
            GUI.Label(new Rect(20, 100, 280, 20), 
                $"State: {GetStateName(testHero.State)}");
            GUI.Label(new Rect(20, 120, 280, 20), 
                $"Frame Counter: {testHero.FrameCounter:F2}");
        }
    }
    
    string GetStateName(int state)
    {
        switch (state)
        {
            case BaseHero.STATE_WAIT: return "WAIT";
            case BaseHero.STATE_MOVE: return "MOVE";
            case BaseHero.STATE_ATTACK: return "ATTACK";
            case BaseHero.STATE_SKILL: return "SKILL";
            case BaseHero.STATE_DIE: return "DIE";
            default: return "UNKNOWN";
        }
    }
}