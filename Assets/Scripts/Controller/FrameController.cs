using System;
using System.Collections.Generic;
using UnityEngine;

public class FrameController : MonoBehaviour
{
    private static FrameController _instance;
    public static FrameController Instance
    {
        get
        {
            if (_instance == null)
                CreateController();
            return _instance;
        }
        private set => _instance = value;
    }

    private static void CreateController()
    {
        var go = new GameObject("FrameController");
        Instance = go.AddComponent<FrameController>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        // 중복 인스턴스 방지
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        this.Play();
        DontDestroyOnLoad(gameObject);
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

    public void Add(Action callback, UnityEngine.Object context)
    {
        if (handlers.Exists(h => h.callback == callback && h.context == context))
            return;

        Handler h = handlerPool.Count > 0 ? handlerPool.Pop() : new Handler();
        h.callback = callback;
        h.context = context;
        handlers.Add(h);
    }

    public bool Remove(Action callback, UnityEngine.Object context)
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

    public void SetSpeed(float newSpeed)
    {
        speed = Mathf.Max(0f, newSpeed);
        accumulatedTime = 0f;
    }

    public float GetSpeed() => speed;
    public void Play() => isRunning = true;
    public void Stop() => isRunning = false;
}
