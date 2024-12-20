using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltipUI : MonoBehaviour
{
    public GameObject logEntryPrefab; // 日志条目的 Prefab
    public Transform content;         // Scroll View 的 Content 对象
    public ScrollRect scrollRect;     // Scroll View 本体


    // 添加一条日志
    public void AddLog(string message)
    {
        //Debug.Log(message);

        // 实例化日志条目
        GameObject logEntry = Instantiate(logEntryPrefab, content);

        // 设置日志内容
        Text logText = logEntry.transform .Find ("Text").GetComponent<Text>();
        logText.text = message;

        // 自动滚动到最新消息
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0;
    }

    // 清空所有日志
    public void ClearLogs()
    {
        // 删除 Content 下的所有子对象
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // 强制更新 UI
        Canvas.ForceUpdateCanvases();
    }

    // 测试日志
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            AddLog($"[Log] {System.DateTime.Now}: 测试消息");
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearLogs();
        }
    }
}
