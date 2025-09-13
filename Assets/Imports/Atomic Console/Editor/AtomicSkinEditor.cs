using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using AtomicConsole.Skin.editor;

namespace AtomicConsole.engine.editor
{

[CustomEditor(typeof(AtomicSkin))]
public class AtomicSkinEditor : Editor
{
    // Static dictionary to keep track of which skins are dirty
    private static Dictionary<int, bool> dirtyStates = new Dictionary<int, bool>();

    //bool isDirty = false;
    bool showLogStyle;
    bool showHeaderStyle;
    bool showInputStyle;
    bool showButtonStyle;
    bool showOutputStyle;
    bool showCommandStyle;
    bool showCloseButtonStyle;
    bool showScrollbarStyle;
    bool showVerticalScrollbarStyle;
    bool showHorizontalScrollbarStyle;
    bool ShowMoreLogs;

    public override void OnInspectorGUI()
    {
        AtomicSkin AtomicSkin = (AtomicSkin)target;   

         // Determine if the current skin is dirty
        bool isDirty;
        int instanceID = AtomicSkin.GetInstanceID();
        if (!dirtyStates.TryGetValue(instanceID, out isDirty))
        {
            isDirty = false;  // Default state
        }
        
        //to center labels or headers
        GUIStyle centeredLabelStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 15,
            alignment = TextAnchor.MiddleCenter
        };

        //title
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label) 
        { 
            fontSize = 20, 
            fontStyle = FontStyle.Bold, 
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(0, 0, 0, -10) 
        };


        // Custom Header
        EditorGUILayout.LabelField("Atomic Console Skin Editor", titleStyle);
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        //check changes
        EditorGUI.BeginChangeCheck();




        //Text Editor
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Text Editor", centeredLabelStyle);
        GUILayout.Space(10);

