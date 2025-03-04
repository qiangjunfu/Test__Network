using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


public class SingleTon<T> where T : SingleTon<T>
{
    private static T instance;
    private static readonly object lockObj = new object();  // 锁对象
    private static readonly ConstructorInfo ctor;

    // 静态构造函数，缓存构造函数
    static SingleTon()
    {
        ctor = typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null);

        if (ctor == null)
        {
            throw new InvalidOperationException($"类型 {typeof(T)} 必须包含一个私有的无参构造函数。");
        }
    }

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObj)
                {
                    if (instance == null)
                    {
                        instance = (T)ctor.Invoke(null);
                    }
                }
            }
            return instance;
        }
    }
}
