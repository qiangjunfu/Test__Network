using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum GameEventType
{
    None,
    NetworkSuccess,
    NetworkFaild,
    WebRequestError,

    ExitYuAnBianJi,
    DHC_DeviceDataUpdate,

    SceneloadStart,

}

public delegate void Callback();
public delegate void Callback<T>(T arg1);
public delegate void Callback<T1, T2>(T1 arg1, T2 arg2);
public delegate void Callback<T1, T2,T3>(T1 arg1, T2 arg2 ,T3 arg3 );


public static  class MessageManager
{
    public static Dictionary<GameEventType, Delegate> AllEventDic = new Dictionary<GameEventType, Delegate>();


    #region  添加监听
    public static void AddListener(GameEventType eventType, Callback handler)
    {
        if (!IsCanAddListener(eventType, handler))
        {
            return;
        }

        AllEventDic[eventType] = (Callback)AllEventDic[eventType] + handler;
    }
    public static void AddListener<T>(GameEventType eventType, Callback<T> handler)
    {
        if (!IsCanAddListener(eventType, handler))
        {
            return;
        }

        AllEventDic[eventType] = (Callback<T>)AllEventDic[eventType] + handler;
    }
    public static void AddListener<T1, T2>(GameEventType eventType, Callback<T1, T2> handler)
    {
        if (!IsCanAddListener(eventType, handler))
        {
            return;
        }

        AllEventDic[eventType] = (Callback<T1, T2>)AllEventDic[eventType] + handler;
    }
    public static void AddListener<T1, T2, T3>(GameEventType eventType, Callback<T1, T2, T3> handler)
    {
        if (!IsCanAddListener(eventType, handler))
        {
            return;
        }

        AllEventDic[eventType] = (Callback<T1, T2, T3>)AllEventDic[eventType] + handler;
    }

    private static bool IsCanAddListener(GameEventType eventType, Delegate handler)
    {
        if (!AllEventDic.ContainsKey(eventType))
        {
            AllEventDic.Add(eventType, null);
        }

        Delegate d = AllEventDic[eventType];

        if (d != null && d.GetType() != handler.GetType())
        {
            Debug.Log(eventType + " 加入監聽回調與當前監聽類型不符合,  當前類型:" + d.GetType().Name + " , 加入類型:" + handler.GetType().Name);
            return false;
        }

        return true;
    }
    #endregion


    #region  移除监听
    public static void RemoveListener(GameEventType eventType, Callback handler)
    {
        if (!IsCanRemoveListener(eventType, handler))
        {
            return;
        }

        AllEventDic[eventType] = (Callback)AllEventDic[eventType] - handler;

        if (AllEventDic[eventType] == null)
        {
            AllEventDic.Remove(eventType);
        }
    }

    public static void RemoveListener<T>(GameEventType eventType, Callback<T> handler)
    {
        if (!IsCanRemoveListener(eventType, handler))
        {
            return;
        }

        AllEventDic[eventType] = (Callback<T>)AllEventDic[eventType] - handler;

        if (AllEventDic[eventType] == null)
        {
            AllEventDic.Remove(eventType);
        }
    }
    public static void RemoveListener<T1, T2>(GameEventType eventType, Callback<T1, T2> handler)
    {
        if (!IsCanRemoveListener(eventType, handler))
        {
            return;
        }

        AllEventDic[eventType] = (Callback<T1, T2>)AllEventDic[eventType] - handler;

        if (AllEventDic[eventType] == null)
        {
            AllEventDic.Remove(eventType);
        }
    }
    public static void RemoveListener<T1, T2, T3>(GameEventType eventType, Callback<T1, T2, T3> handler)
    {
        if (!IsCanRemoveListener(eventType, handler))
        {
            return;
        }

        AllEventDic[eventType] = (Callback<T1, T2, T3>)AllEventDic[eventType] - handler;

        if (AllEventDic[eventType] == null)
        {
            AllEventDic.Remove(eventType);
        }
    }

    #endregion


    #region  广播
    public static bool IsCanRemoveListener(GameEventType eventType, Delegate handler)
    {
        if (AllEventDic.ContainsKey(eventType))
        {
            Delegate d = AllEventDic[eventType];

            if (d == null)
            {
                Debug.Log("試圖移除的 " + eventType + " , 但當前監聽為空");
                return false;
            }
            else if (d.GetType() != handler.GetType())
            {
                Debug.Log("試圖移除的 " + eventType + " , 與當前類型 " + d.GetType().Name + " 不服和");

                return false;
            }
        }
        else
        {
            Debug.Log("MessageManager 不包含要移除的對象 " + eventType);
        }

        return true;
    }


    public static void Broadcast(GameEventType eventType)
    {
        if (!AllEventDic.ContainsKey(eventType))
        {
            return;
        }

        Delegate dele_gate;
        if (AllEventDic.TryGetValue(eventType, out dele_gate))
        {
            Callback call = dele_gate as Callback;

            if (call != null)
            {
                call();
            }
            else
            {
                Debug.Log("廣播 " + eventType + " 報錯");
            }
        }
    }

    public static void Broadcast<T>(GameEventType eventType, T arg1)
    {
        if (!AllEventDic.ContainsKey(eventType))
        {
            return;
        }

        Delegate d;
        if (AllEventDic.TryGetValue(eventType, out d))
        {
            Callback<T> call = d as Callback<T>;

            if (call != null)
            {
                call(arg1);
            }
            else
            {
                Debug.Log("廣播 " + eventType + " 報錯");
            }
        }
    }

    public static void Broadcast<T1, T2>(GameEventType eventType, T1 arg1, T2 arg2)
    {
        if (!AllEventDic.ContainsKey(eventType))
        {
            return;
        }

        Delegate d;
        if (AllEventDic.TryGetValue(eventType, out d))
        {
            Callback<T1, T2> call = d as Callback<T1, T2>;

            if (call != null)
            {
                call(arg1, arg2);
            }
            else
            {
                Debug.Log("廣播 " + eventType + " 報錯");
            }
        }
    }

    public static void Broadcast<T1, T2 ,T3 >(GameEventType eventType, T1 arg1, T2 arg2, T3 arg3)
    {
        if (!AllEventDic.ContainsKey(eventType))
        {
            return;
        }

        Delegate d;
        if (AllEventDic.TryGetValue(eventType, out d))
        {
            Callback<T1, T2, T3> call = d as Callback<T1, T2, T3>;

            if (call != null)
            {
                call(arg1, arg2 , arg3);
            }
            else
            {
                Debug.Log("廣播 " + eventType + " 報錯");
            }
        }
    }

  

    #endregion
}
