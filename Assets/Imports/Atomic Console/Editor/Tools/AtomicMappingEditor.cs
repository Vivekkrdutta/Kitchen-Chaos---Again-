using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;
using AtomicConsole.Mapping;
using AtomicConsole.Mapping.tool;
using AtomicMapping;
namespace AtomicConsole.Mapping.editor
{

public class AtomicMappingEditor : EditorWindow
{  
    private Vector2 scrollPos;
    public static HashSet<string> customMappings = new HashSet<string>();

    [MenuItem("Tools/Atomic Console/TypeMapping", false, 3)]
    public static void ShowWindow()
    {
        AtomicMappingEditor window = GetWindow<AtomicMappingEditor>("TypeMapping");
        window.minSize = new Vector2(250, 400);
    }

    private void OnGUI()
    {
        // Custom Header
        EditorGUILayout.Space(5);
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label) 
        { 
            fontSize = 20, 
            fontStyle = FontStyle.Bold, 
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(0, 0, 0, -10) 
        };
        EditorGUILayout.LabelField("Atomic TypeMapping Editor", titleStyle);
        
        EditorGUILayout.Space(10);
         // Reset to Default button near the header
        if (GUILayout.Button("Reset", GUILayout.Height(20), GUILayout.Width(80)))
        {
            AtomicTypeMapping.TypeMappings = new Dictionary<string, string>
            {
                {"Single", "Float"},
                {"Int32", "Int"},
                {"Int16", "Short"},
                {"Int64", "Long"},
                {"UInt32", "Uint"},
                {"UInt16", "Ushort"},
                {"UInt64", "Ulong"},
                {"Boolean", "Bool"},
                {"String", "String"},
                {"Char", "Char"},
                {"Byte", "Byte"},
                {"SByte", "Sbyte"},
                {"Double", "Double"},
                {"Decimal", "Decimal"},
                {"Object", "Object"},
                {"Vector2", "Vector2"},
                {"Vector3", "Vector3"},
                {"Vector4", "Vector4"},
                {"Quaternion", "Quaternion"},
                {"Color", "Color"},
                {"Rect", "Rect"},
                {"Transform", "Transform"},
                {"GameObject", "GameObject"},
                {"Sprite", "Sprite"},
                {"Texture2D", "Texture2D"},
                {"AudioClip", "AudioClip"},
                {"AnimationClip", "AnimationClip"},
                {"Rigidbody", "Rigidbody"},
                {"Rigidbody2D", "Rigidbody2D"},
                {"Collider", "Collider"},
                {"Collider2D", "Collider2D"},
                {"Material", "Material"},
                {"Shader", "Shader"},
                {"Mesh", "Mesh"},
                {"Camera", "Camera"},
                {"Light", "Light"},
                {"Animator", "Animator"},
                {"Canvas", "Canvas"},
                {"RectTransform", "RectTransform"},
                {"Text", "Text"},
                {"Image", "Image"},     
            };
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(10);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        Dictionary<string, string> tempMappings = new Dictionary<string, string>(AtomicTypeMapping.TypeMappings);
        List<string> keysToRemove = new List<string>();  // To keep track of keys to remove

        foreach (var key in AtomicTypeMapping.TypeMappings.Keys)
        {
            EditorGUILayout.BeginHorizontal();
            string value = AtomicTypeMapping.TypeMappings[key];
            tempMappings[key] = EditorGUILayout.TextField(key, value);

            // Add a "Delete" button for every mapping
            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                keysToRemove.Add(key);  // Mark this key for removal
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(3);
        }

        // Remove the marked keys
        foreach (var key in keysToRemove)
        {
            tempMappings.Remove(key);
        }

        AtomicTypeMapping.TypeMappings = tempMappings;

        EditorGUILayout.EndScrollView();

        // Footer Buttons
        EditorGUILayout.Space(15);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add", GUILayout.Height(30)))
        {
            AddMappingWindow.ShowWindow();
        }

        if (GUILayout.Button("Save", GUILayout.Height(30)))
        {
            SaveMapping();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(15);
    }



    private void SaveMapping()
{
    StringBuilder sb = new StringBuilder();
    sb.AppendLine("using System;");
    sb.AppendLine("using System.Collections.Generic;");
    sb.AppendLine("namespace AtomicMapping"); 
    sb.AppendLine("{");
    sb.AppendLine("    public static class AtomicTypeMapping"); 
    sb.AppendLine("    {");
    sb.AppendLine("        public static Dictionary<string, string> TypeMappings = new Dictionary<string, string>");
    sb.AppendLine("        {");

    foreach (var pair in AtomicTypeMapping.TypeMappings)
    {
        sb.AppendLine($"            {{\"{pair.Key}\", \"{pair.Value}\"}},");
    }

    sb.AppendLine("        };");
    
    // Added GetType function
    sb.AppendLine("        public static string GetType(Type type)");
    sb.AppendLine("        {");
    sb.AppendLine("            string mappedType;");
    sb.AppendLine("            if (TypeMappings.TryGetValue(type.Name, out mappedType))");
    sb.AppendLine("            {");
    sb.AppendLine("                return mappedType;");
    sb.AppendLine("            }");
    sb.AppendLine("            return type.Name;");
    sb.AppendLine("        }");
    sb.AppendLine("    }");
    sb.AppendLine("}");

    string path = "Assets/Atomic Console/Scripts/AtomicTypeMapping.cs";  // Replace with the actual path
    File.WriteAllText(path, sb.ToString());

    // Refresh the asset database to reflect changes
    UnityEditor.AssetDatabase.Refresh();
}

}

}