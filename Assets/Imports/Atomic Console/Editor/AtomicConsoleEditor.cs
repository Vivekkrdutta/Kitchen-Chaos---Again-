using UnityEditor;
using UnityEngine;
using AtomicConsole.Skin.editor;

namespace AtomicConsole.Engine.editor
{

[CustomEditor(typeof(AtomicConsoleEngine))]
public class AtomicConsoleEditor : Editor
{
    private bool showPicker;
    private bool showPassword;
    private string previousLogin;
    private string previousPassword;
    private bool isPasswordApplied = false;
    private string statusMessage;
    private bool previousCanSelectLog;
    private bool previousSleepMode;
    private bool previousAlwaysOnFocus;
    private bool previousDontDestroyOnLoad;

    private bool firstRun = true;

    public override void OnInspectorGUI()
    {
        AtomicConsoleEngine devConsole = (AtomicConsoleEngine)target;

        //Retrieve stored EditorPrefs
        isPasswordApplied = EditorPrefs.GetBool("IsPasswordApplied", false);

        if (firstRun)
        {
            devConsole.password = EditorPrefs.GetString("pass", "");
            
            //devConsole.CommmadStartLine = (AtomicConsoleEngine.CommandStartEnum)EditorPrefs.GetInt("CommandStartLine", (int)AtomicConsoleEngine.CommandStartEnum.Null);
            
            devConsole.invidiualLogs = EditorPrefs.GetBool("CanSelectLog", false);
            previousCanSelectLog = devConsole.invidiualLogs; 

            devConsole.SleepMode = EditorPrefs.GetBool("SleepMode", false);
            previousSleepMode = devConsole.SleepMode; 

            devConsole.AlwaysOnFocus = EditorPrefs.GetBool("OnFocus", false);
            previousAlwaysOnFocus = devConsole.AlwaysOnFocus; 

            devConsole.dontDestroyOnLoad = EditorPrefs.GetBool("DontDestroyOnLoad", false);
            previousDontDestroyOnLoad = devConsole.dontDestroyOnLoad; 
            
            firstRun = false;
        }
        
        serializedObject.Update();

        // Custom Header
        EditorGUILayout.Space(5);
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label) 
        { 
            fontSize = 20, 
            fontStyle = FontStyle.Bold, 
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(0, 0, 0, -10) 
        };
        EditorGUILayout.LabelField("Atomic Console", titleStyle);
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // Skin field
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Console Skin", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();

        GUI.enabled = false;
        EditorGUILayout.ObjectField("Selected Skin", devConsole.skin, typeof(AtomicSkin), false, GUILayout.Height(20));
        GUI.enabled = true;

        if (GUILayout.Button("Select", EditorStyles.miniButtonLeft, GUILayout.Width(60), GUILayout.Height(20)))
        {
            EditorGUIUtility.ShowObjectPicker<AtomicSkin>(null, false, "", 1);
            showPicker = true;
        }

        GUI.backgroundColor = Color.red;

        if (GUILayout.Button("Remove", EditorStyles.miniButtonRight, GUILayout.Width(60), GUILayout.Height(20)))
        {
            devConsole.skin = null;
        }

        GUI.backgroundColor = Color.white;

