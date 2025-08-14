using System;
using System.Collections.Generic;
using UnityEngine;

// AS3/PixiJS Ticker와 유사한 프레임 기반 업데이트 시스템
// Main Camera에 자동으로 추가됨
public class FrameController : MonoBehaviour
{
    private static FrameController _instance;
    
    public static FrameController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Camera.main?.GetComponent<FrameController>();
                
                if (_instance == null && Camera.main != null)
                {
                    _instance = Camera.main.gameObject.AddComponent<FrameController>();
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }
        
        _instance = this;
        this.PlayInternal();
    }

    private class Handler
    {
        public Action callback;
        public UnityEngine.Object context;
    }

    private readonly List<Handler> handlers = new List<Handler>();
    private readonly Stack<Handler> handlerPool = new Stack<Handler>();

    private readonly float frameInterval = 1f / 60f;
    private float accumulatedTime = 0f;

    private float speed = 1f;
    private bool isRunning = true;

    void Update()
    {
        if (!isRunning || speed <= 0f) return;
        accumulatedTime += Time.deltaTime * speed;
        while (accumulatedTime >= frameInterval)
        {
            ExecuteHandlers();
            accumulatedTime -= frameInterval;
        }
    }

    private void ExecuteHandlers()
    {
        for (int i = handlers.Count - 1; i >= 0; i--)
        {
            var h = handlers[i];
            if (h.callback != null && h.context != null)
                h.callback.Invoke();
        }
    }

    // === 내부 메서드 ===
    private void AddHandler(Action callback, UnityEngine.Object context)
    {
        if (handlers.Exists(h => h.callback == callback && h.context == context))
            return;

        Handler h = handlerPool.Count > 0 ? handlerPool.Pop() : new Handler();
        h.callback = callback;
        h.context = context;
        handlers.Add(h);
    }

    private bool RemoveHandler(Action callback, UnityEngine.Object context)
    {
        int idx = handlers.FindIndex(h => h.callback == callback && h.context == context);
        if (idx == -1) return false;

        var h = handlers[idx];
        handlers.RemoveAt(idx);

        h.callback = null;
        h.context = null;
        handlerPool.Push(h);
        return true;
    }

    private void SetSpeedInternal(float newSpeed)
    {
        speed = Mathf.Max(0f, newSpeed);
        accumulatedTime = 0f;
    }

    private float GetSpeedInternal() => speed;
    private void PlayInternal() => isRunning = true;
    private void StopInternal() => isRunning = false;
    
    // === 공개 API (정적 메서드) ===
    public static void Add(Action callback, UnityEngine.Object context)
    {
        Instance?.AddHandler(callback, context);
    }
    
    public static bool Remove(Action callback, UnityEngine.Object context)
    {
        return Instance?.RemoveHandler(callback, context) ?? false;
    }
    
    public static void SetSpeed(float newSpeed)
    {
        Instance?.SetSpeedInternal(newSpeed);
    }
    
    public static float GetSpeed()
    {
        return Instance?.GetSpeedInternal() ?? 1f;
    }
    
    public static void Play()
    {
        Instance?.PlayInternal();
    }
    
    public static void Stop()
    {
        Instance?.StopInternal();
    }
    
    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
        
        handlers.Clear();
        handlerPool.Clear();
    }
    
}
