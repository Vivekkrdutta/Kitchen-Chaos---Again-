using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using AtomicAssembly.generator;
using System;

namespace AtomicConsole.Engine.editor
{
public class CommandListWindow : EditorWindow
{
    private Vector2 scrollPos;
    private List<CommandDisplayInfo> commandDisplayList = new List<CommandDisplayInfo>();

    [MenuItem("Tools/Atomic Console/CommandList", false, 2)]
    public static void ShowWindow()
    {
        CommandListWindow window = GetWindow<CommandListWindow>("CommandList");
        window.minSize = new Vector2(250, 400);
    }

    private void OnEnable()
    {
        PopulateCommandList();
        RefreshToggle();
    }


    private void RefreshToggle()
    {
        string path = "Assets/Atomic Console/Scripts/AtomicCommands.cs";
        if (File.Exists(path))
        {
            string[] lines = File.ReadAllLines(path);
            int commandMethodsStartIndex = Array.FindIndex(lines, line => line.Contains("public static List<MethodInfo> commandMethods = new List<MethodInfo>"));
            commandMethodsStartIndex += 2; // Skip the opening brace '{' line

            for (int i = 0; i < commandDisplayList.Count; i++)
            {
                CommandDisplayInfo commandInfo = commandDisplayList[i];
                string line = lines[commandMethodsStartIndex + i];
                commandInfo.IsToggled = !line.TrimStart().StartsWith("//");
            }
        }
    }



    private void PopulateCommandList()
    {
        commandDisplayList.Clear();

        // Read the JSON file
        string jsonPath = "Assets/Atomic Console/Resources/AtomicCommandList.json";
        if (File.Exists(jsonPath))
        {
            string json = File.ReadAllText(jsonPath);
            CommandInfoList commandInfoList = JsonUtility.FromJson<CommandInfoList>(json);

            // Populate the command list from the JSON data
            foreach (CommandInfo commandInfo in commandInfoList.Commands)
            {
                string command = $"{commandInfo.Name} - {commandInfo.Description}";
                commandDisplayList.Add(new CommandDisplayInfo 
                { 
                    CommandText = command, 
                    FieldType = commandInfo.TypeField, 
                    IsLocked = commandInfo.Locked,
                    StaticMethod = commandInfo.StaticMethod
                });
            }
        }
        else
        {
            Debug.LogWarning("AtomicCommands.json not found!");
        }
    }

    private void OnGUI()
    {
        GUIStyle boldStyle = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold
        };

        // Custom Header
        EditorGUILayout.Space(5);
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(0, 0, 0, -10)
        };
        EditorGUILayout.LabelField("Command List", titleStyle);
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // Commands
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (CommandDisplayInfo commandInfo in commandDisplayList)
        {
            // Set the background color based on the toggle state
            GUI.backgroundColor = commandInfo.IsToggled ? Color.green : Color.red;
        
            EditorGUILayout.BeginVertical("box");
        
            // Reset the background color to default
            GUI.backgroundColor = Color.white;
            EditorGUILayout.BeginHorizontal();

            // Use the IsToggled property for each command's toggle button
            commandInfo.IsToggled = EditorGUILayout.Toggle("", commandInfo.IsToggled, GUILayout.Width(20));

            string staticText = commandInfo.StaticMethod ? "Static" : "Non-Static";
            EditorGUILayout.LabelField(staticText, boldStyle, GUILayout.Width(80));
            GUIStyle labelStyle = new GUIStyle(EditorStyles.wordWrappedLabel);

            EditorGUILayout.LabelField(commandInfo.CommandText, labelStyle, GUILayout.ExpandWidth(true));


            // Display the type field on the right side
            GUILayout.FlexibleSpace();
            if (!string.IsNullOrEmpty(commandInfo.FieldType))
            {
                EditorGUILayout.LabelField($"Type ({commandInfo.FieldType})", GUILayout.Width(100));
            }
            else
            {
                 EditorGUILayout.LabelField($"Type (Null)", GUILayout.Width(100));
            }

            // Display lock/unlock icon based on some condition
            Texture icon = null;
            if(commandInfo.IsLocked)
            {
               icon = EditorGUIUtility.IconContent("d_AssemblyLock").image;
            }
            GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginVertical("Box");
        // Display the info icon and title
        GUIContent infoLabel = new GUIContent(" Info", EditorGUIUtility.IconContent("_Help").image);
        EditorGUILayout.LabelField(infoLabel, EditorStyles.boldLabel);

        // Display the warning message
        string warningMessage = "Click on any command to toggle its activation status. Disabled commands will appear in red, indicating they are temporarily deactivated and won't be executed.";
        EditorGUILayout.LabelField(warningMessage, EditorStyles.wordWrappedLabel);

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(8);
        
        GUILayout.BeginHorizontal("Box"); 
        if(GUILayout.Button("Cancel", GUILayout.Height(25)))
         {
            Close();
         }
        if (GUILayout.Button("Apply", GUILayout.Height(25)))
        {
            ApplyChangesToAtomicCommands();
            RefreshToggle();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(5);

        // Export Button
        if (GUILayout.Button("Export all commands", GUILayout.Height(25)))
        {
            string path = EditorUtility.SaveFilePanel("Save Commands as Text File", "", "Commands.txt", "txt");
            if (!string.IsNullOrEmpty(path))
            {
                List<string> commandTexts = new List<string>();
                foreach (var commandInfo in commandDisplayList)
                {
                    commandTexts.Add(commandInfo.CommandText);
                }
                File.WriteAllLines(path, commandTexts);
                Debug.Log("Exported at: " + path);
            }
        }

    }


// Write the changes back to AtomicCommands.cs
 private void ApplyChangesToAtomicCommands()
{
    string path = "Assets/Atomic Console/Scripts/AtomicCommands.cs";
    if (File.Exists(path))
    {
        string[] lines = File.ReadAllLines(path);
        int commandMethodsStartIndex = Array.FindIndex(lines, line => line.Contains("public static List<MethodInfo> commandMethods = new List<MethodInfo>"));
        commandMethodsStartIndex += 2; // Skip the opening brace '{' line

        for (int i = 0; i < commandDisplayList.Count; i++)
        {
            CommandDisplayInfo commandInfo = commandDisplayList[i];
            string line = lines[commandMethodsStartIndex + i];
            lines[commandMethodsStartIndex + i] = UpdateLine(line, commandInfo.IsToggled);
        }

        File.WriteAllLines(path, lines);
        UnityEditor.AssetDatabase.Refresh(); // Refresh the asset database to reflect the changes
    }
}
private string UpdateLine(string line, bool isToggled)
{
    if (isToggled)
    {
        // Uncomment the line if it's commented
        if (line.TrimStart().StartsWith("//"))
        {
            return line.Substring(3);  // Remove the leading "// "
        }
    }
    else
    {
        // Comment the line if it's not already commented
        if (!line.TrimStart().StartsWith("//"))
        {
            return "// " + line;
        }
    }
    return line;
}





}

public class CommandDisplayInfo
{
    public string CommandText { get; set; }
    public string FieldType { get; set; }
    public bool IsLocked { get; set; }
    public bool StaticMethod { get; set; }
    public bool IsToggled { get; set; } 

}

}