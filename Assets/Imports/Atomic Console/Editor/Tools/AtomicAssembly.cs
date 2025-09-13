using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using AtomicConsole;
using System.Linq;
using AtomicConsole.Mapping;
using AtomicMapping;

namespace AtomicAssembly.generator
{

public class AtomicAssembly
{
    [MenuItem("Tools/Atomic Console/Generate Assembly", false, 1)]
        public static void GenerateCommandAndSetLists()
    {
        List<string> commandMethods = new List<string>();
        List<string> setFields = new List<string>();
        List<CommandInfo> commandInfoList = new List<CommandInfo>();
        List<CommandInfo> fieldInfoList = new List<CommandInfo>(); // New list for field information

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblies)
        {
            foreach (Type type in assembly.GetTypes())
            {
                foreach (MethodInfo method in type.GetMethods())
                {
                    if (Attribute.IsDefined(method, typeof(AtomicCommandAttribute)))
                    {
                        AtomicCommandAttribute attribute = method.GetCustomAttribute<AtomicCommandAttribute>();
                        commandMethods.Add($"typeof({type.FullName}).GetMethod(\"{method.Name}\")");

                        // Get parameter types
                        ParameterInfo[] parameters = method.GetParameters();
                        string paramTypes = string.Join(", ", parameters.Select(p => $"{AtomicTypeMapping.GetType(p.ParameterType)}"));

                        commandInfoList.Add(new CommandInfo
                        {
                            Name = string.IsNullOrEmpty(attribute.Group) ? attribute.Name : attribute.Group + "." + attribute.Name,
                            Description = attribute.Description,
                            Locked = attribute.PasswordProtected,
                            TypeField = paramTypes,
                            StaticMethod = method.IsStatic
                        });
                    }
                }

                foreach (FieldInfo field in type.GetFields())
                {
                    if (Attribute.IsDefined(field, typeof(AtomicSetAttribute)))
                    {
                        setFields.Add($"typeof({type.FullName}).GetField(\"{field.Name}\")");

                        // Add field information to the list
                        AtomicSetAttribute attribute = field.GetCustomAttribute<AtomicSetAttribute>();
                        fieldInfoList.Add(new CommandInfo
                        {
                            //Name = $"Variable - {field.Name}",
                            Name = string.IsNullOrEmpty(attribute.Group) ? attribute.Name : attribute.Group + "." + attribute.Name,
                            Description = attribute.Description,
                            TypeField = AtomicTypeMapping.GetType(field.FieldType),
                            Locked = attribute.PasswordProtected,
                            StaticMethod = field.IsStatic
                        });
                    }
                }
            }
        }

        string generatedCode = $@"using System.Collections.Generic;
using System.Reflection;

namespace AtomicAssembly.GeneratedCommands
{{
    public static class AtomicCommands
    {{
        public static List<MethodInfo> commandMethods = new List<MethodInfo>
        {{
            {string.Join(",\n        ", commandMethods)}
        }};

        public static List<FieldInfo> setFields = new List<FieldInfo>
        {{
            {string.Join(",\n        ", setFields)}
        }};
    }}
}}";

        // Combine command and field information
        List<CommandInfo> combinedList = new List<CommandInfo>();
        combinedList.AddRange(commandInfoList);
        combinedList.AddRange(fieldInfoList);

        // Generate JSON file
        string json = JsonUtility.ToJson(new CommandInfoList { Commands = combinedList }, true);
        File.WriteAllText("Assets/Atomic Console/Resources/AtomicCommandList.json", json);



        string path = "Assets/Atomic Console/Scripts/AtomicCommands.cs";

        // Delete the existing file if it exists
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        // Normalize line endings to \r\n (Windows-style)
        generatedCode = generatedCode.Replace("\r\n", "\n").Replace("\n", "\r\n");

        File.WriteAllText(path, generatedCode);

        // Refresh the asset database to reflect changes
        AssetDatabase.Refresh();

        // Log completion message
        Debug.Log("Commands and fields are ready to use!");
    }
}

[Serializable]
public class CommandInfo
{
    public string Name;
    public string Description;
    public string TypeField;
    public bool Locked;
    public bool StaticMethod;
}

[Serializable]
public class CommandInfoList
{
    public List<CommandInfo> Commands;
}
}