#if UNITY_EDITOR
//#endif
using UnityEditor;
using UnityEngine;
using System.IO;
using ExcelDataReader;
using System.Data;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public class ExcelDataToJson : EditorWindow
{
    string folderPath = string.Empty;
    string selectedFilePath = string.Empty;
    string saveJsonFilePath = string.Empty; 


    [MenuItem("Tools/ExcelTools/Excel To Json")]
    public static void ShowWindow()
    {
        GetWindow(typeof(ExcelDataToJson)).Show();
    }

    void OnGUI()
    {
        string excelsPath = "Assets/StreamingAssets/Excels";
        saveJsonFilePath = Path.Combine(Application.dataPath, "StreamingAssets/Jsons");


        GUILayout.Label("Load Data from Excel Files", EditorStyles.boldLabel);
        if (GUILayout.Button("Select Excel Files Folder"))
        {
            folderPath = EditorUtility.OpenFolderPanel("Select Folder with Excel Files", excelsPath, "");
            if (!string.IsNullOrEmpty(folderPath))
            {
                ReadExcelFiles(folderPath);
            }

            AssetDatabase.Refresh();
        }

        if (GUILayout.Button("Select Single Excel File"))
        {
            selectedFilePath = EditorUtility.OpenFilePanel("Select Excel File", excelsPath, "xlsx");
            if (!string.IsNullOrEmpty(selectedFilePath))
            {
                ReadSingleExcelFile(selectedFilePath);
            }

            AssetDatabase.Refresh();
        }
    }


    #region MyRegion
    private void ReadExcelFiles(string path)
    {
        string[] filePaths = Directory.GetFiles(path, "*.xlsx", SearchOption.AllDirectories);
        foreach (string filePath in filePaths)
        {
            ProcessExcelFile(filePath);
        }
    }
    private void ReadSingleExcelFile(string filePath)
    {
        ProcessExcelFile(filePath);
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
                    Debug.Log($"Processing {table.TableName}");
                    var list = ConvertDataTableToList(table, Path.GetFileNameWithoutExtension(filePath));
                    var jsonData = JsonConvert.SerializeObject(list, Formatting.Indented);
                    Debug.Log(jsonData);


                    
                    if (!Directory.Exists(saveJsonFilePath ))
                    {
                        Directory.CreateDirectory(saveJsonFilePath);
                    }
                    var jsonFileName = Path.GetFileNameWithoutExtension(filePath) + ".json";
                    var jsonFilePath = Path.Combine(saveJsonFilePath, jsonFileName);

                    SaveDataToJson(list, jsonFilePath);
                }
            }
        }
    }

    /*
    private void ReadExcelFiles___22(string path)
    {
        string[] filePaths = Directory.GetFiles(path, "*.xlsx", SearchOption.AllDirectories);
        foreach (string filePath in filePaths)
        {
            string className = Path.GetFileNameWithoutExtension(filePath); // 获取不带扩展名的文件名作为类名
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
                        Debug.Log($"Processing {table.TableName} in {className}");
                        List<object> list = ConvertDataTableToList(table, className);
                        string jsonData = JsonConvert.SerializeObject(list, Formatting.Indented);
                        Debug.Log(jsonData);


                        // Save to JSON file
                        string jsonFilePath = Path.ChangeExtension(filePath, ".json");
                        SaveDataToJson(list, jsonFilePath);
                    }
                }
            }
        }
    }
    */
    #endregion



    //   Type type = Type.GetType($"{className}");
    private List<object> ConvertDataTableToList(DataTable table, string className)
    {
        List<object> list = new List<object>();
        //// 替换为你的类的命名空间和名称
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
                // 尝试获取属性，如果失败则获取字段
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

    // 通用设置成员值的方法
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




    private void SaveDataToJson(List<object> data, string filePath)
    {
        try
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, json);
            Debug.Log("Data saved to JSON file: " + filePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save JSON: " + e.Message);
        }
    }

}

#endif