        if (showPicker && Event.current.commandName == "ObjectSelectorClosed")
        {
            devConsole.skin = (AtomicSkin)EditorGUIUtility.GetObjectPickerObject();
            showPicker = false;
            EditorUtility.SetDirty(target);
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

      // Password Protection
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.Space(5);

        GUIContent passwordLabel = new GUIContent(" Info", EditorGUIUtility.IconContent("_Help").image);
        EditorGUILayout.LabelField(passwordLabel, EditorStyles.boldLabel);

        string passwordInfo = "Enable password protection for high-level commands to ensure " +
                             "they can only be accessed by authorized users.";
        EditorGUILayout.LabelField(passwordInfo, EditorStyles.wordWrappedLabel);

        EditorGUILayout.Space();

        devConsole.password_Protected = EditorGUILayout.ToggleLeft("Enable Password Protection", devConsole.password_Protected);
    
        if (devConsole.password_Protected)
        {
            EditorGUILayout.BeginHorizontal();
            string newPassword = showPassword ? EditorGUILayout.TextField("Password", devConsole.password) : EditorGUILayout.PasswordField("Password", devConsole.password);

            if(newPassword != devConsole.password)
            {
                devConsole.password = newPassword;
                isPasswordApplied = false;
                EditorPrefs.SetBool("IsPasswordApplied", isPasswordApplied);
            }


            if (GUILayout.Button(showPassword ? "Hide" : "Show", GUILayout.Width(60)))
            {
                showPassword = !showPassword;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Apply", GUILayout.Width(60)))
            {
                if (string.IsNullOrEmpty(devConsole.password) || devConsole.password.Length < 4)
                {
                    statusMessage = "Please update";
                    if(devConsole.password.Length < 4)
                        EditorUtility.DisplayDialog("Error", "Password must be at least 4 characters long.", "Ok");
                }
                else
                {
                    // Save to EditorPrefs
                    EditorPrefs.SetString("pass", devConsole.password);
                    isPasswordApplied = true;
                    EditorPrefs.SetBool("IsPasswordApplied", isPasswordApplied);
                    statusMessage = "Updated";
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorStyles.centeredGreyMiniLabel.normal.textColor = Color.gray;
            string statusMessageShow = isPasswordApplied ? statusMessage + " ✓" : statusMessage;
            EditorGUILayout.LabelField(statusMessageShow, EditorStyles.centeredGreyMiniLabel);
            EditorStyles.centeredGreyMiniLabel.normal.textColor = Color.gray;
            GUILayout.Space(5);

            if (isPasswordApplied)
            {
                statusMessage = "Updated";
                EditorGUILayout.LabelField($"To unlock protected commands, type in console:", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.LabelField($"[Command] '>>'  Your Password:{new string('*', devConsole.password.Length)}", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.Space(5);
            }
            else
            {
                statusMessage = "Please update";
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.Space(5);
            }

        }
        else
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        GUIContent supportLabel8 = new GUIContent(" Info", EditorGUIUtility.IconContent("_Help").image);
        EditorGUILayout.LabelField(supportLabel8, EditorStyles.boldLabel);
        string info9 = "Key input to toggle the Atomic Console. Note: Using the 'Tab' key may cause bugs.";
        EditorGUILayout.LabelField(info9, EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space(5);


        //devConsole.CommmadStartLine = (AtomicConsoleEngine.CommandStartEnum)EditorGUILayout.EnumPopup("Custom Log Type", devConsole.CommmadStartLine);
        devConsole.ToggleConsoleInput = (KeyCode)EditorGUILayout.EnumPopup("Toggle Console Input", devConsole.ToggleConsoleInput);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        //TODO next update
        /*GUIContent supportLabel2 = new GUIContent(" Info", EditorGUIUtility.IconContent("_Help").image);
        EditorGUILayout.LabelField(supportLabel2, EditorStyles.boldLabel);
        string info3 = "The Password Protection Command will not affect this property.";
        EditorGUILayout.LabelField(info3, EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space(5);

        //devConsole.CommmadStartLine = (AtomicConsoleEngine.CommandStartEnum)EditorGUILayout.EnumPopup("Custom Log Type", devConsole.CommmadStartLine);
        AtomicConsoleEngine.CommandStartEnum newEnumValue = (AtomicConsoleEngine.CommandStartEnum)EditorGUILayout.EnumPopup("Command Start Line", devConsole.CommmadStartLine);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); */
        EditorGUILayout.Space(5);

        GUIContent supportLabel3 = new GUIContent(" Warning", EditorGUIUtility.IconContent("console.warnicon.inactive.sml").image);
        EditorGUILayout.LabelField(supportLabel3, EditorStyles.boldLabel);
        string info4 = "Outputing Invidiual logs can drastically reduce performance.";
        EditorGUILayout.LabelField(info4, EditorStyles.wordWrappedLabel);

        EditorGUILayout.Space(5);
        devConsole.invidiualLogs = EditorGUILayout.Toggle("Invidiual Logs", devConsole.invidiualLogs);
        EditorGUILayout.Space(5);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);


        GUIContent supportLabel5 = new GUIContent(" Info", EditorGUIUtility.IconContent("_Help").image);
        EditorGUILayout.LabelField(supportLabel5, EditorStyles.boldLabel);
        string info6 = "The focus (Caret) will always be on the Input Text field of the console.";
        EditorGUILayout.LabelField(info6, EditorStyles.wordWrappedLabel);

        EditorGUILayout.Space(5);
        devConsole.AlwaysOnFocus = EditorGUILayout.Toggle("Always On Focus", devConsole.AlwaysOnFocus);
        EditorGUILayout.Space(5);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);



        GUIContent supportLabel6 = new GUIContent(" Info", EditorGUIUtility.IconContent("_Help").image);
        EditorGUILayout.LabelField(supportLabel6, EditorStyles.boldLabel);
        string info7 = "[Atomic Console] will persist across scene changes.";
        EditorGUILayout.LabelField(info7, EditorStyles.wordWrappedLabel);

        EditorGUILayout.Space(5);
        devConsole.dontDestroyOnLoad = EditorGUILayout.Toggle("Dont Destroy On Load", devConsole.dontDestroyOnLoad);
        EditorGUILayout.Space(5);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);




        GUIContent supportLabel4 = new GUIContent(" Info", EditorGUIUtility.IconContent("_Help").image);
        EditorGUILayout.LabelField(supportLabel4, EditorStyles.boldLabel);
        string info5 = "Sleep mode will make console collect all logs, even if console is not displayed.";
        EditorGUILayout.LabelField(info5, EditorStyles.wordWrappedLabel);

        EditorGUILayout.Space(5);
        devConsole.SleepMode = EditorGUILayout.Toggle("Sleep Mode", devConsole.SleepMode);
        EditorGUILayout.Space(5);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);



        //save prefs
        if (devConsole.SleepMode != previousSleepMode)
        {
            // Save to EditorPrefs
            EditorPrefs.SetBool("SleepMode", devConsole.SleepMode);

            // Update the previous state
            previousSleepMode = devConsole.SleepMode;
        }


        if (devConsole.AlwaysOnFocus != previousAlwaysOnFocus)
        {
            // Save to EditorPrefs
            EditorPrefs.SetBool("OnFocus", devConsole.AlwaysOnFocus);

            // Update the previous state
            previousAlwaysOnFocus = devConsole.AlwaysOnFocus;
        }


        if (devConsole.dontDestroyOnLoad != previousDontDestroyOnLoad)
        {
            // Save to EditorPrefs
            EditorPrefs.SetBool("DontDestroyOnLoad", devConsole.dontDestroyOnLoad);

            // Update the previous state
            previousDontDestroyOnLoad = devConsole.dontDestroyOnLoad;
        }


        if (devConsole.invidiualLogs != previousCanSelectLog)
        {
            // Save to EditorPrefs
            EditorPrefs.SetBool("CanSelectLog", devConsole.invidiualLogs);

            // Update the previous state
            previousCanSelectLog = devConsole.invidiualLogs;
        }


        // Check if the enum value has changed
       /* if (newEnumValue != devConsole.CommmadStartLine)
        {
            // Update the enum value in the object
            devConsole.CommmadStartLine = newEnumValue;

            // Save the new enum value to EditorPrefs
            EditorPrefs.SetInt("CommandStartLine", (int)newEnumValue);
        } */






        EditorGUILayout.Space(5);
        // Contact field
        GUIContent supportLabel = new GUIContent(" Support", EditorGUIUtility.IconContent("_Help").image);
        EditorGUILayout.LabelField(supportLabel, EditorStyles.boldLabel);

        string supportText = "For any issues, concerns, or bugs encountered with the [Atomic Console], " +
                             "please don't hesitate to reach out for support. You can directly contact " +
                             "us by clicking the 'Contact' button below.";

        GUIStyle centeredStyle = new GUIStyle(EditorStyles.wordWrappedLabel) { alignment = TextAnchor.MiddleCenter };
        EditorGUILayout.LabelField(supportText, centeredStyle);

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Contact", GUILayout.Width(70), GUILayout.Height(20)))
        {
            Application.OpenURL("mailto:gospodinsime@outlook.com?subject=DevConsoleHelp");
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(8);
        
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Version field
        EditorGUILayout.LabelField("Version 1.0.0", EditorStyles.centeredGreyMiniLabel);

        //devConsole.scrollPos = EditorGUILayout.Vector2Field("scrollPos", devConsole.scrollPos);
        //devConsole.scrollPosSmoothnes = EditorGUILayout.FloatField("Smoothnes", devConsole.scrollPosSmoothnes);
        if(devConsole.consoleLogs != null)
        {
            EditorGUILayout.LabelField("Console Logs");
            for (int i = 0; i < devConsole.consoleLogs.Count; i++)
            {
                devConsole.consoleLogs[i] = EditorGUILayout.TextField($"Log {i+1}", devConsole.consoleLogs[i]);
            }
        }
        devConsole.isVisible = EditorGUILayout.Toggle("Console Visible", devConsole.isVisible);
        serializedObject.ApplyModifiedProperties();
    }
}

}