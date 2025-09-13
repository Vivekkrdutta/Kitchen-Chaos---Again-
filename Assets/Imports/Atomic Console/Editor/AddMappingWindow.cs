using UnityEditor;
using UnityEngine;
using AtomicConsole.Mapping.editor;
using AtomicMapping;

namespace AtomicConsole.Mapping.tool
{


public class AddMappingWindow : EditorWindow
{
    private string key = "";
    private string value = "";
    private bool shouldClose = false;

    public static void ShowWindow()
    {
        AddMappingWindow window = ScriptableObject.CreateInstance<AddMappingWindow>();
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 300, 100);
        window.maxSize = new Vector2(400, 150);
        window.minSize = new Vector2(400, 150);
        window.ShowUtility();
    }

    private void OnGUI()
    {
        //EditorGUILayout.LabelField("Add Custom Mapping", EditorStyles.boldLabel);

        EditorGUILayout.Space(5);
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label) 
        { 
            fontSize = 17, 
            fontStyle = FontStyle.Bold, 
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(0, 0, 0, -10) 
        };
        EditorGUILayout.LabelField("Add Custom Mapping", titleStyle);
        EditorGUILayout.Space(10);

        key = EditorGUILayout.TextField("Type Name", key);
        EditorGUILayout.Space(5);
        value = EditorGUILayout.TextField("Mapped Name", value);

        EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Cancel", GUILayout.Height(25)))
        {
            shouldClose = true;
        }

        if (GUILayout.Button("OK", GUILayout.Height(25)))
        {
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                AtomicTypeMapping.TypeMappings[key] = value;
                AtomicMappingEditor.customMappings.Add(key);
                shouldClose = true;
            }
        }

        if (shouldClose)
        {
            Close();
        }
        EditorGUILayout.EndHorizontal();
    }
}

}