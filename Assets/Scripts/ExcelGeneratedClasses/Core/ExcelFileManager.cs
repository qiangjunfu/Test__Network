using ExcelDataReader;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;


// ������ȡ������
public class ExcelFileManager : MonoBehaviour
{
    #region Mono����

    private static ExcelFileManager instance;
    private static readonly object lockObj = new object();
    private static bool isApplicationQuitting = false; // ��ֹ�˳�ʱ������ʵ��

    public static ExcelFileManager Instance
    {
        get
        {
            if (isApplicationQuitting)
            {
                Debug.LogWarning($"[ExcelFileManager] Instance requested after application quit. Returning null.");
                return null;
            }

            lock (lockObj)
            {
                if (instance == null)
                {
                    // ���Դӳ������ҵ�ʵ��
                    instance = FindObjectOfType<ExcelFileManager>();
                    if (instance == null)
                    {
                        // ���δ�ҵ����Զ�����
                        GameObject obj = new GameObject(nameof(ExcelFileManager));
                        instance = obj.AddComponent<ExcelFileManager>();
                        DontDestroyOnLoad(obj); // ���ֿ糡������
                    }
                }
                return instance;
            }
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject); // ���ֿ糡������
            Initialize();
        }
        else if (instance != this)
        {
            Debug.LogWarning($"[ExcelFileManager] Duplicate instance detected. Destroying this object.");
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        isApplicationQuitting = true;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    /// <summary>
    /// ��ʼ���߼�
    /// </summary>
    private void Initialize()
    {
        // ��������ӳ�ʼ���߼�
        Debug.Log("ExcelFileManager Initialized.");

    }
    #endregion

    [SerializeField, ReadOnly] string folderPath = "";
    [SerializeField] List<TestData> m_TestDataList = new List<TestData>();
    [SerializeField] List<NetworkData> m_NetworkDataList = new List<NetworkData>(); 




    public void Init()
    {
        // //ע�� CodePagesEncodingProvider����֧�� ExcelDataReader ����ı���
        //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        //Debug.Log("ExcelFileManager Initialized.");


        //folderPath = Path.Combine(Application.dataPath, "StreamingAssets/Excels");
        //folderPath = Path.Combine(Application.streamingAssetsPath, "Excels");
        //if (!Directory.Exists(folderPath))
        //{
        //    Debug.LogError($"�ļ��в����ڣ�{folderPath}");
        //    GameClientManager .Instance .AddLog($"�ļ��в����ڣ�{folderPath}");
        //    return;
        //}
        //else
        //{
        //    GameClientManager.Instance.AddLog($"�ļ��д��ڣ�{folderPath}");
        //}

        folderPath = Path.Combine(Application.streamingAssetsPath, "Excels");
        Debug.Log("folderPath: " + folderPath);
        ReadExcelFiles(folderPath);

    }


    #region GetDataList

    public List<TestData> GetTestDataList()
    {
        return new List<TestData>(this.m_TestDataList);
    }
    public List<NetworkData> GetNetworkDataList()
    {
        return new List<NetworkData>(this.m_NetworkDataList); 
    }
 
    #endregion


    private void ReadExcelFiles(string path)
    {
        string[] filePaths = Directory.GetFiles(path, "*.xlsx", SearchOption.AllDirectories);
        foreach (string filePath in filePaths)
        {
            ProcessExcelFile(filePath);
        }
    }
    private void ProcessExcelFile(string filePath)
    {
        Debug.Log($"Reading Excel file: {filePath}");
        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = true
                    }
                });

                foreach (DataTable table in result.Tables)
                {
                    string dataType = Path.GetFileNameWithoutExtension(filePath);
                    //Type type = Type.GetType($"{dataType}");
                    Debug.Log($"Processing {table.TableName} , {dataType}");
                    var list = ConvertDataTableToList(table, Path.GetFileNameWithoutExtension(filePath));
                    var jsonData = JsonConvert.SerializeObject(list, Formatting.Indented);
                    //Debug.Log(jsonData);

                    switch (dataType)
                    {
                        case "TestData":
                            List<TestData> TestDataList = JsonConvert.DeserializeObject<List<TestData>>(jsonData);
                            this.m_TestDataList.AddRange(TestDataList);
                            break;
                        case "NetworkData":
                            List<NetworkData> NetworkDataList = JsonConvert.DeserializeObject<List<NetworkData>>(jsonData);
                            this.m_NetworkDataList.AddRange(NetworkDataList);
                            break;
                        default:
                            Debug.LogWarning($"Unsupported data type {dataType}");
                            break;
                    }



                }
            }
        }
    }

    //   Type type = Type.GetType($"{className}");
    private List<object> ConvertDataTableToList(DataTable table, string className)
    {
        List<object> list = new List<object>();
        //// �滻Ϊ�����������ռ������
        //Type type = Type.GetType($"YourNamespace.{className}", true);
        Type type = Type.GetType($"{className}", true);

        if (type == null)
        {
            Debug.LogError($"Class not found for file/class: {className}");
            return list;
        }

        foreach (DataRow row in table.Rows)
        {
            var instance = Activator.CreateInstance(type);
            foreach (DataColumn column in table.Columns)
            {
                // ���Ի�ȡ���ԣ����ʧ�����ȡ�ֶ�
                MemberInfo member = type.GetProperty(column.ColumnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                                ?? (MemberInfo)type.GetField(column.ColumnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (member is PropertyInfo propertyInfo)
                {
                    SetMemberValue(propertyInfo, instance, row[column]);
                }
                else if (member is FieldInfo fieldInfo)
                {
                    SetMemberValue(fieldInfo, instance, row[column]);
                }
                else
                {
                    Debug.LogError($"No field or property found for column: {column.ColumnName}");
                }
            }
            list.Add(instance);
        }

        return list;
    }
    // ͨ�����ó�Աֵ�ķ���
    private void SetMemberValue(MemberInfo member, object instance, object value)
    {
        if (value != DBNull.Value)
        {
            if (member.MemberType == MemberTypes.Property)
            {
                var propertyInfo = (PropertyInfo)member;
                if (propertyInfo.PropertyType.IsArray)
                {
                    SetPropertyArrayValue(propertyInfo, instance, value.ToString());
                }
                else
                {
                    propertyInfo.SetValue(instance, Convert.ChangeType(value, propertyInfo.PropertyType), null);
                }
            }
            else if (member.MemberType == MemberTypes.Field)
            {
                var fieldInfo = (FieldInfo)member;
                if (fieldInfo.FieldType.IsArray)
                {
                    SetFieldArrayValue(fieldInfo, instance, value.ToString());
                }
                else
                {
                    fieldInfo.SetValue(instance, Convert.ChangeType(value, fieldInfo.FieldType));
                }
            }
        }
        else
        {
            Debug.Log($"Null value for {member.Name} in row.");
        }
    }
    private void SetMemberValue2(MemberInfo member, object instance, object value)
    {
        if (value != DBNull.Value)
        {
            if (member.MemberType == MemberTypes.Property)
            {
                var propertyInfo = (PropertyInfo)member;
                if (propertyInfo.PropertyType.IsArray)
                {
                    SetPropertyArrayValue(propertyInfo, instance, value.ToString());
                }
                else
                {
                    // ������������Ƿ�Ϊ float �� double���������ʵ���ת��
                    if (propertyInfo.PropertyType == typeof(float))
                    {
                        propertyInfo.SetValue(instance, Convert.ToSingle(value), null);
                    }
                    else if (propertyInfo.PropertyType == typeof(double))
                    {
                        propertyInfo.SetValue(instance, Convert.ToDouble(value), null);
                    }
                    else
                    {
                        propertyInfo.SetValue(instance, Convert.ChangeType(value, propertyInfo.PropertyType), null);
                    }
                }
            }
            else if (member.MemberType == MemberTypes.Field)
            {
                var fieldInfo = (FieldInfo)member;
                if (fieldInfo.FieldType.IsArray)
                {
                    SetFieldArrayValue(fieldInfo, instance, value.ToString());
                }
                else
                {
                    // ����ֶ������Ƿ�Ϊ float �� double���������ʵ���ת��
                    if (fieldInfo.FieldType == typeof(float))
                    {
                        fieldInfo.SetValue(instance, Convert.ToSingle(value));
                    }
                    else if (fieldInfo.FieldType == typeof(double))
                    {
                        fieldInfo.SetValue(instance, Convert.ToDouble(value));
                    }
                    else
                    {
                        fieldInfo.SetValue(instance, Convert.ChangeType(value, fieldInfo.FieldType));
                    }
                }
            }
        }
        else
        {
            Debug.Log($"Null value for {member.Name} in row.");
        }
    }


    private void SetPropertyArrayValue(PropertyInfo propertyInfo, object instance, string stringValue)
    {
        Type elementType = propertyInfo.PropertyType.GetElementType();
        string[] stringValues = stringValue.Split(',');
        Array array = Array.CreateInstance(elementType, stringValues.Length);
        for (int i = 0; i < stringValues.Length; i++)
        {
            array.SetValue(Convert.ChangeType(stringValues[i].Trim(), elementType), i);
        }
        propertyInfo.SetValue(instance, array, null);
    }

    private void SetFieldArrayValue(FieldInfo fieldInfo, object instance, string stringValue)
    {
        Type elementType = fieldInfo.FieldType.GetElementType();
        string[] stringValues = stringValue.Split(',');
        Array array = Array.CreateInstance(elementType, stringValues.Length);
        for (int i = 0; i < stringValues.Length; i++)
        {
            array.SetValue(Convert.ChangeType(stringValues[i].Trim(), elementType), i);
        }
        fieldInfo.SetValue(instance, array);
    }


}
