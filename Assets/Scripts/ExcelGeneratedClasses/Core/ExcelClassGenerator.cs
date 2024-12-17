#if UNITY_EDITOR
//#endif
using ExcelDataReader;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;


public class ExcelClassGenerator : EditorWindow
{
    private string folderPath = "";
    private string selectedFilePath = "";

    [MenuItem("Tools/ExcelTools/Excel Generate Classes")]
    public static void ShowWindow()
    {
        GetWindow(typeof(ExcelClassGenerator)).Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Generate C# Classes from Excel Files", EditorStyles.boldLabel);
        if (GUILayout.Button("Select Folder"))
        {
            string excelsPath = "Assets/StreamingAssets/Excels";
            folderPath = EditorUtility.OpenFolderPanel("Select Folder with Excel Files", excelsPath, "");

            if (!string.IsNullOrEmpty(folderPath))
            {
                GenerateClasses(folderPath);
            }
        }

        if (GUILayout.Button("Select Excel File"))
        {
            string excelsPath = "Assets/StreamingAssets/Excels";
            selectedFilePath = EditorUtility.OpenFilePanel("Select Excel File", excelsPath, "xlsx");
            if (!string.IsNullOrEmpty(selectedFilePath))
            {
                GenerateClassForExcel(selectedFilePath);
            }
        }

        AssetDatabase.Refresh(); // Refresh after all files are written
    }

    private void GenerateClasses(string folderPath)
    {
        string[] excelFiles = Directory.GetFiles(folderPath, "*.xlsx");
        foreach (var file in excelFiles)
        {
            GenerateClassForExcel(file);
        }
      
    }


    // -----------

    #region MyRegion
    private void GenerateClassForExcel(string filePath)
    {
        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
                });

                var dataTable = result.Tables[0];
                var className = Path.GetFileNameWithoutExtension(filePath);
                StringBuilder classContent = new StringBuilder();

                classContent.AppendLine("using System;");
                classContent.AppendLine("using System.Collections;");
                classContent.AppendLine("using System.Collections.Generic;");
                classContent.AppendLine("using UnityEngine;\n");
                classContent.AppendLine();
                classContent.AppendLine("[System.Serializable]");
                classContent.AppendLine($"public class {className}");
                classContent.AppendLine("{");

                foreach (DataColumn column in dataTable.Columns)
                {
                    string propertyName = column.ColumnName.Replace(" ", "").Replace("-", "");
                    string type = InferDataType(dataTable, column);
                    classContent.AppendLine($"    [SerializeField, ReadOnly] public {type} {propertyName};");
                }

                classContent.AppendLine($"\n\n    public {className}() {{ }}");

                classContent.AppendLine("}");

                string folderPath = Path.Combine(Application.dataPath, "Scripts/ExcelGeneratedClasses");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fullPath = Path.Combine(folderPath, $"{className}.cs");
                File.WriteAllText(fullPath, classContent.ToString()); // This should overwrite existing files
            }
        }
    }


    private string InferDataType(DataTable dataTable, DataColumn column)
    {
        bool anyCommas = false; // Check if there are commas
        bool consistentArrays = true; // Check if arrays are consistent

        bool allInt = true;
        bool allBool = true;
        bool allFloat = true;

        foreach (DataRow row in dataTable.Rows)
        {
            string cellValue = row[column].ToString().Trim();
            string[] values = cellValue.Split(',');

            if (values.Length > 1) // There's a comma, assume array
            {
                anyCommas = true;
                CheckArrayTypes(values, ref allInt, ref allBool, ref allFloat);
            }
            else // No comma, regular field
            {
                if (anyCommas) // Previous entries were arrays, now a single value
                {
                    consistentArrays = false;
                }
                UpdateTypeChecks(cellValue, ref allInt, ref allBool, ref allFloat);
            }
        }

        if (anyCommas && consistentArrays)
        {
            if (allInt) return "int[]";
            if (allBool) return "bool[]";
            if (allFloat) return "float[]";
            return "string[]"; // Default to string array if no specific type matches
        }
        else
        {
            if (allInt && !allFloat) return "int";
            if (allBool) return "bool";
            if (allFloat) return "float"; // Ensure floats are recognized correctly
            return "string"; // Default to string if no specific type matches
        }
    }

    private void CheckArrayTypes(string[] values, ref bool allInt, ref bool allBool, ref bool allFloat)
    {
        foreach (string value in values)
        {
            string trimmedValue = value.Trim();
            if (!IsInteger(trimmedValue))
                allInt = false;
            if (!bool.TryParse(trimmedValue, out _))
                allBool = false;
            if (!IsFloat(trimmedValue))
                allFloat = false;
        }
    }

    private void UpdateTypeChecks(string value, ref bool allInt, ref bool allBool, ref bool allFloat)
    {
        value = value.Trim();
        if (!IsInteger(value))
            allInt = false;
        if (!bool.TryParse(value, out _))
            allBool = false;
        if (!IsFloat(value))
            allFloat = false;
    }

    private bool IsInteger(string value)
    {
        // Ensure that values like "3.00" are not treated as integers
        if (value.Contains("."))
        {
            return false;
        }
        return int.TryParse(value, out _);
    }

    private bool IsFloat(string value)
    {
        if (float.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float result))
        {
            // Ensure floats are recognized, even if they look like integers (e.g., "3.00")
            return result % 1 != 0 || value.Contains(".");
        }
        return false;
    }

    #endregion



    public string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // 检查第一个字母是否已经是大写 
        if (char.IsUpper(input[0]))
        {
            return input.ToUpper();
        }
        else
        {
            // 如果不是，只将首字母转换为大写
            return char.ToUpper(input[0]) + input.Substring(1);
        }
    }
}

#endif