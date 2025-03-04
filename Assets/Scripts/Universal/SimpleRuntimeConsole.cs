using UnityEngine;
using System.Collections.Generic;

public class SimpleRuntimeConsole : MonoBehaviour
{
    private Queue<string> logQueue = new Queue<string>(); // 缓存日志的队列
    private Vector2 scrollPosition; // 滚动视图的滚动条位置
    private bool showConsole = true; // 是否显示控制台
    private const int MaxLogCount = 50; // 最大日志条数

    private GUIStyle logStyle; // 自定义日志样式

    private void Awake()
    {
        Debug.Log("This is Awake!");
    }

    private void Start()
    {
        Debug.Log("This is Start!");

        // 初始化日志样式
        logStyle = new GUIStyle
        {
            fontSize = 50, // 设置字体大小
            normal = { textColor = Color.white }, // 设置字体颜色
            wordWrap = true // 自动换行
        };
    }

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog; // 订阅日志事件
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog; // 取消订阅日志事件
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        string logEntry = $"[{type}] {logString}";
        if (type == LogType.Exception || type == LogType.Error)
        {
            logEntry += $"\n{stackTrace}";
        }

        logQueue.Enqueue(logEntry); // 将日志加入队列
        if (logQueue.Count > MaxLogCount)
        {
            logQueue.Dequeue(); // 移除最旧的日志
        }
    }

    private void OnGUI()
    {
        // 添加一个按键切换显示控制台
        if (GUI.Button(new Rect(10, 10, 100, 40), showConsole ? "隐藏控制台" : "显示控制台"))
        {
            showConsole = !showConsole;
        }

        if (!showConsole) return;

        // 绘制控制台背景
        GUI.Box(new Rect(10, 60, Screen.width - 20, Screen.height / 2), "控制台日志");

        // 创建滚动区域
        GUILayout.BeginArea(new Rect(20, 80, Screen.width - 40, Screen.height / 2 - 40));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width - 40), GUILayout.Height(Screen.height / 2 - 40));

        // 显示日志
        foreach (var log in logQueue)
        {
            GUILayout.Label(log, logStyle, GUILayout.ExpandHeight(false)); // 应用自定义样式
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}