        AtomicSkin.Header = EditorGUILayout.TextField("Header Title", AtomicSkin.Header);

        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        AtomicSkin.Button_Send = EditorGUILayout.TextField("Button Send", AtomicSkin.Button_Send, GUILayout.ExpandWidth(true));
        AtomicSkin.Show_Send_Button = EditorGUILayout.Toggle(AtomicSkin.Show_Send_Button, GUILayout.Width(15));
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        AtomicSkin.Button_Clear = EditorGUILayout.TextField("Button Clear", AtomicSkin.Button_Clear, GUILayout.ExpandWidth(true));
        AtomicSkin.Show_Clear_Button = EditorGUILayout.Toggle(AtomicSkin.Show_Clear_Button, GUILayout.Width(15));
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        AtomicSkin.CloseButton_Style.text = EditorGUILayout.TextField("Button Close", AtomicSkin.CloseButton_Style.text, GUILayout.ExpandWidth(true));
        AtomicSkin.ShowCloseBtn = EditorGUILayout.Toggle(AtomicSkin.ShowCloseBtn, GUILayout.Width(15));
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);
        AtomicSkin.font = (Font)EditorGUILayout.ObjectField("Global Font", AtomicSkin.font, typeof(Font), false);

        GUILayout.Space(5);

        AtomicSkin.logPrefixType = (AtomicSkin.LogPrefixType)EditorGUILayout.EnumPopup("Custom Log Type", AtomicSkin.logPrefixType);

        if(AtomicSkin.logPrefixType == AtomicSkin.LogPrefixType.Custom)
        {
            GUILayout.Space(5);
            AtomicSkin.logPrefix = EditorGUILayout.TextField("Custom Log", AtomicSkin.logPrefix);
        }

        GUILayout.Space(5);
        AtomicSkin.logTypesToShow = (AtomicSkin.LogTypes)EditorGUILayout.EnumFlagsField("Log Type Prefix", AtomicSkin.logTypesToShow);

        GUILayout.Space(5);
        AtomicSkin.CaretSpeed = EditorGUILayout.FloatField("Caret Speed", AtomicSkin.CaretSpeed);
       
        GUILayout.Space(10);
        EditorGUILayout.EndVertical();


        //add space between sections
        GUILayout.Space(10);

        //options
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Options", centeredLabelStyle);
        GUILayout.Space(10);

        AtomicSkin.windowType = (AtomicSkin.WindowType)EditorGUILayout.EnumPopup("Window Type", AtomicSkin.windowType);
        GUILayout.Space(5);
        if(AtomicSkin.windowType == AtomicSkin.WindowType.Draggable)
        {
            GUILayout.Space(5);
            AtomicSkin.CanResize = EditorGUILayout.Toggle("Can Resize Window", AtomicSkin.CanResize);

             if(AtomicSkin.CanResize) 
             {
                GUILayout.Space(5);
                AtomicSkin.MinWinSize = Vector2IntField("Min Window Size", AtomicSkin.MinWinSize);
                GUILayout.Space(2);
                AtomicSkin.MaxWinSize = Vector2IntField("Max Window Size", AtomicSkin.MaxWinSize);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
             }
             else
             {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
             }
        }

        AtomicSkin.MaxCommandHistory = EditorGUILayout.IntSlider("Max Log Output", AtomicSkin.MaxCommandHistory, 10, 300);
        if(AtomicSkin.MaxCommandHistory > 100) //throw warnign
        {
            GUILayout.Space(5);
            GUIContent label = new GUIContent(" Warning", EditorGUIUtility.IconContent("console.warnicon.inactive.sml").image);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            string labelInfo = "Setting Max Log Output to a value greater than 100 may impact performance and potentially cause the game to crash or experience lag. Please use caution and consider the capabilities of your target hardware.";
            EditorGUILayout.LabelField(labelInfo, EditorStyles.wordWrappedLabel);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(5);
        }
        else
        {
            GUILayout.Space(5);
        }

        AtomicSkin.MaxSuggestion = EditorGUILayout.IntSlider("Max Suggestions Output", AtomicSkin.MaxSuggestion, 1, 10);

        GUILayout.Space(5);

        AtomicSkin.ShowArgName = EditorGUILayout.Toggle("Show Arguments Type", AtomicSkin.ShowArgName);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        AtomicSkin.SelectionColor = EditorGUILayout.ColorField("Selection Color", AtomicSkin.SelectionColor);
        AtomicSkin.CaretColor = EditorGUILayout.ColorField("Caret Color", AtomicSkin.CaretColor);
        GUILayout.Space(5);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        AtomicSkin.Output_Command = EditorGUILayout.ColorField("Output Command", AtomicSkin.Output_Command);
        GUILayout.Space(5);
        AtomicSkin.Output_Log = EditorGUILayout.ColorField("Output Log", AtomicSkin.Output_Log);
        GUILayout.Space(5);
        AtomicSkin.Output_Warning = EditorGUILayout.ColorField("Output Warning", AtomicSkin.Output_Warning);
        GUILayout.Space(5);
        AtomicSkin.Output_Error = EditorGUILayout.ColorField("Output Error", AtomicSkin.Output_Error);
        GUILayout.Space(5);
        AtomicSkin.Output_Exception = EditorGUILayout.ColorField("Output Exception", AtomicSkin.Output_Exception);

        EditorGUILayout.Space(5);

        ShowMoreLogs = GUILayout.Toggle(ShowMoreLogs, "Edvanced", "button", GUILayout.Height(25));
            if(ShowMoreLogs)
            {
                GUILayout.Space(5);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                AtomicSkin.Output_CommandWarning = EditorGUILayout.ColorField("Output Command Warning", AtomicSkin.Output_CommandWarning);
                GUILayout.Space(5);
                AtomicSkin.Output_CommandError = EditorGUILayout.ColorField("Output Command Error", AtomicSkin.Output_CommandError);
                GUILayout.Space(5);
                AtomicSkin.Output_CommandException = EditorGUILayout.ColorField("Output Command Exception", AtomicSkin.Output_CommandException);
                GUILayout.Space(5);
                AtomicSkin.Output_Console = EditorGUILayout.ColorField("Output Console", AtomicSkin.Output_Console);
                GUILayout.Space(5);
                AtomicSkin.Output_Network = EditorGUILayout.ColorField("Output Network", AtomicSkin.Output_Network);
                GUILayout.Space(5);
                AtomicSkin.Output_NetworkError = EditorGUILayout.ColorField("Output Network Error", AtomicSkin.Output_NetworkError);
                GUILayout.Space(5);
                AtomicSkin.Output_NetworkWarning = EditorGUILayout.ColorField("Output Network Warning", AtomicSkin.Output_NetworkWarning);
                GUILayout.Space(5);
                AtomicSkin.Output_Object = EditorGUILayout.ColorField("Output Object", AtomicSkin.Output_Object);
                GUILayout.Space(5);
                AtomicSkin.Output_Material = EditorGUILayout.ColorField("Output Material", AtomicSkin.Output_Material);
                GUILayout.Space(5);
                AtomicSkin.Output_Info = EditorGUILayout.ColorField("Output Info", AtomicSkin.Output_Info);
                GUILayout.Space(5);
                AtomicSkin.Output_System = EditorGUILayout.ColorField("Output System", AtomicSkin.Output_System);
                GUILayout.Space(5);
                AtomicSkin.Output_Audio = EditorGUILayout.ColorField("Output Audio", AtomicSkin.Output_Audio);
                GUILayout.Space(5);
                AtomicSkin.Output_Critical = EditorGUILayout.ColorField("Output Critical", AtomicSkin.Output_Critical);
                GUILayout.Space(5);
                AtomicSkin.Output_GameState = EditorGUILayout.ColorField("Output Game State", AtomicSkin.Output_GameState);
                GUILayout.Space(5);
                AtomicSkin.Output_Physics = EditorGUILayout.ColorField("Output Physics", AtomicSkin.Output_Physics);
                GUILayout.Space(5);
                AtomicSkin.Output_AI = EditorGUILayout.ColorField("Output AI", AtomicSkin.Output_AI);
                GUILayout.Space(5);
                AtomicSkin.Output_Input = EditorGUILayout.ColorField("Output Input", AtomicSkin.Output_Input);


                EditorGUILayout.Space(10);
            }
            else
                EditorGUILayout.Space(10);
        EditorGUILayout.EndVertical();


         //add space between sections
        GUILayout.Space(10);
        //gui style
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("GUI", centeredLabelStyle);
        GUILayout.Space(10);

        EditorGUILayout.BeginVertical();

        // Display the info icon and title
        GUIContent infoLabel = new GUIContent(" Info", EditorGUIUtility.IconContent("_Help").image);
        EditorGUILayout.LabelField(infoLabel, EditorStyles.boldLabel);

        // Display the warning message
        string warningMessage = "Enabling the real-time GUI editor can cause crashes.";
        EditorGUILayout.LabelField(warningMessage, EditorStyles.wordWrappedLabel);

        // Add some space before the toggle
        EditorGUILayout.Space(5);

        // Display the toggle with a label and a tooltip
        AtomicSkin.ChangeInRealTime = EditorGUILayout.Toggle(
            new GUIContent("Real-time GUI Editing", "Toggle this to edit the GUI in real-time. Use with caution."), 
            AtomicSkin.ChangeInRealTime
        );

        EditorGUILayout.EndVertical();

       
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        showHeaderStyle = GUILayout.Toggle(showHeaderStyle, "Header Style", "button", GUILayout.Height(25));
            if(showHeaderStyle)
            {
                GUILayout.Space(5);
                AtomicSkin.Header_Style.font_style = (FontStyle)EditorGUILayout.EnumPopup("Font Style", AtomicSkin.Header_Style.font_style);
                GUILayout.Space(5);
                AtomicSkin.Header_Style.font_color = EditorGUILayout.ColorField("Font Color", AtomicSkin.Header_Style.font_color);
                GUILayout.Space(5);
                AtomicSkin.Header_Style.alignment = (TextAnchor)EditorGUILayout.EnumPopup("Alignment", AtomicSkin.Header_Style.alignment);
                GUILayout.Space(5);
                AtomicSkin.Header_Style.font_size = EditorGUILayout.IntField("Font Size", AtomicSkin.Header_Style.font_size);

                GUILayout.Space(5);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                GUILayout.Space(5);
                AtomicSkin.Header_Style.HeaderSize = EditorGUILayout.IntField("Header Height", AtomicSkin.Header_Style.HeaderSize);
                GUILayout.Space(5);
                AtomicSkin.Header_Style.border = EditorGUILayout.Vector4Field("Image border", AtomicSkin.Header_Style.border);
                GUILayout.Space(5);
                AtomicSkin.Header_Style.background = (Texture2D)EditorGUILayout.ObjectField("Background", AtomicSkin.Header_Style.background, typeof(Texture2D), false);
                GUILayout.Space(5);
                AtomicSkin.Header_Style.on_background = (Texture2D)EditorGUILayout.ObjectField("On Normal Background", AtomicSkin.Header_Style.on_background, typeof(Texture2D), false);
                GUILayout.Space(5);
                AtomicSkin.Header_Style.transparent = EditorGUILayout.Slider("Transparency", AtomicSkin.Header_Style.transparent, 0, 1);
                 GUILayout.Space(5);
                AtomicSkin.Header_Style.background_color = EditorGUILayout.ColorField("Background Color", AtomicSkin.Header_Style.background_color);
                GUILayout.Space(5);
                AtomicSkin.Header_Style.onNormal_background_color = EditorGUILayout.ColorField("On Normal Background Color", AtomicSkin.Header_Style.onNormal_background_color);
                GUILayout.Space(5);
                AtomicSkin.Header_Style.background_position = (ImagePosition)EditorGUILayout.EnumPopup("Background Position", AtomicSkin.Header_Style.background_position);
                GUILayout.Space(5);
                AtomicSkin.Header_Style.text_clip = (TextClipping)EditorGUILayout.EnumPopup("Text Clipping", AtomicSkin.Header_Style.text_clip);
                GUILayout.Space(5);
                AtomicSkin.Header_Style.content_offset = EditorGUILayout.Vector2Field("Content Offset", AtomicSkin.Header_Style.content_offset);
                GUILayout.Space(5);
                AtomicSkin.Header_Style.WH = EditorGUILayout.Vector2Field("Width & Height", AtomicSkin.Header_Style.WH);
                GUILayout.Space(5);
                AtomicSkin.Header_Style.margin = EditorGUILayout.Vector4Field("Margin", AtomicSkin.Header_Style.margin);
                GUILayout.Space(5);
                AtomicSkin.Header_Style.padding = EditorGUILayout.Vector4Field("Padding", AtomicSkin.Header_Style.padding);
                GUILayout.Space(5);
                AtomicSkin.Header_Style.overflow = EditorGUILayout.Vector4Field("Overflow", AtomicSkin.Header_Style.overflow);     
                /*AtomicSkin.Header_Style.useWindowPosition = EditorGUILayout.Toggle("Use WindowStyle Rect", AtomicSkin.Header_Style.useWindowPosition);
                if(AtomicSkin.Header_Style.useWindowPosition)
                {
                    AtomicSkin.Header_Style.background_position = AtomicSkin.Log_Style.background_position;
                    AtomicSkin.Header_Style.text_clip = AtomicSkin.Log_Style.text_clip;
                    AtomicSkin.Header_Style.content_offset = AtomicSkin.Log_Style.content_offset;
                    AtomicSkin.Header_Style.margin = AtomicSkin.Log_Style.margin;
                    AtomicSkin.Header_Style.padding = AtomicSkin.Log_Style.padding;
                    AtomicSkin.Header_Style.overflow = AtomicSkin.Log_Style.overflow;       
                }
                else
                {
                    AtomicSkin.Header_Style.background_position = (ImagePosition)EditorGUILayout.EnumPopup("Background Position", AtomicSkin.Header_Style.background_position);
                    GUILayout.Space(5);
                    AtomicSkin.Header_Style.text_clip = (TextClipping)EditorGUILayout.EnumPopup("Text Clipping", AtomicSkin.Header_Style.text_clip);
                    GUILayout.Space(5);
                    AtomicSkin.Header_Style.content_offset = EditorGUILayout.Vector2Field("Content Offset", AtomicSkin.Header_Style.content_offset);
                    GUILayout.Space(5);
                    AtomicSkin.Header_Style.WH = EditorGUILayout.Vector2Field("Width & Height", AtomicSkin.Header_Style.WH);
                    GUILayout.Space(5);
                    AtomicSkin.Header_Style.margin = EditorGUILayout.Vector4Field("Margin", AtomicSkin.Header_Style.margin);
                    GUILayout.Space(5);
                    AtomicSkin.Header_Style.padding = EditorGUILayout.Vector4Field("Padding", AtomicSkin.Header_Style.padding);
                    GUILayout.Space(5);
                    AtomicSkin.Header_Style.overflow = EditorGUILayout.Vector4Field("Overflow", AtomicSkin.Header_Style.overflow);     
                }*/

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            }
             else
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            }

        showLogStyle = GUILayout.Toggle(showLogStyle, "Window Style", "button", GUILayout.Height(25));
            if(showLogStyle)
            {


            AtomicSkin.Log_Style.border = EditorGUILayout.Vector4Field("Image border", AtomicSkin.Log_Style.border);
            GUILayout.Space(5);
            AtomicSkin.Log_Style.background = (Texture2D)EditorGUILayout.ObjectField("Background", AtomicSkin.Log_Style.background, typeof(Texture2D), false);
            GUILayout.Space(5);
            AtomicSkin.Log_Style.on_background = (Texture2D)EditorGUILayout.ObjectField("On Normal Background", AtomicSkin.Log_Style.on_background, typeof(Texture2D), false);

            GUILayout.Space(5);

            AtomicSkin.Log_Style.transparent = EditorGUILayout.Slider("Transparency", AtomicSkin.Log_Style.transparent, 0, 1);
            GUILayout.Space(5);
            AtomicSkin.Log_Style.background_color = EditorGUILayout.ColorField("Background Color", AtomicSkin.Log_Style.background_color);
            GUILayout.Space(5);
            AtomicSkin.Log_Style.onNormal_background_color = EditorGUILayout.ColorField("On Normal Background Color", AtomicSkin.Log_Style.onNormal_background_color);
            
            GUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(5);

            AtomicSkin.Log_Style.background_position = (ImagePosition)EditorGUILayout.EnumPopup("Background Position", AtomicSkin.Log_Style.background_position);
            GUILayout.Space(5);
            AtomicSkin.Log_Style.text_clip = (TextClipping)EditorGUILayout.EnumPopup("Text Clipping", AtomicSkin.Log_Style.text_clip);
            GUILayout.Space(5);
            AtomicSkin.Log_Style.content_offset = EditorGUILayout.Vector2Field("Content Offset", AtomicSkin.Log_Style.content_offset);
            GUILayout.Space(5);
            AtomicSkin.Log_Style.margin = EditorGUILayout.Vector4Field("Margin", AtomicSkin.Log_Style.margin);
            GUILayout.Space(5);
            AtomicSkin.Log_Style.padding = EditorGUILayout.Vector4Field("Padding", AtomicSkin.Log_Style.padding);
            GUILayout.Space(5);
            AtomicSkin.Log_Style.overflow = EditorGUILayout.Vector4Field("Overflow", AtomicSkin.Log_Style.overflow);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            }
            else
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            }
        


       


        showCommandStyle = GUILayout.Toggle(showCommandStyle, "Suggestion Style", "button", GUILayout.Height(25));
        if (showCommandStyle)
        {
            GUILayout.Space(5);
            AtomicSkin.Command_Style.font_style = (FontStyle)EditorGUILayout.EnumPopup("Font Style", AtomicSkin.Command_Style.font_style);
            GUILayout.Space(5);
            AtomicSkin.Command_Style.font_color = EditorGUILayout.ColorField("Font Color", AtomicSkin.Command_Style.font_color);
            GUILayout.Space(5);
            AtomicSkin.Command_Style.alignment = (TextAnchor)EditorGUILayout.EnumPopup("Alignment", AtomicSkin.Command_Style.alignment);
            GUILayout.Space(5);
            AtomicSkin.Command_Style.font_size = EditorGUILayout.IntField("Font Size", AtomicSkin.Command_Style.font_size);

            GUILayout.Space(5);
            AtomicSkin.Command_Style.border = EditorGUILayout.Vector4Field("border", AtomicSkin.Command_Style.border);
            GUILayout.Space(5);

            AtomicSkin.Command_Style.background = (Texture2D)EditorGUILayout.ObjectField("Background", AtomicSkin.Command_Style.background, typeof(Texture2D), false);
            GUILayout.Space(5);
            AtomicSkin.Command_Style.on_background = (Texture2D)EditorGUILayout.ObjectField("On Normal Background", AtomicSkin.Command_Style.on_background, typeof(Texture2D), false);

            GUILayout.Space(5);

            AtomicSkin.Command_Style.transparent = EditorGUILayout.Slider("Transparency", AtomicSkin.Command_Style.transparent, 0, 1);

            GUILayout.Space(5);

            AtomicSkin.Command_Style.background_color = EditorGUILayout.ColorField("Background Color", AtomicSkin.Command_Style.background_color);
            GUILayout.Space(5);
            AtomicSkin.Command_Style.onNormal_background_color = EditorGUILayout.ColorField("On Normal Background Color", AtomicSkin.Command_Style.onNormal_background_color);
            GUILayout.Space(5);
            AtomicSkin.Command_Style.background_position = (ImagePosition)EditorGUILayout.EnumPopup("Background Position", AtomicSkin.Command_Style.background_position);
            GUILayout.Space(5);
            AtomicSkin.Command_Style.text_clip = (TextClipping)EditorGUILayout.EnumPopup("Text Clipping", AtomicSkin.Command_Style.text_clip);
            GUILayout.Space(5);
            AtomicSkin.Command_Style.content_offset = EditorGUILayout.Vector2Field("Content Offset", AtomicSkin.Command_Style.content_offset);
            
            GUILayout.Space(5);
            
            AtomicSkin.Command_Style.WH = EditorGUILayout.Vector2Field("Width & Height", AtomicSkin.Command_Style.WH);
            
            GUILayout.Space(5);

            AtomicSkin.Command_Style.margin = EditorGUILayout.Vector4Field("Margin", AtomicSkin.Command_Style.margin);
            GUILayout.Space(5);
            AtomicSkin.Command_Style.padding = EditorGUILayout.Vector4Field("Padding", AtomicSkin.Command_Style.padding);
            GUILayout.Space(5);
            AtomicSkin.Command_Style.overflow = EditorGUILayout.Vector4Field("Overflow", AtomicSkin.Command_Style.overflow);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }
        else
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }



        showInputStyle = GUILayout.Toggle(showInputStyle, "Input Style", "button", GUILayout.Height(25));
        if (showInputStyle)
        {
            GUILayout.Space(5);
            AtomicSkin.Input_Style.font_style = (FontStyle)EditorGUILayout.EnumPopup("Font Style", AtomicSkin.Input_Style.font_style);
            GUILayout.Space(5);
            AtomicSkin.Input_Style.font_color = EditorGUILayout.ColorField("Font Color", AtomicSkin.Input_Style.font_color);
            GUILayout.Space(5);
            AtomicSkin.Input_Style.alignment = (TextAnchor)EditorGUILayout.EnumPopup("Alignment", AtomicSkin.Input_Style.alignment);
            GUILayout.Space(5);
            AtomicSkin.Input_Style.font_size = EditorGUILayout.IntField("Font Size", AtomicSkin.Input_Style.font_size);

            GUILayout.Space(5);
            AtomicSkin.Input_Style.border = EditorGUILayout.Vector4Field("border", AtomicSkin.Input_Style.border);
            GUILayout.Space(5);

            AtomicSkin.Input_Style.background = (Texture2D)EditorGUILayout.ObjectField("Background", AtomicSkin.Input_Style.background, typeof(Texture2D), false);
            GUILayout.Space(5);
            AtomicSkin.Input_Style.on_background = (Texture2D)EditorGUILayout.ObjectField("On Normal Background", AtomicSkin.Input_Style.on_background, typeof(Texture2D), false);

            GUILayout.Space(5);

            AtomicSkin.Input_Style.transparent = EditorGUILayout.Slider("Transparency", AtomicSkin.Input_Style.transparent, 0, 1);

            GUILayout.Space(5);

            AtomicSkin.Input_Style.background_color = EditorGUILayout.ColorField("Background Color", AtomicSkin.Input_Style.background_color);
            GUILayout.Space(5);
            AtomicSkin.Input_Style.onNormal_background_color = EditorGUILayout.ColorField("On Normal Background Color", AtomicSkin.Input_Style.onNormal_background_color);

            GUILayout.Space(5);

            AtomicSkin.Input_Style.background_position = (ImagePosition)EditorGUILayout.EnumPopup("Background Position", AtomicSkin.Input_Style.background_position);
            GUILayout.Space(5);
            AtomicSkin.Input_Style.text_clip = (TextClipping)EditorGUILayout.EnumPopup("Text Clipping", AtomicSkin.Input_Style.text_clip);
            GUILayout.Space(5);
            AtomicSkin.Input_Style.content_offset = EditorGUILayout.Vector2Field("Content Offset", AtomicSkin.Input_Style.content_offset);

            GUILayout.Space(5);
            AtomicSkin.Input_Style.WH = EditorGUILayout.Vector2Field("Width & Height", AtomicSkin.Input_Style.WH);
            GUILayout.Space(5);

            AtomicSkin.Input_Style.margin = EditorGUILayout.Vector4Field("Margin", AtomicSkin.Input_Style.margin);
            GUILayout.Space(5);
            AtomicSkin.Input_Style.padding = EditorGUILayout.Vector4Field("Padding", AtomicSkin.Input_Style.padding);
            GUILayout.Space(5);
            AtomicSkin.Input_Style.overflow = EditorGUILayout.Vector4Field("Overflow", AtomicSkin.Input_Style.overflow);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        }
        else
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }    



         showOutputStyle = GUILayout.Toggle(showOutputStyle, "Output Style", "button", GUILayout.Height(25));
        if (showOutputStyle)
        {
            GUILayout.Space(5);
            AtomicSkin.Output_Style.font_style = (FontStyle)EditorGUILayout.EnumPopup("Font Style", AtomicSkin.Output_Style.font_style);
            GUILayout.Space(5);
            AtomicSkin.Output_Style.font_color = EditorGUILayout.ColorField("Font Color", AtomicSkin.Output_Style.font_color);
            GUILayout.Space(5);
            AtomicSkin.Output_Style.alignment = (TextAnchor)EditorGUILayout.EnumPopup("Alignment", AtomicSkin.Output_Style.alignment);
            GUILayout.Space(5);
            AtomicSkin.Output_Style.font_size = EditorGUILayout.IntField("Font Size", AtomicSkin.Output_Style.font_size);

            GUILayout.Space(5);
            AtomicSkin.Output_Style.border = EditorGUILayout.Vector4Field("border", AtomicSkin.Output_Style.border);
            GUILayout.Space(5);

            AtomicSkin.Output_Style.background = (Texture2D)EditorGUILayout.ObjectField("Background", AtomicSkin.Output_Style.background, typeof(Texture2D), false);
            GUILayout.Space(5);
            AtomicSkin.Output_Style.on_background = (Texture2D)EditorGUILayout.ObjectField("On Normal Background", AtomicSkin.Output_Style.on_background, typeof(Texture2D), false);

            GUILayout.Space(5);

            AtomicSkin.Output_Style.transparent = EditorGUILayout.Slider("Transparency", AtomicSkin.Output_Style.transparent, 0, 1);

            GUILayout.Space(5);

            AtomicSkin.Output_Style.background_color = EditorGUILayout.ColorField("Background Color", AtomicSkin.Output_Style.background_color);
            GUILayout.Space(5);
            AtomicSkin.Output_Style.onNormal_background_color = EditorGUILayout.ColorField("On Normal Background Color", AtomicSkin.Output_Style.onNormal_background_color);

            GUILayout.Space(5);

            AtomicSkin.Output_Style.background_position = (ImagePosition)EditorGUILayout.EnumPopup("Background Position", AtomicSkin.Output_Style.background_position);
            GUILayout.Space(5);
            AtomicSkin.Output_Style.text_clip = (TextClipping)EditorGUILayout.EnumPopup("Text Clipping", AtomicSkin.Output_Style.text_clip);
            GUILayout.Space(5);
            AtomicSkin.Output_Style.content_offset = EditorGUILayout.Vector2Field("Content Offset", AtomicSkin.Output_Style.content_offset);
            GUILayout.Space(5);
            AtomicSkin.Output_Style.margin = EditorGUILayout.Vector4Field("Margin", AtomicSkin.Output_Style.margin);
            GUILayout.Space(5);
            AtomicSkin.Output_Style.padding = EditorGUILayout.Vector4Field("Padding", AtomicSkin.Output_Style.padding);
            GUILayout.Space(5);
            AtomicSkin.Output_Style.overflow = EditorGUILayout.Vector4Field("Overflow", AtomicSkin.Output_Style.overflow);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }
         else
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        } 




        showButtonStyle =  GUILayout.Toggle(showButtonStyle, "Button Style", "button", GUILayout.Height(25));
        if (showButtonStyle)
        {
            GUILayout.Space(5);
            AtomicSkin.Button_Style.font_style = (FontStyle)EditorGUILayout.EnumPopup("Font Style", AtomicSkin.Button_Style.font_style);
            GUILayout.Space(5);
            AtomicSkin.Button_Style.font_color = EditorGUILayout.ColorField("Font Color", AtomicSkin.Button_Style.font_color);
            GUILayout.Space(5);
            AtomicSkin.Button_Style.alignment = (TextAnchor)EditorGUILayout.EnumPopup("Alignment", AtomicSkin.Button_Style.alignment);
            GUILayout.Space(5);
            AtomicSkin.Button_Style.font_size = EditorGUILayout.IntField("Font Size", AtomicSkin.Button_Style.font_size);

            GUILayout.Space(5);
            AtomicSkin.Button_Style.border = EditorGUILayout.Vector4Field("border", AtomicSkin.Button_Style.border);
            GUILayout.Space(5);

            AtomicSkin.Button_Style.background = (Texture2D)EditorGUILayout.ObjectField("Background", AtomicSkin.Button_Style.background, typeof(Texture2D), false);
            GUILayout.Space(5);
            AtomicSkin.Button_Style.on_background = (Texture2D)EditorGUILayout.ObjectField("On Normal Background", AtomicSkin.Button_Style.on_background, typeof(Texture2D), false);

            GUILayout.Space(5);

            AtomicSkin.Button_Style.transparent = EditorGUILayout.Slider("Transparency", AtomicSkin.Button_Style.transparent, 0, 1);

            GUILayout.Space(5);

            AtomicSkin.Button_Style.background_color = EditorGUILayout.ColorField("Background Color", AtomicSkin.Button_Style.background_color);
            GUILayout.Space(5);
            AtomicSkin.Button_Style.onNormal_background_color = EditorGUILayout.ColorField("On Normal Background Color", AtomicSkin.Button_Style.onNormal_background_color);
            GUILayout.Space(5);
            AtomicSkin.Button_Style.background_position = (ImagePosition)EditorGUILayout.EnumPopup("Background Position", AtomicSkin.Button_Style.background_position);
            GUILayout.Space(5);
            AtomicSkin.Button_Style.text_clip = (TextClipping)EditorGUILayout.EnumPopup("Text Clipping", AtomicSkin.Button_Style.text_clip);
            GUILayout.Space(5);
            AtomicSkin.Button_Style.content_offset = EditorGUILayout.Vector2Field("Content Offset", AtomicSkin.Button_Style.content_offset);
            GUILayout.Space(5);

            AtomicSkin.Button_Style.WH = EditorGUILayout.Vector2Field("Width & Height", AtomicSkin.Button_Style.WH);

            GUILayout.Space(5);
            AtomicSkin.Button_Style.margin = EditorGUILayout.Vector4Field("Margin", AtomicSkin.Button_Style.margin);
            GUILayout.Space(5);
            AtomicSkin.Button_Style.padding = EditorGUILayout.Vector4Field("Padding", AtomicSkin.Button_Style.padding);
            GUILayout.Space(5);
            AtomicSkin.Button_Style.overflow = EditorGUILayout.Vector4Field("Overflow", AtomicSkin.Button_Style.overflow);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
          
        }
        else
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        } 

   



        showCloseButtonStyle = GUILayout.Toggle(showCloseButtonStyle, "Close Button Style", "button", GUILayout.Height(25));
        if (showCloseButtonStyle)
        {
            GUILayout.Space(5);
            AtomicSkin.CloseButton_Style.font_style = (FontStyle)EditorGUILayout.EnumPopup("Font Style", AtomicSkin.CloseButton_Style.font_style);
            GUILayout.Space(5);
            AtomicSkin.CloseButton_Style.font_color = EditorGUILayout.ColorField("Font Color", AtomicSkin.CloseButton_Style.font_color);
            GUILayout.Space(5);
            AtomicSkin.CloseButton_Style.alignment = (TextAnchor)EditorGUILayout.EnumPopup("Alignment", AtomicSkin.CloseButton_Style.alignment);
            GUILayout.Space(5);
            AtomicSkin.CloseButton_Style.font_size = EditorGUILayout.IntField("Font Size", AtomicSkin.CloseButton_Style.font_size);

            GUILayout.Space(5);
            AtomicSkin.CloseButton_Style.border = EditorGUILayout.Vector4Field("border", AtomicSkin.CloseButton_Style.border);
            GUILayout.Space(5);
            AtomicSkin.CloseButton_Style.background = (Texture2D)EditorGUILayout.ObjectField("Background", AtomicSkin.CloseButton_Style.background, typeof(Texture2D), false);
            GUILayout.Space(5);
            AtomicSkin.CloseButton_Style.on_background = (Texture2D)EditorGUILayout.ObjectField("On Normal Background", AtomicSkin.CloseButton_Style.on_background, typeof(Texture2D), false);

            GUILayout.Space(5);

            AtomicSkin.CloseButton_Style.transparent = EditorGUILayout.Slider("Transparency", AtomicSkin.CloseButton_Style.transparent, 0, 1);

            GUILayout.Space(5);

            AtomicSkin.CloseButton_Style.background_color = EditorGUILayout.ColorField("Background Color", AtomicSkin.CloseButton_Style.background_color);
            GUILayout.Space(5);
            AtomicSkin.CloseButton_Style.onNormal_background_color = EditorGUILayout.ColorField("On Normal Background Color", AtomicSkin.CloseButton_Style.onNormal_background_color);
            GUILayout.Space(5);
            AtomicSkin.CloseButton_Style.background_position = (ImagePosition)EditorGUILayout.EnumPopup("Background Position", AtomicSkin.CloseButton_Style.background_position);
            GUILayout.Space(5);
            AtomicSkin.CloseButton_Style.text_clip = (TextClipping)EditorGUILayout.EnumPopup("Text Clipping", AtomicSkin.CloseButton_Style.text_clip);
            GUILayout.Space(5);
            AtomicSkin.CloseButton_Style.content_offset = EditorGUILayout.Vector2Field("Content Offset", AtomicSkin.CloseButton_Style.content_offset);
            GUILayout.Space(5);

            AtomicSkin.CloseButton_Style.Button_Rect = EditorGUILayout.RectField("Rect",AtomicSkin.CloseButton_Style.Button_Rect);

            GUILayout.Space(5);

            AtomicSkin.CloseButton_Style.margin = EditorGUILayout.Vector4Field("Margin", AtomicSkin.CloseButton_Style.margin);
            GUILayout.Space(5);
            AtomicSkin.CloseButton_Style.padding = EditorGUILayout.Vector4Field("Padding", AtomicSkin.CloseButton_Style.padding);
            GUILayout.Space(5);
            AtomicSkin.CloseButton_Style.overflow = EditorGUILayout.Vector4Field("Overflow", AtomicSkin.CloseButton_Style.overflow);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }
        else
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        } 




        showScrollbarStyle = GUILayout.Toggle(showScrollbarStyle, "Scrollbar Style", "button", GUILayout.Height(25));
        if (showScrollbarStyle)
        {
            AtomicSkin.Scrollbar_Style.Background_Horizontal = (Texture2D)EditorGUILayout.ObjectField("Background Horizontal", AtomicSkin.Scrollbar_Style.Background_Horizontal, typeof(Texture2D), false);
            AtomicSkin.Scrollbar_Style.On_Background_Horizontal = (Texture2D)EditorGUILayout.ObjectField("On Background Horizontal", AtomicSkin.Scrollbar_Style.On_Background_Horizontal, typeof(Texture2D), false);

            AtomicSkin.Scrollbar_Style.Background_Vertical = (Texture2D)EditorGUILayout.ObjectField("Background Vertical", AtomicSkin.Scrollbar_Style.Background_Vertical, typeof(Texture2D), false);
            AtomicSkin.Scrollbar_Style.On_Background_Vertical = (Texture2D)EditorGUILayout.ObjectField("On Background Vertical", AtomicSkin.Scrollbar_Style.On_Background_Vertical, typeof(Texture2D), false);
            GUILayout.Space(15);
            AtomicSkin.Scrollbar_Style.transparent = EditorGUILayout.Slider("Background Transparency", AtomicSkin.Scrollbar_Style.transparent, 0, 1);
            GUILayout.Space(15);
            AtomicSkin.Scrollbar_Style.Thumb_Horizontal = (Texture2D)EditorGUILayout.ObjectField("Thumb Horizontal", AtomicSkin.Scrollbar_Style.Thumb_Horizontal, typeof(Texture2D), false);
            AtomicSkin.Scrollbar_Style.on_Thumb_Horizontal = (Texture2D)EditorGUILayout.ObjectField("on Thumb Horizontal", AtomicSkin.Scrollbar_Style.on_Thumb_Horizontal, typeof(Texture2D), false);
            
            AtomicSkin.Scrollbar_Style.Thumb_Vertical = (Texture2D)EditorGUILayout.ObjectField("Thumb Vertical", AtomicSkin.Scrollbar_Style.Thumb_Vertical, typeof(Texture2D), false);
            AtomicSkin.Scrollbar_Style.on_Thumb_Vertical = (Texture2D)EditorGUILayout.ObjectField("on Thumb Vertical", AtomicSkin.Scrollbar_Style.on_Thumb_Vertical, typeof(Texture2D), false);



            GUILayout.Space(30);

            AtomicSkin.Scrollbar_Style.Background_Color = EditorGUILayout.ColorField("Background Color", AtomicSkin.Scrollbar_Style.Background_Color);
            AtomicSkin.Scrollbar_Style.Thumb_Color = EditorGUILayout.ColorField("Thumb Color", AtomicSkin.Scrollbar_Style.Thumb_Color);
            AtomicSkin.Scrollbar_Style.HoverThumb_Color = EditorGUILayout.ColorField("Thumb Hover Color", AtomicSkin.Scrollbar_Style.HoverThumb_Color);

            GUILayout.Space(30);

            showVerticalScrollbarStyle = GUILayout.Toggle(showVerticalScrollbarStyle, "Vertical Scrollbar", "button", GUILayout.Height(25));
           

            if(showVerticalScrollbarStyle)
            { 

                AtomicSkin.Scrollbar_Style.Vertical_Background_Border = EditorGUILayout.Vector4Field("Background Border", AtomicSkin.Scrollbar_Style.Vertical_Background_Border);
                AtomicSkin.Scrollbar_Style.Vertical_Thumb_Border = EditorGUILayout.Vector4Field("Thumb Border", AtomicSkin.Scrollbar_Style.Vertical_Thumb_Border);
               
                GUILayout.Space(20);

                AtomicSkin.Scrollbar_Style.Vertical_Stretch_Height = EditorGUILayout.Toggle("Stretch Height", AtomicSkin.Scrollbar_Style.Vertical_Stretch_Height);
                AtomicSkin.Scrollbar_Style.Vertical_Stretch_Width = EditorGUILayout.Toggle("Stretch Width", AtomicSkin.Scrollbar_Style.Vertical_Stretch_Width);

                GUILayout.Space(20);

                AtomicSkin.Scrollbar_Style.Vertical_Background_Fixed_Height = EditorGUILayout.IntField("Background Fixed Height", AtomicSkin.Scrollbar_Style.Vertical_Background_Fixed_Height);
                AtomicSkin.Scrollbar_Style.Vertical_Background_Fixed_Width = EditorGUILayout.IntField("Background Fixed Width", AtomicSkin.Scrollbar_Style.Vertical_Background_Fixed_Width);
                AtomicSkin.Scrollbar_Style.Vertical_Thumb_Fixed_Height = EditorGUILayout.IntField("Thumb Fixed Height", AtomicSkin.Scrollbar_Style.Vertical_Thumb_Fixed_Height);
                AtomicSkin.Scrollbar_Style.Vertical_Thumb_Fixed_Width = EditorGUILayout.IntField("Thumb Fixed Width", AtomicSkin.Scrollbar_Style.Vertical_Thumb_Fixed_Width);

                GUILayout.Space(20);

                AtomicSkin.Scrollbar_Style.Vertical_Text_clip = (TextClipping)EditorGUILayout.EnumPopup("Text Clipping", AtomicSkin.Scrollbar_Style.Vertical_Text_clip);
                AtomicSkin.Scrollbar_Style.Vertical_Image_position = (ImagePosition)EditorGUILayout.EnumPopup("Image Clipping", AtomicSkin.Scrollbar_Style.Vertical_Image_position);
                AtomicSkin.Scrollbar_Style.Vertical_Alignment = (TextAnchor)EditorGUILayout.EnumPopup("Alignment", AtomicSkin.Scrollbar_Style.Vertical_Alignment);

                GUILayout.Space(20);

                AtomicSkin.Scrollbar_Style.Vertical_Background_margin = EditorGUILayout.Vector4Field("Background Margin", AtomicSkin.Scrollbar_Style.Vertical_Background_margin);
                AtomicSkin.Scrollbar_Style.Vertical_Background_padding = EditorGUILayout.Vector4Field("Background Padding", AtomicSkin.Scrollbar_Style.Vertical_Background_padding);
                AtomicSkin.Scrollbar_Style.Vertical_Background_overflow = EditorGUILayout.Vector4Field("Background Overflow", AtomicSkin.Scrollbar_Style.Vertical_Background_overflow);

                GUILayout.Space(10);

                AtomicSkin.Scrollbar_Style.Vertical_Thumb_margin = EditorGUILayout.Vector4Field("Thumb Margin", AtomicSkin.Scrollbar_Style.Vertical_Thumb_margin);
                AtomicSkin.Scrollbar_Style.Vertical_Thumb_padding = EditorGUILayout.Vector4Field("Thumb Padding", AtomicSkin.Scrollbar_Style.Vertical_Thumb_padding);
                AtomicSkin.Scrollbar_Style.Vertical_Thumb_overflow = EditorGUILayout.Vector4Field("Thumb Overflow", AtomicSkin.Scrollbar_Style.Vertical_Thumb_overflow);

                GUILayout.Space(20);

                AtomicSkin.Scrollbar_Style.Vertical_Background_Contect_Offset = EditorGUILayout.Vector2Field("Background Contect Offset", AtomicSkin.Scrollbar_Style.Vertical_Background_Contect_Offset);
                AtomicSkin.Scrollbar_Style.Vertical_Thumb_Contect_Offset = EditorGUILayout.Vector2Field("Thumb Contect Offset", AtomicSkin.Scrollbar_Style.Vertical_Thumb_Contect_Offset);

            }


            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            showHorizontalScrollbarStyle = GUILayout.Toggle(showHorizontalScrollbarStyle, "Horizontal Scrollbar", "button", GUILayout.Height(25));

            if (showHorizontalScrollbarStyle)
            {

                AtomicSkin.Scrollbar_Style.Horizontal_Background_Border = EditorGUILayout.Vector4Field("Background Border", AtomicSkin.Scrollbar_Style.Horizontal_Background_Border);
                AtomicSkin.Scrollbar_Style.Horizontal_Thumb_Border = EditorGUILayout.Vector4Field("Thumb Border", AtomicSkin.Scrollbar_Style.Horizontal_Thumb_Border);
                                            
                GUILayout.Space(20);        

                AtomicSkin.Scrollbar_Style.Horizontal_Stretch_Height = EditorGUILayout.Toggle("Stretch Height", AtomicSkin.Scrollbar_Style.Horizontal_Stretch_Height);
                AtomicSkin.Scrollbar_Style.Horizontal_Stretch_Width = EditorGUILayout.Toggle("Stretch Width", AtomicSkin.Scrollbar_Style.Horizontal_Stretch_Width);
                                            
                GUILayout.Space(20);       
                                            
                AtomicSkin.Scrollbar_Style.Horizontal_Background_Fixed_Height = EditorGUILayout.IntField("Background Fixed Height", AtomicSkin.Scrollbar_Style.Horizontal_Background_Fixed_Height);
                AtomicSkin.Scrollbar_Style.Horizontal_Background_Fixed_Width = EditorGUILayout.IntField("Background Fixed Width", AtomicSkin.Scrollbar_Style.Horizontal_Background_Fixed_Width);
                AtomicSkin.Scrollbar_Style.Horizontal_Thumb_Fixed_Height = EditorGUILayout.IntField("Thumb Fixed Height", AtomicSkin.Scrollbar_Style.Horizontal_Thumb_Fixed_Height);
                AtomicSkin.Scrollbar_Style.Horizontal_Thumb_Fixed_Width = EditorGUILayout.IntField("Thumb Fixed Width", AtomicSkin.Scrollbar_Style.Horizontal_Thumb_Fixed_Width);
                                            
                GUILayout.Space(20);        
                                            
                AtomicSkin.Scrollbar_Style.Horizontal_Text_clip = (TextClipping)EditorGUILayout.EnumPopup("Text Clipping", AtomicSkin.Scrollbar_Style.Horizontal_Text_clip);
                AtomicSkin.Scrollbar_Style.Horizontal_Image_position = (ImagePosition)EditorGUILayout.EnumPopup("Image Clipping", AtomicSkin.Scrollbar_Style.Horizontal_Image_position);
                AtomicSkin.Scrollbar_Style.Horizontal_Alignment = (TextAnchor)EditorGUILayout.EnumPopup("Alignment", AtomicSkin.Scrollbar_Style.Horizontal_Alignment);
                                            
                GUILayout.Space(20);        
                                           
                AtomicSkin.Scrollbar_Style.Horizontal_Background_margin = EditorGUILayout.Vector4Field("Background Margin", AtomicSkin.Scrollbar_Style.Horizontal_Background_margin);
                AtomicSkin.Scrollbar_Style.Horizontal_Background_padding = EditorGUILayout.Vector4Field("Background Padding", AtomicSkin.Scrollbar_Style.Horizontal_Background_padding);
                AtomicSkin.Scrollbar_Style.Horizontal_Background_overflow = EditorGUILayout.Vector4Field("Background Overflow", AtomicSkin.Scrollbar_Style.Horizontal_Background_overflow);
                                           
                GUILayout.Space(10);       
                                           
                AtomicSkin.Scrollbar_Style.Horizontal_Thumb_margin = EditorGUILayout.Vector4Field("Thumb Margin", AtomicSkin.Scrollbar_Style.Horizontal_Thumb_margin);
                AtomicSkin.Scrollbar_Style.Horizontal_Thumb_padding = EditorGUILayout.Vector4Field("Thumb Padding", AtomicSkin.Scrollbar_Style.Horizontal_Thumb_padding);
                AtomicSkin.Scrollbar_Style.Horizontal_Thumb_overflow = EditorGUILayout.Vector4Field("Thumb Overflow", AtomicSkin.Scrollbar_Style.Horizontal_Thumb_overflow);
                                           
                GUILayout.Space(20);        
                                          
                AtomicSkin.Scrollbar_Style.Horizontal_Background_Contect_Offset = EditorGUILayout.Vector2Field("Background Contect Offset", AtomicSkin.Scrollbar_Style.Horizontal_Background_Contect_Offset);
                AtomicSkin.Scrollbar_Style.Horizontal_Thumb_Contect_Offset = EditorGUILayout.Vector2Field("Thumb Contect Offset", AtomicSkin.Scrollbar_Style.Horizontal_Thumb_Contect_Offset);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            }
            else
            {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            }
        }
        else
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }


        GUILayout.EndVertical();

        GUILayout.Space(20);

        if(AtomicSkin.defaultSkin == null)
            AtomicSkin.defaultSkin = Resources.Load<GUISkin>("DefaultGUI");
        
        if(AtomicSkin.defaultSkin != null)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Default Skin", AtomicSkin.defaultSkin, typeof(GUISkin), false);
            EditorGUI.EndDisabledGroup();
        }

        GUILayout.Space(20); //end
        //check for changes and press apply to save
        if(EditorGUI.EndChangeCheck())
        {
            dirtyStates[instanceID] = true;
        }

         if (GUILayout.Button(isDirty ? "Save Settings*" : "Save Settings", GUILayout.Height(40)))
        {
            if (isDirty)
            {
                EditorUtility.SetDirty(AtomicSkin);
                AssetDatabase.SaveAssets();
                dirtyStates[instanceID] = false;
            }
        }
        if(isDirty)
        {
            GUILayout.BeginVertical("Box");
            GUILayout.Space(5);
            GUIContent labelEnd = new GUIContent(" Warning", EditorGUIUtility.IconContent("console.warnicon.inactive.sml").image);
            EditorGUILayout.LabelField(labelEnd, EditorStyles.boldLabel);
            string labelInfoEnd = "Be sure to Save Settings before play mode or closing Unity Engine.";
            EditorGUILayout.LabelField(labelInfoEnd, EditorStyles.wordWrappedLabel);
            GUILayout.Space(5);
            GUILayout.EndVertical();
        }

        EditorGUILayout.Space();
        Texture2D logo = Resources.Load<Texture2D>("Ico/SoL_Ico");
        if (logo != null)
        {
            Rect rect = GUILayoutUtility.GetRect(0.0f, 60.0f, GUILayout.ExpandWidth(true));
            GUI.DrawTexture(rect, logo, ScaleMode.ScaleToFit);
        }
    
    }




    private Vector2Int Vector2IntField(string label, Vector2Int value)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(EditorGUIUtility.labelWidth));

        EditorGUIUtility.labelWidth = 30;
        value.x = EditorGUILayout.IntField("X", value.x);
        value.y = EditorGUILayout.IntField("Y", value.y);
        EditorGUIUtility.labelWidth = 0;

        EditorGUILayout.EndHorizontal();

        return value;
    }

     private Rect RectField(string label, Rect value)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(EditorGUIUtility.labelWidth));

        EditorGUIUtility.labelWidth = 60;
        EditorGUILayout.BeginVertical();
        value.x = EditorGUILayout.FloatField("top", value.x);
        GUILayout.Space(5);
        value.y = EditorGUILayout.FloatField("bottom", value.y);
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        value.width = EditorGUILayout.FloatField("width", value.width);
        GUILayout.Space(5);
        value.height = EditorGUILayout.FloatField("height", value.height);
        EditorGUILayout.EndVertical();
        EditorGUIUtility.labelWidth = 0;

        EditorGUILayout.EndHorizontal();

        return value;
    }
}
}