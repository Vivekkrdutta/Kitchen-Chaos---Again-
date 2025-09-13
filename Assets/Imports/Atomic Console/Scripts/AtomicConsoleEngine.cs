using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Reflection;
using System.Globalization;
using System.Linq;
using System.IO;
using System;
using System.Text;
using System.Text.RegularExpressions;
using AtomicMapping;
using AtomicConsole.debug;
using AtomicConsole.Skin.editor;
using AtomicAssembly.GeneratedCommands;
using AtomicAssembly;
using System.Net;
using Unity.VisualScripting;

namespace AtomicConsole.Engine
{
    public class AtomicConsoleEngine : MonoBehaviour
    {
        public static AtomicConsoleEngine Instance;
        public AtomicSkin skin;

        //public CommandStartEnum CommmadStartLine; //TODO next update
        public bool invidiualLogs = false;
        public bool SleepMode = false;
        public bool AlwaysOnFocus = true;
        public bool dontDestroyOnLoad = true;
        public KeyCode ToggleConsoleInput = KeyCode.BackQuote;
        public bool password_Protected;
        public string password;
        public bool isLocked = true;
        private List<MethodInfo> filteredCommandList = new List<MethodInfo>();
        private List<FieldInfo> filteredSetList = new List<FieldInfo>();
        public List<(MethodInfo, object)> commandMethods;
        public List<(FieldInfo, object)> setFields = new List<(FieldInfo, object)>();

        [HideInInspector] public List<string> consoleLogs = new List<string>();
        public Vector2 scrollPos;
        public float scrollPosSmoothnes = 0.3f;
        [HideInInspector] public bool isVisible = false;
        public string inputText = "";
        private Rect windowRect = new Rect(Screen.width / 4, Screen.height / 4, 3 * Screen.width / 4, Screen.height / 4); //next update

        private List<string> commandHistory = new List<string>();
        private int commandHistoryIndex = 0;


        private Vector2 _resizingStart;
        private Rect _resizingStartRect;


        private string HeaderName = "";
        private string Btn_Send = "";
        private string Btn_Clear = "";


        private GUIStyle logLabelStyle;
        private GUIStyle inputStyle;
        private GUIStyle buttonStyle;
        private GUIStyle outputTextAreaStyle;
        private GUIStyle CommandStyle;
        private GUIStyle closeButtonStyle;
        private GUISkin scrollBarStyle;
        private GUIStyle headerStyle;

        private bool GUIskinReady = false;

        private int maxSuggestions;

        Texture2D normalBackground;
        Texture2D highlightedBackground;


        public bool Diagnostic = false;
        private float fps;
        private float msec;
        private long memoryUsage;

        //next update
        /*public enum CommandStartEnum
        {
            Null,
            ForwardSlash,
            Minus,
            Plus,
            ExclamationMark,
            DollarSign
        }*/


        private void OnApplicationFocus(bool focusStatus)
        {
            if (focusStatus)
            {
                if (skin.defaultSkin == null)
                {
                    skin.defaultSkin = Resources.Load<GUISkin>("DefaultGUI");
                }
            }
        }

        private Resizer resizer;

        private void Awake()
        {
            resizer = GetComponent<Resizer>();
            if(resizer.constantSize)
            {
                windowRect = new Rect(Screen.width / 4, Screen.height / 4, resizer.windowSize.x, resizer.windowSize.y);
            }
            else
            {
                windowRect = new Rect(Screen.width / 4, Screen.height / 4, Screen.width * resizer.sizeMulitplier.x, Screen.height * resizer.sizeMulitplier.y);
            }
                isLocked = password_Protected;
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(this.gameObject);
            }

            if (isVisible)
                isVisible = !isVisible;

            Application.logMessageReceived += UnityHandleLog;
            AtomicDebug.OnLogReceived += HandleLog;
        }


        bool HighlightFlagSetup = false;
        bool HighlightNormalSetup = false;
        private void Start()
        {
            if (Instance == null)
                Instance = FindFirstObjectByType<AtomicConsoleEngine>();

            //make sure if gui skin is ready
            if (skin.defaultSkin == null)
            {
                Debug.LogError("Gui default skin is unable to load, please restart Unity or change AtomicConsole Skin!");
                GUIskinReady = false;
            }
            else
            {
                GUIskinReady = true;
            }

            //setup skin
            HeaderName = skin.Header;
            Btn_Send = skin.Button_Send;
            Btn_Clear = skin.Button_Clear;


            if (GUIskinReady)
            {
                // Apply GUI styles
                logLabelStyle = CreateGUIStyleFromSkinStyle(skin.Log_Style);
                headerStyle = CreateGUIStyleFromSkinStyle(skin.Header_Style);
                inputStyle = CreateGUIStyleFromSkinStyle(skin.Input_Style);
                buttonStyle = CreateGUIStyleFromSkinStyle(skin.Button_Style);
                outputTextAreaStyle = CreateGUIStyleFromSkinStyle(skin.Output_Style);
                CommandStyle = CreateGUIStyleFromSkinStyle(skin.Command_Style);
                closeButtonStyle = CreateGUIStyleFromSkinStyle(skin.CloseButton_Style);
                //scrollBarStyle = CreateGUIStyleFromSkinStyle(skin.Scrollbar_Style);
                scrollBarStyle = AssignPropertiesScrollbar(skin.defaultSkin, skin.Scrollbar_Style);
            }

            maxSuggestions = skin.MaxSuggestion;


            // Initialize commandMethods and setFields lists
            commandMethods = new List<(MethodInfo, object)>();
            setFields = new List<(FieldInfo, object)>();

            if (HighlightNormalSetup)
            {
                normalBackground = TextureColor(skin.Command_Style.background, skin.Command_Style.background_color, skin.Command_Style.transparent);
                HighlightNormalSetup = false;
            }
            else
            {
                normalBackground = skin.defaultSkin.box.normal.background;
            }

            if (HighlightFlagSetup)
            {
                highlightedBackground = TextureColor(skin.Command_Style.on_background, skin.Command_Style.onNormal_background_color, skin.Command_Style.transparent);
                HighlightFlagSetup = false;
            }
            else
            {
                highlightedBackground = skin.defaultSkin.box.hover.background;
            }


            // Use the generated lists
            foreach (MethodInfo methodInfo in AtomicCommands.commandMethods)
            {
                object instance = null;
                if (!methodInfo.IsStatic)
                {
                    instance = FindFirstObjectByType(methodInfo.ReflectedType);
                    if (instance == null)
                    {
                        //Debug.LogWarning($"No instance found for non-static method {methodInfo.Name} in type {methodInfo.ReflectedType.FullName}");
                        continue;
                    }
                }
                commandMethods.Add((methodInfo, instance));
            }

            foreach (FieldInfo fieldInfo in AtomicCommands.setFields)
            {
                object instance = null;
                if (!fieldInfo.IsStatic)
                {
                    instance = FindFirstObjectByType(fieldInfo.ReflectedType);
                    if (instance == null)
                    {
                        Debug.LogWarning($"No instance found for non-static field {fieldInfo.Name} in type {fieldInfo.ReflectedType.FullName}");
                        continue;
                    }
                }
                setFields.Add((fieldInfo, instance));
            }

            storeCaretColor = skin.CaretColor;

        }


        private void Update()
        {
            if (Input.GetKeyDown(ToggleConsoleInput) && !isVisible)
            {
                ToggleConsole(!isVisible);
                SkipFrameUpdate = true;
            }
        }



        private void LateUpdate()
        {
            if (logsChanged)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < consoleLogs.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(consoleLogs[i]))
                    {
                        continue;
                    }

                    sb.Append(consoleLogs[i]);

                    if (i < consoleLogs.Count - 1)
                    {
                        sb.AppendLine();
                    }
                }
                concatenatedLogs = sb.ToString().Trim();
                logsChanged = false;
            }


            if (skin.ChangeInRealTime)
            {
                HeaderName = skin.Header;
                Btn_Send = skin.Button_Send;
                Btn_Clear = skin.Button_Clear;

                logLabelStyle = CreateGUIStyleFromSkinStyle(skin.Log_Style);
                inputStyle = CreateGUIStyleFromSkinStyle(skin.Input_Style);
                buttonStyle = CreateGUIStyleFromSkinStyle(skin.Button_Style);
                outputTextAreaStyle = CreateGUIStyleFromSkinStyle(skin.Output_Style);
                CommandStyle = CreateGUIStyleFromSkinStyle(skin.Command_Style);
                closeButtonStyle = CreateGUIStyleFromSkinStyle(skin.CloseButton_Style);
                //scrollBarStyle = CreateGUIStyleFromSkinStyle(skin.Scrollbar_Style);
                scrollBarStyle = AssignPropertiesScrollbar(skin.defaultSkin, skin.Scrollbar_Style);
                headerStyle = CreateGUIStyleFromSkinStyle(skin.Header_Style);
            }

        }

        private GUIStyle CreateGUIStyleFromSkinStyle(object skinStyle)
        {
            var guiStyle = new GUIStyle();

            switch (skinStyle)
            {
                case log_Style style:
                    AssignProperties(guiStyle, style);
                    break;
                case input_Style style:
                    AssignProperties(guiStyle, style);
                    break;
                case button_Style style:
                    AssignProperties(guiStyle, style);
                    break;
                case output_Style style:
                    AssignProperties(guiStyle, style);
                    break;
                case command_Style style:
                    AssignProperties(guiStyle, style);
                    break;
                case closeButton_Style style:
                    AssignProperties(guiStyle, style);
                    break;
                case header_Style style:
                    AssignProperties(guiStyle, style);
                    break;
            }
            return guiStyle;
        }


        private void AssignProperties(GUIStyle guiStyle, log_Style style)
        {

            //if user did not provide any gui texture, use unity default gui textures
            if (style.background == null)
            {
                guiStyle.normal.background = skin.defaultSkin.box.normal.background;
                //throw warning
                //Debug.LogWarning("[AtomicSkin]Window Style 'onNormal' texture is missing. Using Unity's default Box texture.");
            }
            else
                guiStyle.normal.background = TextureColor(style.background, style.background_color, style.transparent);


            if (style.on_background == null)
            {
                guiStyle.hover.background = skin.defaultSkin.box.hover.background;
                //throw warning
                //Debug.LogWarning("[AtomicSkin]Window Style 'Hover' texture is missing. Using Unity's default Box texture.");
            }
            else
                guiStyle.hover.background = TextureColor(style.on_background, style.onNormal_background_color, style.transparent);



            if (guiStyle.normal.background == skin.defaultSkin.box.normal.background)
            {
                guiStyle.border = new RectOffset(6, 6, 6, 6);
            }
            else
            {
                guiStyle.border = Vector4ToRectOffset(style.border);
            }

            if (guiStyle.hover.background == skin.defaultSkin.box.hover.background)
            {
                guiStyle.border = new RectOffset(6, 6, 6, 6);
            }
            else
            {
                guiStyle.border = Vector4ToRectOffset(style.border);
            }


            //guiStyle.normal.background = TextureColor(style.background, style.background_color, style.transparent);
            //guiStyle.hover.background = TextureColor(style.on_background, style.onNormal_background_color, style.transparent);

            //guiStyle.border = Vector4ToRectOffset(style.border);

            guiStyle.margin = Vector4ToRectOffset(style.margin);
            guiStyle.padding = Vector4ToRectOffset(style.padding);
            guiStyle.overflow = Vector4ToRectOffset(style.overflow);

            guiStyle.contentOffset = skin.Log_Style.content_offset;

            guiStyle.clipping = style.text_clip;
            guiStyle.imagePosition = style.background_position;
        }


        private void AssignProperties(GUIStyle guiStyle, header_Style style)
        {
            if (guiStyle.font == null)
                guiStyle.font = skin.defaultSkin.font;
            guiStyle.fontStyle = style.font_style;
            guiStyle.normal.textColor = style.font_color;
            guiStyle.alignment = style.alignment;
            guiStyle.fontSize = style.font_size;
            guiStyle.hover.textColor = style.font_color;


            //if user did not provide any gui texture, use unity default gui textures
            if (style.background == null)
            {
                guiStyle.normal.background = skin.defaultSkin.box.normal.background;
                //throw warning
                //Debug.LogWarning("[AtomicSkin]Header Style 'onNormal' texture is missing. Using Unity's default Box texture.");
            }
            else
                guiStyle.normal.background = TextureColor(style.background, style.background_color, style.transparent);


            if (style.on_background == null)
            {
                guiStyle.hover.background = skin.defaultSkin.box.hover.background;
                //throw warning
                //Debug.LogWarning("[AtomicSkin]Header Style 'Hover' texture is missing. Using Unity's default Box texture.");
            }
            else
                guiStyle.hover.background = TextureColor(style.on_background, style.onNormal_background_color, style.transparent);



            if (guiStyle.normal.background == skin.defaultSkin.box.normal.background)
            {
                guiStyle.border = new RectOffset(6, 6, 6, 6);
            }
            else
            {
                guiStyle.border = Vector4ToRectOffset(style.border);
            }

            if (guiStyle.hover.background == skin.defaultSkin.box.hover.background)
            {
                guiStyle.border = new RectOffset(6, 6, 6, 6);
            }
            else
            {
                guiStyle.border = Vector4ToRectOffset(style.border);
            }


            //guiStyle.normal.background = TextureColor(style.background, style.background_color, style.transparent);
            //guiStyle.hover.background = TextureColor(style.on_background, style.onNormal_background_color, style.transparent);

            //guiStyle.border = Vector4ToRectOffset(style.border);

            guiStyle.margin = Vector4ToRectOffset(style.margin);
            guiStyle.padding = Vector4ToRectOffset(style.padding);
            guiStyle.overflow = Vector4ToRectOffset(style.overflow);

            guiStyle.contentOffset = skin.Log_Style.content_offset;

            guiStyle.clipping = style.text_clip;
            guiStyle.imagePosition = style.background_position;

            guiStyle.fixedHeight = style.WH.y;
            guiStyle.fixedWidth = style.WH.x;
        }


        private void AssignProperties(GUIStyle guiStyle, input_Style style)
        {
            if (guiStyle.font == null)
                guiStyle.font = skin.defaultSkin.font;
            guiStyle.fontStyle = style.font_style;
            guiStyle.normal.textColor = style.font_color;
            guiStyle.alignment = style.alignment;
            guiStyle.fontSize = style.font_size;

            //if user did not provide any gui texture, use unity default gui textures
            if (style.background == null)
            {
                guiStyle.normal.background = skin.defaultSkin.textField.normal.background;
                //throw warning
                //Debug.LogWarning("[AtomicSkin]Input Style 'onNormal' texture is missing. Using Unity's default Text Field texture.");
            }
            else
                guiStyle.normal.background = TextureColor(style.background, style.background_color, style.transparent);


            if (style.on_background == null)
            {
                guiStyle.hover.background = skin.defaultSkin.textField.hover.background;
                //throw warning
                //Debug.LogWarning("[AtomicSkin]Input Style 'Hover' texture is missing. Using Unity's default Text Field texture.");
            }
            else
                guiStyle.hover.background = TextureColor(style.on_background, style.onNormal_background_color, style.transparent);



            if (guiStyle.normal.background == skin.defaultSkin.textField.normal.background)
            {
                guiStyle.border = new RectOffset(4, 4, 4, 4);
            }
            else
            {
                guiStyle.border = Vector4ToRectOffset(style.border);
            }

            if (guiStyle.hover.background == skin.defaultSkin.textField.hover.background)
            {
                guiStyle.border = new RectOffset(4, 4, 4, 4);
            }
            else
            {
                guiStyle.border = Vector4ToRectOffset(style.border);
            }


            //guiStyle.normal.background = TextureColor(style.background, style.background_color, style.transparent);
            //guiStyle.hover.background = TextureColor(style.on_background, style.onNormal_background_color, style.transparent);

            guiStyle.hover.textColor = style.font_color;

            guiStyle.margin = Vector4ToRectOffset(style.margin);
            guiStyle.padding = Vector4ToRectOffset(style.padding);
            guiStyle.overflow = Vector4ToRectOffset(style.overflow);

            guiStyle.contentOffset = skin.Input_Style.content_offset;

            guiStyle.clipping = style.text_clip;
            guiStyle.imagePosition = style.background_position;

            guiStyle.fixedHeight = style.WH.y;
            guiStyle.fixedWidth = style.WH.x;
        }

        private void AssignProperties(GUIStyle guiStyle, output_Style style)
        {
            if (guiStyle.font == null)
                guiStyle.font = skin.defaultSkin.font;
            guiStyle.fontStyle = style.font_style;
            guiStyle.normal.textColor = style.font_color;
            guiStyle.alignment = style.alignment;
            guiStyle.fontSize = style.font_size;

            //if user did not provide any gui texture, use unity default gui textures
            if (style.background == null)
            {
                guiStyle.normal.background = skin.defaultSkin.box.normal.background;
                //throw warning
                //Debug.LogWarning("[AtomicSkin]Output Style 'onNormal' texture is missing. Using Unity's default box texture.");
            }
            else
                guiStyle.normal.background = TextureColor(style.background, style.background_color, style.transparent);

            if (style.background == null)
            {
                guiStyle.hover.background = skin.defaultSkin.box.hover.background;
                //throw warning
                //Debug.LogWarning("[AtomicSkin]Output Style 'Hover' texture is missing. Using Unity's default box texture.");
            }
            else
                guiStyle.hover.background = TextureColor(style.on_background, style.onNormal_background_color, style.transparent);


            if (guiStyle.normal.background == skin.defaultSkin.box.normal.background)
            {
                guiStyle.border = new RectOffset(6, 6, 6, 6);
            }
            else
            {
                guiStyle.border = Vector4ToRectOffset(style.border);
            }

            if (guiStyle.hover.background == skin.defaultSkin.box.hover.background)
            {
                guiStyle.border = new RectOffset(6, 6, 6, 6);
            }
            else
            {
                guiStyle.border = Vector4ToRectOffset(style.border);
            }

            guiStyle.hover.textColor = style.font_color;

            guiStyle.margin = Vector4ToRectOffset(style.margin);
            guiStyle.padding = Vector4ToRectOffset(style.padding);
            guiStyle.overflow = Vector4ToRectOffset(style.overflow);

            guiStyle.contentOffset = skin.Output_Style.content_offset;

            guiStyle.clipping = style.text_clip;
            guiStyle.imagePosition = style.background_position;
        }

        //TODO add rect for button
        private void AssignProperties(GUIStyle guiStyle, button_Style style)
        {
            if (guiStyle.font == null)
                guiStyle.font = skin.defaultSkin.font;
            guiStyle.fontStyle = style.font_style;
            guiStyle.normal.textColor = style.font_color;
            guiStyle.alignment = style.alignment;
            guiStyle.fontSize = style.font_size;

            //if user did not provide any gui texture, use unity default gui textures
            if (style.background == null)
            {
                guiStyle.normal.background = skin.defaultSkin.button.normal.background;
                //throw warning
                //Debug.LogWarning("[AtomicSkin]Send/Clear Button's 'onNormal' texture is missing. Using Unity's default Button texture.");
            }
            else
                guiStyle.normal.background = TextureColor(style.background, style.background_color, style.transparent);


            if (style.on_background == null)
            {
                guiStyle.hover.background = skin.defaultSkin.button.hover.background;
                //throw warning
                //Debug.LogWarning("[AtomicSkin]Send/Clear Button's 'Hover' texture is missing. Using Unity's default Button texture.");
            }
            else
                guiStyle.hover.background = TextureColor(style.on_background, style.onNormal_background_color, style.transparent);



            if (guiStyle.normal.background == skin.defaultSkin.button.normal.background)
            {
                guiStyle.border = new RectOffset(6, 6, 6, 4);
            }
            else
            {
                guiStyle.border = Vector4ToRectOffset(style.border);
            }

            if (guiStyle.hover.background == skin.defaultSkin.button.hover.background)
            {
                guiStyle.border = new RectOffset(6, 6, 6, 4);
            }
            else
            {
                guiStyle.border = Vector4ToRectOffset(style.border);
            }

            guiStyle.hover.textColor = style.font_color;

            guiStyle.margin = Vector4ToRectOffset(style.margin);
            guiStyle.padding = Vector4ToRectOffset(style.padding);
            guiStyle.overflow = Vector4ToRectOffset(style.overflow);

            guiStyle.contentOffset = skin.Button_Style.content_offset;

            guiStyle.clipping = style.text_clip;
            guiStyle.imagePosition = style.background_position;

            guiStyle.fixedHeight = style.WH.y;
            guiStyle.fixedWidth = style.WH.x;
        }

        private void AssignProperties(GUIStyle guiStyle, command_Style style)
        {
            if (guiStyle.font == null)
                guiStyle.font = skin.defaultSkin.font;
            guiStyle.fontStyle = style.font_style;
            guiStyle.normal.textColor = style.font_color;
            guiStyle.alignment = style.alignment;
            guiStyle.fontSize = style.font_size;

            //if user did not provide any gui texture, use unity default gui textures
            if (style.background == null)
            {
                guiStyle.normal.background = skin.defaultSkin.box.normal.background;
                //throw warning
                //Debug.LogWarning("[AtomicSkin]Suggestion Style 'onNormal' texture is missing. Using Unity's default Box texture.");
            }
            else
            {
                guiStyle.normal.background = TextureColor(style.background, style.background_color, style.transparent);
                HighlightNormalSetup = true;
            }

            if (style.on_background == null) //hot fix
            {
                guiStyle.hover.background = skin.defaultSkin.box.hover.background;
                //throw warning
                //Debug.LogWarning("[AtomicSkin]Suggestion Style 'Hover' texture is missing. Using Unity's default Box texture.");
            }
            else
            {
                guiStyle.hover.background = TextureColor(style.on_background, style.onNormal_background_color, style.transparent);
                HighlightFlagSetup = true;
            }


            if (guiStyle.normal.background == skin.defaultSkin.box.normal.background)
            {
                guiStyle.border = new RectOffset(6, 6, 6, 6);
            }
            else
            {
                guiStyle.border = Vector4ToRectOffset(style.border);
            }

            if (guiStyle.hover.background == skin.defaultSkin.box.hover.background)
            {
                guiStyle.border = new RectOffset(6, 6, 6, 6);
            }
            else
            {
                guiStyle.border = Vector4ToRectOffset(style.border);
            }
            //guiStyle.border = Vector4ToRectOffset(style.border);

            guiStyle.hover.textColor = style.font_color;

            guiStyle.margin = Vector4ToRectOffset(style.margin);
            guiStyle.padding = Vector4ToRectOffset(style.padding);
            guiStyle.overflow = Vector4ToRectOffset(style.overflow);

            guiStyle.contentOffset = skin.Command_Style.content_offset;

            guiStyle.clipping = style.text_clip;
            guiStyle.imagePosition = style.background_position;

            guiStyle.fixedHeight = style.WH.y;
            guiStyle.fixedWidth = style.WH.x;
        }


        private void AssignProperties(GUIStyle guiStyle, closeButton_Style style)
        {
            if (guiStyle.font == null)
                guiStyle.font = skin.defaultSkin.font;
            guiStyle.fontStyle = style.font_style;
            guiStyle.normal.textColor = style.font_color;
            guiStyle.alignment = style.alignment;
            guiStyle.fontSize = style.font_size;

            //if user did not provide any gui texture, use unity default gui textures
            if (style.background == null)
            {
                guiStyle.normal.background = skin.defaultSkin.button.normal.background;
                //throw warning
                //Debug.LogWarning("[AtomicSkin]Close Button's 'onNormal' texture is missing. Using Unity's default Button texture.");
            }
            else
                guiStyle.normal.background = TextureColor(style.background, style.background_color, style.transparent);

            if (style.on_background == null)
            {
                guiStyle.hover.background = skin.defaultSkin.button.hover.background;
                //throw warning
                //Debug.LogWarning("[AtomicSkin]Close Button's 'Hover' texture is missing. Using Unity's default Button texture.");
            }
            else
                guiStyle.hover.background = TextureColor(style.on_background, style.onNormal_background_color, style.transparent);


            if (guiStyle.normal.background == skin.defaultSkin.button.normal.background)
            {
                guiStyle.border = new RectOffset(6, 6, 6, 4);
            }
            else
            {
                guiStyle.border = Vector4ToRectOffset(style.border);
            }

            if (guiStyle.hover.background == skin.defaultSkin.button.hover.background)
            {
                guiStyle.border = new RectOffset(6, 6, 6, 4);
            }
            else
            {
                guiStyle.border = Vector4ToRectOffset(style.border);
            }

            //guiStyle.border = Vector4ToRectOffset(style.border);

            guiStyle.hover.textColor = style.font_color;

            guiStyle.margin = Vector4ToRectOffset(style.margin);
            guiStyle.padding = Vector4ToRectOffset(style.padding);
            guiStyle.overflow = Vector4ToRectOffset(style.overflow);

            guiStyle.contentOffset = skin.CloseButton_Style.content_offset;

            guiStyle.clipping = style.text_clip;
            guiStyle.imagePosition = style.background_position;

        }

        /* private void AssignProperties(GUIStyle guiSkin, scrollbar_Style style)
         {
               //if user did not provide any gui texture, use unity default gui textures
             if(style.Background_Vertical == null)
                 {
                     guiSkin.normal.background = skin.defaultSkin.verticalScrollbar.normal.background;
                     //throw warning
                     Debug.LogWarning("[AtomicSkin]Scrollbar Style 'Normal Vertical' texture is missing. Using Unity's default Vertical Scrollbar texture.");
                 }
             else
                 {
                     guiSkin.normal.background = TextureColor(style.Background_Vertical, style.Background_Color, style.transparent);
                 }

             if(style.On_Background_Vertical == null)
                 {
                     guiSkin.hover.background = skin.defaultSkin.verticalScrollbar.hover.background;
                     //throw warning
                     Debug.LogWarning("[AtomicSkin]Scrollbar Style 'onNormal Vertical' texture is missing. Using Unity's default Vertical Scrollbar texture.");
                 }
             else
                 {
                     guiSkin.hover.background = TextureColor(style.On_Background_Vertical, style.Background_Color, style.transparent);
                 }



              if(style.Background_Horizontal == null)
                 {
                     guiSkin.normal.background = skin.defaultSkin.horizontalScrollbar.normal.background;
                     //throw warning
                     Debug.LogWarning("[AtomicSkin]Scrollbar Style 'Normal Horizontal' texture is missing. Using Unity's default Horizontal Scrollbar texture.");
                 }
             else
                 {
                     guiSkin.normal.background = TextureColor(style.Background_Horizontal, style.Background_Color, style.transparent);
                 }

             if(style.On_Background_Horizontal == null)
                 {
                     guiSkin.hover.background = skin.defaultSkin.horizontalScrollbar.hover.background;
                     //throw warning
                     Debug.LogWarning("[AtomicSkin]Scrollbar Style 'onNormal Horizontal' texture is missing. Using Unity's default Horizontal Scrollbar texture.");
                 }
             else
                 {
                     guiSkin.hover.background = TextureColor(style.On_Background_Horizontal, style.Background_Color, style.transparent);
                 }




              if(style.Thumb_Vertical == null)
                 {
                     guiSkin.normal.background = skin.defaultSkin.verticalScrollbarThumb.normal.background;
                     //throw warning
                     Debug.LogWarning("[AtomicSkin]Scrollbar Style 'Thumb Vertical' texture is missing. Using Unity's default Vertical Scrollbar Thumb texture.");
                 }
             else
                 {
                     guiSkin.normal.background = TextureColor(style.Thumb_Vertical, style.Thumb_Color, 1);
                 }

             if(style.on_Thumb_Vertical == null)
                 {
                     guiSkin.hover.background = skin.defaultSkin.verticalScrollbarThumb.hover.background;
                     //throw warning
                     Debug.LogWarning("[AtomicSkin]Scrollbar Style 'Thumb Vertical' texture is missing. Using Unity's default Vertical Scrollbar Thumb texture.");
                 }
             else
                 {
                     guiSkin.hover.background = TextureColor(style.on_Thumb_Vertical, style.Thumb_Color, 1);
                 }


             if(style.Thumb_Horizontal == null)
                 {
                     guiSkin.normal.background = skin.defaultSkin.horizontalScrollbarThumb.normal.background;
                     //throw warning
                     Debug.LogWarning("[AtomicSkin]Scrollbar Style 'Thumb Horizontal' texture is missing. Using Unity's default Horizontal Scrollbar Thumb texture.");
                 }
             else
                 {
                     guiSkin.normal.background = TextureColor(style.Thumb_Horizontal, style.Thumb_Color, 1);
                 }

             if(style.on_Thumb_Horizontal == null)
                 {
                     guiSkin.hover.background = skin.defaultSkin.horizontalScrollbarThumb.hover.background;
                     //throw warning
                     Debug.LogWarning("[AtomicSkin]Scrollbar Style 'Thumb Horizontal' texture is missing. Using Unity's default Horizontal Scrollbar Thumb texture.");
                 }
             else
                 {
                     guiSkin.hover.background = TextureColor(style.on_Thumb_Horizontal, style.Thumb_Color, 1);
                 }





             if(style.Background_Vertical == skin.defaultSkin.verticalScrollbar.normal.background)
                 {
                     guiSkin.border = new RectOffset(9,9,0,0);
                 }
             else
                 {
                     guiSkin.border = Vector4ToRectOffset(style.Vertical_Background_Border);
                 }

             if(style.On_Background_Vertical == skin.defaultSkin.verticalScrollbar.hover.background)
                 {
                     style.Horizontal_Background_Border = new Vector4(9,9,0,0);
                 }
             else
                 {
                     guiSkin.border = Vector4ToRectOffset(style.Vertical_Background_Border);
                 }



             if(style.Background_Horizontal == skin.defaultSkin.horizontalScrollbar.normal.background)
                 {
                     guiSkin.border = new RectOffset(9,9,0,0);
                 }
             else
                 {
                     guiSkin.border = Vector4ToRectOffset(style.Horizontal_Background_Border);
                 }

             if(style.On_Background_Horizontal == skin.defaultSkin.horizontalScrollbar.hover.background)
                 {
                     style.Horizontal_Background_Border = new Vector4(9,9,0,0);
                 }
             else
                 {
                     guiSkin.border = Vector4ToRectOffset(style.Horizontal_Background_Border);
                 }




             if(style.Thumb_Vertical = skin.defaultSkin.verticalScrollbarThumb.normal.background)
                 {
                     guiSkin.border = new RectOffset(4,4,4,4);
                 }
             else
                 {
                     guiSkin.border = Vector4ToRectOffset(style.Vertical_Thumb_Border);   
                 }

             if(style.on_Thumb_Vertical = skin.defaultSkin.verticalScrollbarThumb.hover.background)
                 {
                     guiSkin.border = new RectOffset(4,4,4,4);
                 }
             else
                 {
                     guiSkin.border = Vector4ToRectOffset(style.Vertical_Thumb_Border);   
                 }



             if(style.Thumb_Horizontal = skin.defaultSkin.horizontalScrollbarThumb.normal.background)
                 {
                     guiSkin.border = new RectOffset(4,4,4,4);
                 }
             else
                 {
                     guiSkin.border = Vector4ToRectOffset(style.Vertical_Thumb_Border);   
                 }

             if(style.on_Thumb_Horizontal = skin.defaultSkin.horizontalScrollbarThumb.hover.background)
                 {
                     guiSkin.border = new RectOffset(6,6,4,4);
                 }
             else
                 {
                     guiSkin.border = Vector4ToRectOffset(style.Vertical_Thumb_Border);   
                 }




             guiSkin.stretchHeight = style.Vertical_Stretch_Height;
             guiSkin.stretchWidth = style.Vertical_Stretch_Width;

             guiSkin.fixedHeight = style.Vertical_Background_Fixed_Height;
             guiSkin.fixedWidth = style.Vertical_Background_Fixed_Width;

             guiSkin.fixedHeight = style.Vertical_Thumb_Fixed_Height;
             guiSkin.fixedWidth = style.Vertical_Thumb_Fixed_Width;

             guiSkin.clipping = style.Vertical_Text_clip;
             guiSkin.imagePosition = style.Vertical_Image_position;
             guiSkin.alignment = style.Vertical_Alignment;

             guiSkin.margin = Vector4ToRectOffset(style.Vertical_Background_margin);
             guiSkin.padding = Vector4ToRectOffset(style.Vertical_Background_padding);
             guiSkin.overflow = Vector4ToRectOffset(style.Vertical_Background_overflow);

             guiSkin.margin = Vector4ToRectOffset(style.Vertical_Thumb_margin);
             guiSkin.padding = Vector4ToRectOffset(style.Vertical_Thumb_padding);
             guiSkin.overflow = Vector4ToRectOffset(style.Vertical_Thumb_overflow);

             guiSkin.contentOffset = style.Vertical_Background_Contect_Offset;
             guiSkin.contentOffset = style.Vertical_Thumb_Contect_Offset;


             guiSkin.stretchHeight = style.Horizontal_Stretch_Height;
             guiSkin.stretchWidth = style.Horizontal_Stretch_Width;

             guiSkin.fixedHeight = style.Horizontal_Background_Fixed_Height;
             guiSkin.fixedWidth = style.Horizontal_Background_Fixed_Width;

             guiSkin.fixedHeight = style.Horizontal_Thumb_Fixed_Height;
             guiSkin.fixedWidth = style.Horizontal_Thumb_Fixed_Width;

             guiSkin.clipping = style.Horizontal_Text_clip;
             guiSkin.imagePosition = style.Horizontal_Image_position;
             guiSkin.alignment = style.Horizontal_Alignment;

             guiSkin.margin = Vector4ToRectOffset(style.Horizontal_Background_margin);
             guiSkin.padding = Vector4ToRectOffset(style.Horizontal_Background_padding);
             guiSkin.overflow = Vector4ToRectOffset(style.Horizontal_Background_overflow);

             guiSkin.margin = Vector4ToRectOffset(style.Horizontal_Thumb_margin);
             guiSkin.padding = Vector4ToRectOffset(style.Horizontal_Thumb_padding);
             guiSkin.overflow = Vector4ToRectOffset(style.Horizontal_Thumb_overflow);

             guiSkin.contentOffset = style.Horizontal_Background_Contect_Offset;
             guiSkin.contentOffset = style.Horizontal_Thumb_Contect_Offset;
         }*/


        private GUISkin AssignPropertiesScrollbar(GUISkin guiSkin, scrollbar_Style style)
        {
            // Create a new GUISkin
            //GUISkin guiSkin = ScriptableObject.CreateInstance<GUISkin>();

            //if user did not provide any gui texture, use unity default gui textures
            if (style.Background_Vertical == null)
            {
                guiSkin.verticalScrollbar.normal.background = skin.defaultSkin.verticalScrollbar.normal.background;
                //throw warning
                //Debug.LogWarning("[AtomicSkin]Scrollbar Style 'Normal Vertical' texture is missing. Using Unity's default Vertical Scrollbar texture.");
            }
            else
            {
                guiSkin.verticalScrollbar.normal.background = TextureColor(style.Background_Vertical, style.Background_Color, style.transparent);
            }

            if (style.On_Background_Vertical == null)
            {
                guiSkin.verticalScrollbar.hover.background = skin.defaultSkin.verticalScrollbar.hover.background;
                //throw warning
                //Debug.LogWarning("[AtomicSkin]Scrollbar Style 'onNormal Vertical' texture is missing. Using Unity's default Vertical Scrollbar texture.");
            }
            else
            {
                guiSkin.verticalScrollbar.hover.background = TextureColor(style.On_Background_Vertical, style.Background_Color, style.transparent);
            }



            if (style.Background_Horizontal == null)
            {
                guiSkin.horizontalScrollbar.normal.background = skin.defaultSkin.horizontalScrollbar.normal.background;
                //throw warning
                //Debug.LogWarning("[AtomicSkin]Scrollbar Style 'Normal Horizontal' texture is missing. Using Unity's default Horizontal Scrollbar texture.");
            }
            else
            {
                guiSkin.horizontalScrollbar.normal.background = TextureColor(style.Background_Horizontal, style.Background_Color, style.transparent);
            }

            if (style.On_Background_Horizontal == null)
            {
                guiSkin.horizontalScrollbar.hover.background = skin.defaultSkin.horizontalScrollbar.hover.background;
                //throw warning
                //Debug.LogWarning("[AtomicSkin]Scrollbar Style 'onNormal Horizontal' texture is missing. Using Unity's default Horizontal Scrollbar texture.");
            }
            else
            {
                guiSkin.horizontalScrollbar.hover.background = TextureColor(style.On_Background_Horizontal, style.Background_Color, style.transparent);
                guiSkin.horizontalScrollbarThumb.normal.background = TextureColor(style.Thumb_Horizontal, style.Thumb_Color, 1);
            }




            if (style.Thumb_Vertical == null)
            {
                guiSkin.verticalScrollbarThumb.normal.background = skin.defaultSkin.verticalScrollbarThumb.normal.background;
                //throw warning
                //Debug.LogWarning("[AtomicSkin]Scrollbar Style 'Thumb Vertical' texture is missing. Using Unity's default Vertical Scrollbar Thumb texture.");
            }
            else
            {
                guiSkin.verticalScrollbarThumb.normal.background = TextureColor(style.Thumb_Vertical, style.Thumb_Color, 1);
            }

            if (style.on_Thumb_Vertical == null)
            {
                guiSkin.verticalScrollbarThumb.hover.background = skin.defaultSkin.verticalScrollbarThumb.hover.background;
                //throw warning
                //Debug.LogWarning("[AtomicSkin]Scrollbar Style 'Thumb Vertical' texture is missing. Using Unity's default Vertical Scrollbar Thumb texture.");
            }
            else
            {
                guiSkin.verticalScrollbarThumb.hover.background = TextureColor(style.on_Thumb_Vertical, style.HoverThumb_Color, 1);
            }


            if (style.Thumb_Horizontal == null)
            {
                guiSkin.horizontalScrollbarThumb.normal.background = skin.defaultSkin.horizontalScrollbarThumb.normal.background;
                //throw warning
                //Debug.LogWarning("[AtomicSkin]Scrollbar Style 'Thumb Horizontal' texture is missing. Using Unity's default Horizontal Scrollbar Thumb texture.");
            }
            else
            {
                guiSkin.horizontalScrollbarThumb.normal.background = TextureColor(style.Thumb_Horizontal, style.Thumb_Color, 1);
            }

            if (style.on_Thumb_Horizontal == null)
            {
                guiSkin.horizontalScrollbarThumb.hover.background = skin.defaultSkin.horizontalScrollbarThumb.hover.background;
                //throw warning
                //Debug.LogWarning("[AtomicSkin]Scrollbar Style 'Thumb Horizontal' texture is missing. Using Unity's default Horizontal Scrollbar Thumb texture.");
            }
            else
            {
                guiSkin.horizontalScrollbarThumb.hover.background = TextureColor(style.on_Thumb_Horizontal, style.HoverThumb_Color, 1);
            }





            if (style.Background_Vertical == skin.defaultSkin.verticalScrollbar.normal.background)
            {
                guiSkin.verticalScrollbar.border = skin.defaultSkin.verticalScrollbar.border;
            }
            else
            {
                guiSkin.verticalScrollbar.border = Vector4ToRectOffset(style.Vertical_Background_Border);
            }

            if (style.On_Background_Vertical == skin.defaultSkin.verticalScrollbar.hover.background)
            {
                guiSkin.verticalScrollbar.border = skin.defaultSkin.verticalScrollbar.border;
            }
            else
            {
                guiSkin.verticalScrollbar.border = Vector4ToRectOffset(style.Vertical_Background_Border);
            }



            if (style.Background_Horizontal == skin.defaultSkin.horizontalScrollbar.normal.background)
            {
                guiSkin.horizontalScrollbar.border = skin.defaultSkin.horizontalScrollbar.border;
            }
            else
            {
                guiSkin.horizontalScrollbar.border = Vector4ToRectOffset(style.Horizontal_Background_Border);
            }

            if (style.On_Background_Horizontal == skin.defaultSkin.horizontalScrollbar.hover.background)
            {
                guiSkin.horizontalScrollbar.border = skin.defaultSkin.horizontalScrollbar.border;
            }
            else
            {
                guiSkin.horizontalScrollbar.border = Vector4ToRectOffset(style.Horizontal_Background_Border);
            }




            if (style.Thumb_Vertical == skin.defaultSkin.verticalScrollbarThumb.normal.background)
            {
                guiSkin.verticalScrollbarThumb.border = skin.defaultSkin.verticalScrollbarThumb.border;
            }
            else
            {
                guiSkin.verticalScrollbarThumb.border = Vector4ToRectOffset(style.Vertical_Thumb_Border);
            }

            if (style.on_Thumb_Vertical == skin.defaultSkin.verticalScrollbarThumb.hover.background)
            {
                guiSkin.verticalScrollbarThumb.border = skin.defaultSkin.verticalScrollbarThumb.border;
            }
            else
            {
                guiSkin.verticalScrollbarThumb.border = Vector4ToRectOffset(style.Vertical_Thumb_Border);
            }



            if (style.Thumb_Horizontal == skin.defaultSkin.horizontalScrollbarThumb.normal.background)
            {
                guiSkin.horizontalScrollbarThumb.border = skin.defaultSkin.horizontalScrollbarThumb.border;
            }
            else
            {
                guiSkin.horizontalScrollbarThumb.border = Vector4ToRectOffset(style.Vertical_Thumb_Border);
            }

            if (style.on_Thumb_Horizontal == skin.defaultSkin.horizontalScrollbarThumb.hover.background)
            {
                guiSkin.horizontalScrollbarThumb.border = skin.defaultSkin.horizontalScrollbarThumb.border;
            }
            else
            {
                guiSkin.horizontalScrollbarThumb.border = Vector4ToRectOffset(style.Vertical_Thumb_Border);
            }

            //vertical
            //guiSkin.verticalScrollbar.normal.background = TextureColor(style.Background, style.Background_Color, style.transparent);
            //guiSkin.verticalScrollbarThumb.normal.background = TextureColor(style.Thumb, style.Thumb_Color, 1);
            //guiSkin.verticalScrollbarThumb.hover.background = TextureColor(style.Thumb, style.HoverThumb_Color, 1);


            //guiSkin.verticalScrollbar.border = Vector4ToRectOffset(style.Vertical_Background_Border);
            //guiSkin.verticalScrollbarThumb.border = Vector4ToRectOffset(style.Vertical_Thumb_Border);

            guiSkin.verticalScrollbar.stretchHeight = style.Vertical_Stretch_Height;
            guiSkin.verticalScrollbar.stretchWidth = style.Vertical_Stretch_Width;

            guiSkin.verticalScrollbar.fixedHeight = style.Vertical_Background_Fixed_Height;
            guiSkin.verticalScrollbar.fixedWidth = style.Vertical_Background_Fixed_Width;

            guiSkin.verticalScrollbarThumb.fixedHeight = style.Vertical_Thumb_Fixed_Height;
            guiSkin.verticalScrollbarThumb.fixedWidth = style.Vertical_Thumb_Fixed_Width;

            guiSkin.verticalScrollbar.clipping = style.Vertical_Text_clip;
            guiSkin.verticalScrollbar.imagePosition = style.Vertical_Image_position;
            guiSkin.verticalScrollbar.alignment = style.Vertical_Alignment;

            guiSkin.verticalScrollbar.margin = Vector4ToRectOffset(style.Vertical_Background_margin);
            guiSkin.verticalScrollbar.padding = Vector4ToRectOffset(style.Vertical_Background_padding);
            guiSkin.verticalScrollbar.overflow = Vector4ToRectOffset(style.Vertical_Background_overflow);

            guiSkin.verticalScrollbarThumb.margin = Vector4ToRectOffset(style.Vertical_Thumb_margin);
            guiSkin.verticalScrollbarThumb.padding = Vector4ToRectOffset(style.Vertical_Thumb_padding);
            guiSkin.verticalScrollbarThumb.overflow = Vector4ToRectOffset(style.Vertical_Thumb_overflow);

            guiSkin.verticalScrollbar.contentOffset = style.Vertical_Background_Contect_Offset;
            guiSkin.verticalScrollbarThumb.contentOffset = style.Vertical_Thumb_Contect_Offset;



            //horizontal
            //guiSkin.horizontalScrollbar.normal.background = TextureColor(style.Background, style.Background_Color, style.transparent);
            //guiSkin.horizontalScrollbarThumb.normal.background = TextureColor(style.Thumb, style.Thumb_Color, 1);
            //guiSkin.horizontalScrollbarThumb.hover.background = TextureColor(style.Thumb, style.HoverThumb_Color, 1);


            //guiSkin.horizontalScrollbar.border = Vector4ToRectOffset(style.Horizontal_Background_Border);
            //guiSkin.horizontalScrollbarThumb.border = Vector4ToRectOffset(style.Horizontal_Thumb_Border);

            guiSkin.horizontalScrollbar.stretchHeight = style.Horizontal_Stretch_Height;
            guiSkin.horizontalScrollbar.stretchWidth = style.Horizontal_Stretch_Width;

            guiSkin.horizontalScrollbar.fixedHeight = style.Horizontal_Background_Fixed_Height;
            guiSkin.horizontalScrollbar.fixedWidth = style.Horizontal_Background_Fixed_Width;

            guiSkin.horizontalScrollbarThumb.fixedHeight = style.Horizontal_Thumb_Fixed_Height;
            guiSkin.horizontalScrollbarThumb.fixedWidth = style.Horizontal_Thumb_Fixed_Width;

            guiSkin.horizontalScrollbar.clipping = style.Horizontal_Text_clip;
            guiSkin.horizontalScrollbar.imagePosition = style.Horizontal_Image_position;
            guiSkin.horizontalScrollbar.alignment = style.Horizontal_Alignment;

            guiSkin.horizontalScrollbar.margin = Vector4ToRectOffset(style.Horizontal_Background_margin);
            guiSkin.horizontalScrollbar.padding = Vector4ToRectOffset(style.Horizontal_Background_padding);
            guiSkin.horizontalScrollbar.overflow = Vector4ToRectOffset(style.Horizontal_Background_overflow);

            guiSkin.horizontalScrollbarThumb.margin = Vector4ToRectOffset(style.Horizontal_Thumb_margin);
            guiSkin.horizontalScrollbarThumb.padding = Vector4ToRectOffset(style.Horizontal_Thumb_padding);
            guiSkin.horizontalScrollbarThumb.overflow = Vector4ToRectOffset(style.Horizontal_Thumb_overflow);

            guiSkin.horizontalScrollbar.contentOffset = style.Horizontal_Background_Contect_Offset;
            guiSkin.horizontalScrollbarThumb.contentOffset = style.Horizontal_Thumb_Contect_Offset;

            //remove
            guiSkin.verticalScrollbarDownButton = GUIStyle.none;
            guiSkin.verticalScrollbarUpButton = GUIStyle.none;
            guiSkin.horizontalScrollbarLeftButton = GUIStyle.none;
            guiSkin.horizontalScrollbarRightButton = GUIStyle.none;

            return guiSkin;
        }


        Texture2D TextureColor(Texture2D texture, Color color, float transparency)
        {
            Texture2D newTexture = new Texture2D(texture.width, texture.height);
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    Color pixelColor = texture.GetPixel(x, y);
                    color.a = transparency;
                    newTexture.SetPixel(x, y, pixelColor * color);
                }
            }
            newTexture.Apply();
            return newTexture;
        }



        private RectOffset Vector4ToRectOffset(Vector4 vector)
        {
            return new RectOffset((int)vector.x, (int)vector.y, (int)vector.z, (int)vector.w);
        }




        private void OnDestroy()
        {
            AtomicDebug.OnLogReceived -= HandleLog;
            Application.logMessageReceived -= UnityHandleLog;
        }


        //TODO make it at start instead for more perfomance
        private void HandleLog(string logString, AtomicDebug.LogType type)
        {
            if (!isVisible)
            {
                if (!SleepMode)
                    return;
            }

            Color logColor;
            string prefix = "";
            switch (type)
            {
                case AtomicDebug.LogType.CommandWarning:
                    logColor = skin.Output_CommandWarning;
                    prefix = (skin.logTypesToShow & AtomicSkin.LogTypes.CommandWarning) != 0 ? $"[{type}]" : "";
                    break;
                case AtomicDebug.LogType.CommandError:
                    logColor = skin.Output_CommandError;
                    prefix = (skin.logTypesToShow & AtomicSkin.LogTypes.CommandError) != 0 ? $"[{type}]" : "";
                    break;
                case AtomicDebug.LogType.CommandException:
                    logColor = skin.Output_CommandException;
                    prefix = (skin.logTypesToShow & AtomicSkin.LogTypes.CommandException) != 0 ? $"[{type}]" : "";
                    break;
                case AtomicDebug.LogType.Command:
                    logColor = skin.Output_Command;
                    prefix = (skin.logTypesToShow & AtomicSkin.LogTypes.Command) != 0 ? $"[{type}]" : "";
                    break;
                case AtomicDebug.LogType.Console:
                    logColor = skin.Output_Console;
                    prefix = (skin.logTypesToShow & AtomicSkin.LogTypes.Console) != 0 ? $"[{type}]" : "";
                    break;
                case AtomicDebug.LogType.Network:
                    logColor = skin.Output_Network;
                    prefix = (skin.logTypesToShow & AtomicSkin.LogTypes.Network) != 0 ? $"[{type}]" : "";
                    break;
                case AtomicDebug.LogType.NetworkError:
                    logColor = skin.Output_NetworkError;
                    prefix = (skin.logTypesToShow & AtomicSkin.LogTypes.NetworkError) != 0 ? $"[{type}]" : "";
                    break;
                case AtomicDebug.LogType.NetworkWarning:
                    logColor = skin.Output_NetworkWarning;
                    prefix = (skin.logTypesToShow & AtomicSkin.LogTypes.NetworkWarning) != 0 ? $"[{type}]" : "";
                    break;
                case AtomicDebug.LogType.Object:
                    logColor = skin.Output_Object;
                    prefix = (skin.logTypesToShow & AtomicSkin.LogTypes.Object) != 0 ? $"[{type}]" : "";
                    break;
                case AtomicDebug.LogType.Material:
                    logColor = skin.Output_Material;
                    prefix = (skin.logTypesToShow & AtomicSkin.LogTypes.Material) != 0 ? $"[{type}]" : "";
                    break;
                case AtomicDebug.LogType.Info:
                    logColor = skin.Output_Info;
                    prefix = (skin.logTypesToShow & AtomicSkin.LogTypes.Info) != 0 ? $"[{type}]" : "";
                    break;
                case AtomicDebug.LogType.System:
                    logColor = skin.Output_System;
                    prefix = (skin.logTypesToShow & AtomicSkin.LogTypes.System) != 0 ? $"[{type}]" : "";
                    break;
                case AtomicDebug.LogType.Audio:
                    logColor = skin.Output_Audio;
                    prefix = (skin.logTypesToShow & AtomicSkin.LogTypes.Audio) != 0 ? $"[{type}]" : "";
                    break;
                case AtomicDebug.LogType.Critical:
                    logColor = skin.Output_Critical;
                    prefix = (skin.logTypesToShow & AtomicSkin.LogTypes.Critical) != 0 ? $"[{type}]" : "";
                    break;
                case AtomicDebug.LogType.GameState:
                    logColor = skin.Output_GameState;
                    prefix = (skin.logTypesToShow & AtomicSkin.LogTypes.GameState) != 0 ? $"[{type}]" : "";
                    break;
                case AtomicDebug.LogType.Physics:
                    logColor = skin.Output_Physics;
                    prefix = (skin.logTypesToShow & AtomicSkin.LogTypes.Physics) != 0 ? $"[{type}]" : "";
                    break;
                case AtomicDebug.LogType.AI:
                    logColor = skin.Output_AI;
                    prefix = (skin.logTypesToShow & AtomicSkin.LogTypes.AI) != 0 ? $"[{type}]" : "";
                    break;
                case AtomicDebug.LogType.Input:
                    logColor = skin.Output_Input;
                    prefix = (skin.logTypesToShow & AtomicSkin.LogTypes.Input) != 0 ? $"[{type}]" : "";
                    break;
                default:
                    logColor = skin.Output_Log;
                    prefix = (skin.logTypesToShow & AtomicSkin.LogTypes.Log) != 0 ? $"[{type}]" : "";
                    break;
            } //TODO make prefix flag for fill color
            if (skin.logPrefixType == AtomicSkin.LogPrefixType.DateTime)
            {
                string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                consoleLogs.Add($"[{timestamp}] {prefix} <color=#{UnityEngine.ColorUtility.ToHtmlStringRGBA(logColor)}>{logString}</color>");
            }
            else if (skin.logPrefixType == AtomicSkin.LogPrefixType.Null)
            {
                consoleLogs.Add($"{prefix} <color=#{UnityEngine.ColorUtility.ToHtmlStringRGBA(logColor)}>{logString}</color>");
            }
            else
            {
                string customPrefix = skin.logPrefix;
                consoleLogs.Add($"{customPrefix} {prefix} <color=#{UnityEngine.ColorUtility.ToHtmlStringRGBA(logColor)}>{logString}</color>".Trim());
            }

            scrollPos.y = float.MaxValue;
            if (consoleLogs.Count > skin.MaxCommandHistory)
            {
                consoleLogs.RemoveAt(0);
            }
            logsChanged = true;
        }


        private void UnityHandleLog(string logString, string stackTrace, LogType type)
        {
            if (!isVisible)
            {
                if (!SleepMode)
                    return;
            }

            Color logColor;
            string prefix = "";
            switch (type)
            {
                case LogType.Warning:
                    logColor = skin.Output_Warning;
                    if ((skin.logTypesToShow & AtomicSkin.LogTypes.Warning) != 0)
                        prefix = "[Warning]";
                    else
                        prefix = "";
                    break;
                case LogType.Error:
                    logColor = skin.Output_Error;
                    if ((skin.logTypesToShow & AtomicSkin.LogTypes.Error) != 0)
                        prefix = "[Error]";
                    else
                        prefix = "";
                    break;
                case LogType.Exception:
                    logColor = skin.Output_Exception;
                    if ((skin.logTypesToShow & AtomicSkin.LogTypes.Exception) != 0)
                        prefix = "[Exception]";
                    else
                        prefix = "";
                    break;
                default:
                    logColor = skin.Output_Log;
                    if ((skin.logTypesToShow & AtomicSkin.LogTypes.Log) != 0)
                        prefix = "[Log]";
                    else
                        prefix = "";
                    break;
            }


            if (skin.logPrefixType == AtomicSkin.LogPrefixType.DateTime)
            {
                string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                consoleLogs.Add($"[{timestamp}] {prefix} <color=#{UnityEngine.ColorUtility.ToHtmlStringRGBA(logColor)}>{logString}</color>");
            }
            else if (skin.logPrefixType == AtomicSkin.LogPrefixType.Null)
            {
                consoleLogs.Add($"{prefix} <color=#{UnityEngine.ColorUtility.ToHtmlStringRGBA(logColor)}>{logString}</color>");
            }
            else
            {
                string customPrefix = skin.logPrefix;
                consoleLogs.Add($"{customPrefix} {prefix} <color=#{UnityEngine.ColorUtility.ToHtmlStringRGBA(logColor)}>{logString}</color>".Trim());
            }

            scrollPos.y = float.MaxValue;
            if (consoleLogs.Count > skin.MaxCommandHistory)
            {
                consoleLogs.RemoveAt(0);
            }
            logsChanged = true;
        }

        private void UpdateSuggestion()
        {
            filteredCommandList.Clear();
            filteredSetList.Clear();

            if (!string.IsNullOrEmpty(inputText))
            {
                string[] allCommands = inputText.Split(';');
                string lastCommand = allCommands.Last().Trim();

                if (!string.IsNullOrEmpty(lastCommand))
                {
                    string[] parts = lastCommand.Split('.');
                    string commandGroup = parts[0].Trim();
                    string commandWithParameters = parts.Length > 1 ? parts[1].Trim() : commandGroup;

                    if (parts.Length == 1)
                    {
                        // Suggest command groups and commands without a group
                        foreach (var (method, instance) in commandMethods)
                        {
                            AtomicCommandAttribute commandAttribute = method.GetCustomAttribute<AtomicCommandAttribute>();
                            if (commandAttribute != null)
                            {
                                if (string.IsNullOrEmpty(commandAttribute.Group) && commandAttribute.Name.StartsWith(commandGroup, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    filteredCommandList.Add(method);
                                }
                                else if (commandAttribute.Group.StartsWith(commandGroup, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    if (!filteredCommandList.Any(m => m.GetCustomAttribute<AtomicCommandAttribute>().Group == commandAttribute.Group))
                                    {
                                        filteredCommandList.Add(method);
                                    }
                                }
                            }
                        }

                        // Suggest set fields and set field groups
                        foreach (var (field, instance) in setFields)
                        {
                            AtomicSetAttribute setAttribute = field.GetCustomAttribute<AtomicSetAttribute>();
                            if (setAttribute != null)
                            {
                                if (string.IsNullOrEmpty(setAttribute.Group) && setAttribute.Name.StartsWith(commandGroup, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    filteredSetList.Add(field);
                                }
                                else if (setAttribute.Group.StartsWith(commandGroup, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    if (!filteredSetList.Any(f => f.GetCustomAttribute<AtomicSetAttribute>().Group == setAttribute.Group))
                                    {
                                        filteredSetList.Add(field);
                                    }
                                }
                            }
                        }
                    }
                    else if (parts.Length == 2)
                    {
                        // Suggest commands and set fields within the specified group
                        foreach (var (method, instance) in commandMethods)
                        {
                            AtomicCommandAttribute commandAttribute = method.GetCustomAttribute<AtomicCommandAttribute>();
                            if (commandAttribute != null && commandAttribute.Group.Equals(commandGroup, StringComparison.InvariantCultureIgnoreCase) && commandAttribute.Name.StartsWith(commandWithParameters, StringComparison.InvariantCultureIgnoreCase))
                            {
                                filteredCommandList.Add(method);
                            }
                        }

                        foreach (var (field, instance) in setFields)
                        {
                            AtomicSetAttribute setAttribute = field.GetCustomAttribute<AtomicSetAttribute>();
                            if (setAttribute != null && setAttribute.Group.Equals(commandGroup, StringComparison.InvariantCultureIgnoreCase) && setAttribute.Name.StartsWith(commandWithParameters, StringComparison.InvariantCultureIgnoreCase))
                            {
                                filteredSetList.Add(field);
                            }
                        }
                    }
                }
            }
        }



        private int currentSuggestionIndex = -1;
        private void DisplayFilteredCommandList()
        {
            if (filteredCommandList.Count > 0 || filteredSetList.Count > 0)
            {
                GUILayout.BeginVertical();
                bool tabPressed = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Tab;

                // Combine both lists into a single list of objects
                var combinedList = new List<object>();
                combinedList.AddRange(filteredCommandList.Cast<object>());
                combinedList.AddRange(filteredSetList.Cast<object>());


                //TODO fix in next update, if user press tab fast enough it goes to locked command!
                if (tabPressed && currentSuggestionIndex == -1 && combinedList.Count > 0)
                {
                    currentSuggestionIndex = 0;
                    // Handle tab press logic here for the first item in the combined list
                    object firstItem = combinedList[0];
                    if (firstItem is MethodInfo method)
                    {
                        AtomicCommandAttribute commandAttribute = method.GetCustomAttribute<AtomicCommandAttribute>();
                        UpdateInputText(commandAttribute, inputText.Split(';').Last().Trim());
                    }
                    else if (firstItem is FieldInfo field)
                    {
                        AtomicSetAttribute setAttribute = field.GetCustomAttribute<AtomicSetAttribute>();
                        // Update input text for set (implement this function similar to UpdateInputText for commands)
                        UpdateInputTextForSet(setAttribute, inputText.Split(';').Last().Trim());
                    }
                    return;
                }

                int displayedSuggestions = 0;
                string lastCommand = inputText.Split(';').Last().Trim();

                // Display combined list
                for (int i = 0; i < combinedList.Count; i++)
                {
                    object item = combinedList[i];
                    string displayText = "";

                    if (item is MethodInfo method)
                    {
                        AtomicCommandAttribute commandAttribute = method.GetCustomAttribute<AtomicCommandAttribute>();
                        if (Instance.isLocked && commandAttribute.PasswordProtected)
                        {
                            continue;
                        }
                        displayText = GetCommandWithParameters(commandAttribute, method, lastCommand);
                    }
                    else if (item is FieldInfo field)
                    {
                        AtomicSetAttribute setAttribute = field.GetCustomAttribute<AtomicSetAttribute>();
                        if (Instance.isLocked && setAttribute.PasswordProtected)
                        {
                            continue;
                        }
                        displayText = GetSetWithParameters(setAttribute, field, lastCommand);
                        //displayText = $"{setAttribute.Group}.{setAttribute.Name}";
                    }

                    CommandStyle.normal.background = (i == currentSuggestionIndex) ? highlightedBackground : normalBackground;

                    if (GUILayout.Button(displayText, CommandStyle) || (tabPressed && i == currentSuggestionIndex))
                    {
                        // Handle selection logic here
                        if (item is MethodInfo)
                        {
                            AtomicCommandAttribute commandAttribute = ((MethodInfo)item).GetCustomAttribute<AtomicCommandAttribute>();
                            UpdateInputText(commandAttribute, lastCommand);
                        }
                        else if (item is FieldInfo)
                        {
                            AtomicSetAttribute setAttribute = ((FieldInfo)item).GetCustomAttribute<AtomicSetAttribute>();
                            // Update input text for set (implement this function similar to UpdateInputText for commands)
                            UpdateInputTextForSet(setAttribute, lastCommand);
                        }
                        break;
                    }

                    displayedSuggestions++;
                    if (displayedSuggestions >= maxSuggestions)
                    {
                        break;
                    }
                }
                GUILayout.EndVertical();
            }
        }



        private string GetCommandWithParameters(AtomicCommandAttribute commandAttribute, MethodInfo methodInfo, string lastCommand)
        {
            string commandGroup = commandAttribute.Group;
            string commandName = commandAttribute.Name;

            // Get the parameters of the method
            ParameterInfo[] parameters = methodInfo.GetParameters();

            string parameterDescription = "";
            if (skin.ShowArgName)
            {
                parameterDescription = parameters.Length > 0 ?
                 $" : {string.Join(", ", parameters.Select(p => $"{AtomicTypeMapping.GetType(p.ParameterType)} {p.Name}"))}" :
                 "";
            }
            else
            {
                parameterDescription = parameters.Length > 0 ?
                $" : {string.Join(", ", parameters.Select(p => $"{AtomicTypeMapping.GetType(p.ParameterType)}"))}" :
                "";
            }

            return string.IsNullOrEmpty(commandGroup) ?
                $"{commandName}{parameterDescription}" :
                (lastCommand.Contains(".") ? $"{commandGroup}.{commandName}{parameterDescription}" : $"{commandGroup} : group");
        }


        private string GetSetWithParameters(AtomicSetAttribute setAttribute, FieldInfo fieldInfo, string lastCommand)
        {
            string setGroup = setAttribute.Group;
            string setName = setAttribute.Name;
            string fieldType = AtomicTypeMapping.GetType(fieldInfo.FieldType);

            return string.IsNullOrEmpty(setGroup) ?
                $"{setName} : {fieldType}" :
                (lastCommand.Contains(".") ? $"{setGroup}.{setName} : {fieldType}" : $"{setGroup} : group");
        }


        private void UpdateInputText(AtomicCommandAttribute commandAttribute, string lastCommand)
        {
            string newCommand;
            string commandGroup = commandAttribute.Group;
            string commandName = commandAttribute.Name;

            if (lastCommand.EndsWith(".") && !string.IsNullOrEmpty(commandGroup) && lastCommand.Contains(commandGroup + "."))
            {
                newCommand = $"{commandGroup}.{commandName} ";
            }
            else if (!string.IsNullOrEmpty(commandGroup) && lastCommand.StartsWith(commandGroup + "."))
            {
                newCommand = $"{commandGroup}.{commandName} ";
            }
            else
            {
                newCommand = string.IsNullOrEmpty(commandGroup) ? commandName + " " : commandGroup + ".";
            }

            string[] existingCommands = inputText.Split(';');
            existingCommands[existingCommands.Length - 1] = newCommand;
            inputText = string.Join(";", existingCommands);

            filteredCommandList.Clear();
            filteredSetList.Clear();
            currentSuggestionIndex = -1;
        }

        private void UpdateInputTextForSet(AtomicSetAttribute setAttribute, string lastCommand)
        {
            string newCommand;
            string setGroup = setAttribute.Group;
            string setName = setAttribute.Name;

            if (lastCommand.EndsWith(".") && !string.IsNullOrEmpty(setGroup) && lastCommand.Contains(setGroup + "."))
            {
                newCommand = $"{setGroup}.{setName} ";
            }
            else if (!string.IsNullOrEmpty(setGroup) && lastCommand.StartsWith(setGroup + "."))
            {
                newCommand = $"{setGroup}.{setName} ";
            }
            else
            {
                newCommand = string.IsNullOrEmpty(setGroup) ? setName + " " : setGroup + ".";
            }

            string[] existingCommands = inputText.Split(';');
            existingCommands[existingCommands.Length - 1] = newCommand;
            inputText = string.Join(";", existingCommands);

            filteredCommandList.Clear();
            filteredSetList.Clear();
            currentSuggestionIndex = -1;
        }

        private string highlightedSuggestion = "";
        private void UpdateInputWithSuggestion()
        {
            if (currentSuggestionIndex >= 0)
            {
                if (currentSuggestionIndex < filteredCommandList.Count)
                {
                    MethodInfo method = filteredCommandList[currentSuggestionIndex];
                    AtomicCommandAttribute commandAttribute = method.GetCustomAttribute<AtomicCommandAttribute>();
                    if (commandAttribute != null)
                    {
                        string commandGroup = commandAttribute.Group;
                        string commandName = commandAttribute.Name;

                        // Update highlightedSuggestion with the selected command suggestion
                        highlightedSuggestion = string.IsNullOrEmpty(commandGroup) ? commandName : $"{commandGroup}.{commandName}";
                    }
                }
                else if (currentSuggestionIndex - filteredCommandList.Count < filteredSetList.Count)
                {
                    int indexForSet = currentSuggestionIndex - filteredCommandList.Count;
                    FieldInfo field = filteredSetList[indexForSet];
                    AtomicSetAttribute setAttribute = field.GetCustomAttribute<AtomicSetAttribute>();
                    if (setAttribute != null)
                    {
                        string setGroup = setAttribute.Group;
                        string setName = setAttribute.Name;

                        // Update highlightedSuggestion with the selected set suggestion
                        highlightedSuggestion = string.IsNullOrEmpty(setGroup) ? setName : $"{setGroup}.{setName}";
                    }
                }
            }
        }


        private void OnGUI()
        {
            if(!SkipFrameUpdate) { return; }

            // Exit early if the console is not visible
            if (!isVisible)
            {
                GUI.FocusControl(null);
                GUI.UnfocusWindow();
                GUI.SetNextControlName(null);
                return;
            }


            // Statement for commands to display diagnostic tool.
            if (Diagnostic)
            {
                string textFPS = string.Format("FPS: {0:0.}", fps);
                string textMS = string.Format("MS: {0:0.0}", msec);
                string textMemory = string.Format("Memory: {0} MB", memoryUsage);

                GUIStyle style = new GUIStyle();
                style.fontSize = 13;
                style.normal.textColor = Color.white;

                int lineHeight = 30;
                int xOffset = Screen.width - 210;
                int yOffset = 10;

                GUI.Box(new Rect(xOffset - 10, yOffset - 10, 220, 150), "Diagnostic");

                GUI.Label(new Rect(xOffset, yOffset + 25, 200, lineHeight), textFPS, style);
                GUI.Label(new Rect(xOffset, yOffset + 25 + lineHeight, 200, lineHeight), textMS, style);
                GUI.Label(new Rect(xOffset, yOffset + 25 + 2 * lineHeight, 200, lineHeight), textMemory, style);
            }


            windowRect = GUILayout.Window(0, windowRect, ConsoleWindow, "", logLabelStyle);


            if (setFocusOnNextFrame)
            {
                GUI.FocusControl("InputTextField");
                setFocusOnNextFrame = false;
            }
        }

        private string CustomTextField(string value, GUIStyle style, params GUILayoutOption[] options)
        {
            int controlId = GUIUtility.GetControlID(FocusType.Keyboard);
            bool hasFocus = controlId == GUIUtility.keyboardControl;

            value = GUILayout.TextField(value, style, options);

            if (hasFocus)
            {
                TextEditor textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), controlId);

                if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Tab)
                {
                    int newPosition = value.Length;
                    textEditor.cursorIndex = newPosition;
                    textEditor.selectIndex = newPosition;
                }
            }

            return value;
        }

        //TODO make if search history make caret on last spot!
        private string CaretPlacement(string value, GUIStyle style, params GUILayoutOption[] options)
        {
            int controlId = GUIUtility.GetControlID(FocusType.Keyboard);
            bool hasFocus = controlId == GUIUtility.keyboardControl;

            value = GUILayout.TextField(value, style, options);

            if (hasFocus)
            {
                TextEditor textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), controlId);

                int newPosition = value.Length;
                textEditor.cursorIndex = newPosition;
                textEditor.selectIndex = newPosition;
            }

            return value;
        }



        public bool logsChanged = false;
        public string concatenatedLogs = "";

        private void UpdateLogs()
        {
            if (logsChanged)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string log in consoleLogs)
                {
                    sb.AppendLine(log);
                }
                concatenatedLogs = sb.ToString();
                logsChanged = false;
            }
        }

        private void AddLog(string newLog)
        {
            consoleLogs.Add(newLog);
            logsChanged = true;
        }





        private string lastInputText = "";
        bool HistorySearch = false;
        public string actualPassword = "";
        private string maskedInputText = "";
        private bool CheckLogin = false;
        private Color storeCaretColor;

        private void ConsoleWindow(int windowID)
        {
            if (!GUIskinReady)
                return;

            float headerHeight = skin.Header_Style.HeaderSize;



            GUI.Box(new Rect(0, 0, windowRect.width, headerHeight), "", headerStyle);
            GUI.Label(new Rect(0, 0, windowRect.width, headerHeight), HeaderName, headerStyle);

            if (skin.ShowCloseBtn)
            {
                if (GUI.Button(new Rect(windowRect.width - skin.CloseButton_Style.Button_Rect.x,
                skin.CloseButton_Style.Button_Rect.y, skin.CloseButton_Style.Button_Rect.width, skin.CloseButton_Style.Button_Rect.height),
                skin.CloseButton_Style.text, closeButtonStyle))
                {
                    StartCoroutine(ForceClose());
                    //isVisible = false;
                }
            }

            GUI.DragWindow(new Rect(0, 0, windowRect.width - 10, headerHeight));

            GUILayout.BeginVertical();
            GUILayout.Space(headerHeight);

            GUI.skin = scrollBarStyle;
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));


            GUI.skin.settings.cursorColor = skin.CaretColor;
            GUI.skin.settings.selectionColor = skin.SelectionColor;
            GUI.skin.settings.cursorFlashSpeed = skin.CaretSpeed;




            //login function
            /* if (inputText.StartsWith("Login "))
             {
                 UpdateMaskedInput();
             }
         */
            //Update logs if they have changed
            UpdateLogs();

            if (invidiualLogs)
            {
                foreach (string log in consoleLogs)
                {
                    if (!string.IsNullOrEmpty(log))
                    {
                        GUILayout.Label(log, outputTextAreaStyle, GUILayout.ExpandWidth(true), GUILayout.Height(outputTextAreaStyle.CalcHeight(new GUIContent(log), windowRect.width - 25)));
                    }
                }
            }
            else
            {
                GUILayout.Label(concatenatedLogs, outputTextAreaStyle, GUILayout.ExpandWidth(true), GUILayout.Height(outputTextAreaStyle.CalcHeight(new GUIContent(concatenatedLogs), windowRect.width - 25)));
            }


            GUILayout.EndScrollView();

            GUILayout.Space(10);


            GUILayout.BeginHorizontal();


            if (Event.current.type == EventType.KeyUp)
            {
                if (Event.current.keyCode == KeyCode.Tab)
                {
                    inputText = CustomTextField(inputText, inputStyle, GUILayout.ExpandWidth(true));
                    HistorySearch = false;
                }

                int totalSuggestions = filteredCommandList.Count + filteredSetList.Count;

                if (Event.current.keyCode == KeyCode.UpArrow)
                {
                    if (HistorySearch)
                    {
                        if (commandHistoryIndex > 0)
                        {
                            commandHistoryIndex--;
                            inputText = commandHistory[commandHistoryIndex];
                            inputText = CustomTextField(inputText, inputStyle, GUILayout.ExpandWidth(true));
                        }
                    }
                    else if (string.IsNullOrEmpty(inputText))
                    {
                        if (commandHistoryIndex > 0)
                        {
                            HistorySearch = true;
                            commandHistoryIndex--;
                            inputText = commandHistory[commandHistoryIndex];
                            inputText = CustomTextField(inputText, inputStyle, GUILayout.ExpandWidth(true));
                        }
                    }
                    else
                    {
                        if (currentSuggestionIndex <= 0)
                        {
                            currentSuggestionIndex = Mathf.Min(totalSuggestions - 1, skin.MaxSuggestion - 1);
                        }
                        else
                        {
                            currentSuggestionIndex--;
                        }
                        UpdateInputWithSuggestion();
                    }
                }
                else if (Event.current.keyCode == KeyCode.DownArrow)
                {
                    if (HistorySearch)
                    {
                        if (commandHistoryIndex < commandHistory.Count - 1)
                        {
                            commandHistoryIndex++;
                            inputText = commandHistory[commandHistoryIndex];
                            inputText = CustomTextField(inputText, inputStyle, GUILayout.ExpandWidth(true));
                        }
                    }
                    else if (string.IsNullOrEmpty(inputText))
                    {
                        if (commandHistoryIndex < commandHistory.Count - 1)
                        {
                            HistorySearch = true;
                            commandHistoryIndex++;
                            inputText = commandHistory[commandHistoryIndex];
                            inputText = CustomTextField(inputText, inputStyle, GUILayout.ExpandWidth(true));
                        }
                    }
                    else
                    {
                        if (currentSuggestionIndex >= Mathf.Min(totalSuggestions - 1, skin.MaxSuggestion - 1))
                        {
                            currentSuggestionIndex = 0;
                        }
                        else
                        {
                            currentSuggestionIndex++;
                        }
                        UpdateInputWithSuggestion();
                    }
                }
                else
                {
                    inputText = GUILayout.TextField(inputText, inputStyle, GUILayout.ExpandWidth(true));
                    HistorySearch = false;
                }
            }

            else
            {
                //force to close console even if alwaysonfocus in enable
                if (AlwaysOnFocus && Event.current.type == EventType.KeyDown && Event.current.keyCode == ToggleConsoleInput && isVisible)
                {
                    ToggleConsole(!isVisible);
                    Event.current.Use();
                    return;
                }


                if (AlwaysOnFocus)
                {
                    GUI.SetNextControlName("InputTextField");

                    if (inputText.StartsWith(">> ") && isLocked && password_Protected)
                    {
                        CheckLogin = true;
                        string enteredPassword = inputText.Substring(">> ".Length);
                        actualPassword = enteredPassword;
                        string maskedPassword = new string('*', enteredPassword.Length);
                        maskedInputText = "Password: " + maskedPassword;

                        // Display the masked input and hide actual input (TODO if there is still somewhere on the screen fix it)
                        GUIStyle invisibleStyle = new GUIStyle();
                        invisibleStyle.normal.textColor = new Color(0, 0, 0, 0);
                        skin.CaretColor = new Color(0, 0, 0, 0);
                        inputText = GUILayout.TextField(inputText, invisibleStyle, GUILayout.Width(0), GUILayout.Height(0));
                        maskedInputText = "Password: " + GUILayout.TextField(maskedInputText, inputStyle, GUILayout.ExpandWidth(true));
                    }
                    else
                    {
                        if (CheckLogin)
                            CheckLogin = false;
                        skin.CaretColor = storeCaretColor;
                        inputText = GUILayout.TextField(inputText, inputStyle, GUILayout.ExpandWidth(true));
                    }

                    GUI.FocusControl("InputTextField");
                }
                else
                {
                    if (inputText.StartsWith(">> ") && isLocked && password_Protected)
                    {
                        CheckLogin = true;
                        string enteredPassword = inputText.Substring(">> ".Length);
                        actualPassword = enteredPassword;
                        string maskedPassword = new string('*', enteredPassword.Length);
                        maskedInputText = "Password: " + maskedPassword;

                        // Display the masked input and hide actual input (TODO if there is still somewhere on the screen fix it)
                        GUIStyle invisibleStyle = new GUIStyle();
                        invisibleStyle.normal.textColor = new Color(0, 0, 0, 0);
                        skin.CaretColor = new Color(0, 0, 0, 0);
                        inputText = GUILayout.TextField(inputText, invisibleStyle, GUILayout.Width(0), GUILayout.Height(0));
                        maskedInputText = "Password: " + GUILayout.TextField(maskedInputText, inputStyle, GUILayout.ExpandWidth(true));
                    }
                    else
                    {
                        if (CheckLogin)
                            CheckLogin = false;
                        skin.CaretColor = storeCaretColor;
                        inputText = GUILayout.TextField(inputText, inputStyle, GUILayout.ExpandWidth(true));
                    }
                    //inputText = GUILayout.TextField(inputText, inputStyle, GUILayout.ExpandWidth(true));
                }
            }

            // Only call UpdateSuggestion if inputText has changed
            if (inputText != lastInputText)
            {
                UpdateSuggestion();
                lastInputText = inputText;  // Update lastInputText
            }

            if (skin.Show_Send_Button && GUILayout.Button(Btn_Send, buttonStyle, GUILayout.Width(50)))
            {
                if (inputText == "")
                {
                    AtomicDebug.Error("Type 'help' for available command list.");
                    return;
                }
                else
                    SendMessage();
            }

            if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)
            {
                if (inputText == "")
                {
                    AtomicDebug.Error("Type 'help' for available command list.");
                    return;
                }
                else
                    SendMessage();
            }

            if (skin.Show_Clear_Button && GUILayout.Button(Btn_Clear, buttonStyle, GUILayout.Width(50)) || (Event.current.isKey && Event.current.keyCode == KeyCode.Delete))
            {
                consoleLogs.Clear();
                concatenatedLogs = "";
                logsChanged = true;
            }

            GUILayout.EndHorizontal();

            DisplayFilteredCommandList();

            GUILayout.EndVertical();

            if (skin.CanResize)
            {
                ResizeWindow();
            }
        }


        private void SendMessage()
        {
            if (CheckLogin)
            {
                LoginSystem(actualPassword);
                CheckLogin = false;
                actualPassword = "";
                inputText = "";
                maskedInputText = "";
                return;
            }
            string[] commands = inputText.Split(';');
            foreach (string command in commands)
            {
                string trimmedCommand = command.Trim();
                string[] parts = trimmedCommand.Split('.');
                string commandGroup = parts[0];
                string commandWithParameters = parts.Length > 1 ? parts[1] : commandGroup;
                var matches = Regex.Matches(commandWithParameters, @"[\""].+?[\""]|[^ ]+");
                string[] commandParts = matches.Cast<Match>().Select(m => m.Value).ToArray();
                string commandName = commandParts[0];
                string[] parameters = commandParts.Skip(1).ToArray();
                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i] = parameters[i].Trim('"');
                }
                MethodInfo methodToExecute = null;
                object instanceToUse = null;
                bool commandFound = false;
                bool setFound = false;
                foreach (var (method, instance) in commandMethods)
                {
                    AtomicCommandAttribute commandAttribute = method.GetCustomAttribute<AtomicCommandAttribute>();
                    if (commandAttribute != null && commandAttribute.Name.Equals(commandName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(commandAttribute.Group) || commandAttribute.Group.Equals(commandGroup, StringComparison.InvariantCultureIgnoreCase))
                        {
                            methodToExecute = method;
                            instanceToUse = instance;
                            commandFound = true;
                            break;
                        }
                    }
                }
                if (commandFound)
                {
                    ParameterInfo[] parameterInfos = methodToExecute.GetParameters();
                    AtomicCommandAttribute commandAttribute = methodToExecute.GetCustomAttribute<AtomicCommandAttribute>();
                    if (Instance.isLocked && commandAttribute.PasswordProtected)
                    {
                        AtomicDebug.Error($"Cannot execute the password-protected command [{commandName}] while the console is locked.");
                        inputText = "";
                        return;
                    }
                    if (parameters.Length == parameterInfos.Length)
                    {
                        try
                        {
                            object[] convertedParameters = new object[parameters.Length];
                            for (int i = 0; i < parameters.Length; i++)
                            {
                                Type targetType = parameterInfos[i].ParameterType;
                                if (targetType == typeof(float) || targetType == typeof(double) || targetType == typeof(decimal))
                                {
                                    // Handle float, double, and decimal
                                    string param = parameters[i].Replace(",", ".");
                                    try
                                    {
                                        convertedParameters[i] = Convert.ChangeType(param, targetType, CultureInfo.InvariantCulture);
                                    }
                                    catch (Exception ex)
                                    {
                                        AtomicDebug.Exception($"Failed to parse {targetType.Name} from the parameter [{param}]: {ex.Message}");
                                        return;  // Skip executing the command if parsing fails
                                    }
                                }
                                else
                                {
                                    // Handle other types
                                    try
                                    {
                                        convertedParameters[i] = Convert.ChangeType(parameters[i], targetType);
                                    }
                                    catch (Exception ex)
                                    {
                                        AtomicDebug.Exception($"Failed to convert parameter to {targetType.Name}: {ex.Message}");
                                        return;  // Skip executing the command if conversion fails
                                    }
                                }
                            }
                            methodToExecute.Invoke(instanceToUse, convertedParameters);
                        }
                        catch (Exception ex)
                        {
                            AtomicDebug.Exception($"Failed to execute the command [{commandName}]: {ex.Message}");
                        }
                    }
                    else
                    {
                        AtomicDebug.Error($"The command [{commandName}] expects {parameterInfos.Length} parameters, but {parameters.Length} were provided.");
                    }
                }
                else
                {
                    foreach (var (field, instance) in setFields)
                    {
                        AtomicSetAttribute setAttribute = field.GetCustomAttribute<AtomicSetAttribute>();
                        if (Instance.isLocked && setAttribute.PasswordProtected)
                        {
                            AtomicDebug.Error($"Cannot execute the password-protected command [{commandName}] while the console is locked.");
                            inputText = "";
                            return;
                        }
                        if (setAttribute != null && setAttribute.Name.Equals(commandName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            setFound = true;
                            if (parameters.Length == 1)  // Sets should only have one parameter
                            {
                                try
                                {
                                    object convertedValue;
                                    Type fieldType = field.FieldType;
                                    if (fieldType == typeof(float) || fieldType == typeof(double) || fieldType == typeof(decimal))
                                    {
                                        // Handle float, double, and decimal
                                        string param = parameters[0].Replace(",", ".");
                                        convertedValue = Convert.ChangeType(param, fieldType, CultureInfo.InvariantCulture);
                                    }
                                    else
                                    {
                                        // Handle other types
                                        convertedValue = Convert.ChangeType(parameters[0], fieldType);
                                    }
                                    field.SetValue(instance, convertedValue);
                                    AtomicDebug.Command(commandName + " set: " + convertedValue);
                                }
                                catch (Exception ex)
                                {
                                    AtomicDebug.Exception($"Failed to set the field [{commandName}]: {ex.Message}");
                                }
                            }
                            else
                            {
                                AtomicDebug.Error($"The field [{commandName}] expects 1 parameter, but {parameters.Length} were provided.");
                            }
                        }
                    }
                }
                if (!commandFound && !setFound)
                {
                    AtomicDebug.Error($"Method [{trimmedCommand}] not found.");
                }
            }
            if (!string.IsNullOrEmpty(inputText))
            {
                commandHistory.Add(inputText);
                commandHistoryIndex = commandHistory.Count;
            }
            inputText = string.Empty;
            scrollPos.y = float.MaxValue;
            AddLog(inputText);
        }




        private bool setFocusOnNextFrame = false;
        public void ToggleConsole(bool visible)
        {
            isVisible = visible;
            if (visible)
            {
                windowRect.x = (Screen.width - windowRect.width) / 2;
                windowRect.y = (Screen.height - windowRect.height) / 2;
                setFocusOnNextFrame = true;
            }
        }



        private bool _isResizing;
        private void ResizeWindow()
        {
            Vector2 mouse = GUIUtility.ScreenToGUIPoint(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));
            Rect resizeArea = new Rect(windowRect.width - 20, windowRect.height - 20, 20, 20);

            if (Event.current.type == EventType.MouseDown && resizeArea.Contains(mouse))
            {
                Debug.Log("Resizing");
                _isResizing = true;
                _resizingStart = mouse;
                _resizingStartRect = windowRect;
            }

            if (_isResizing)
            {
                if (!Input.GetMouseButton(0))
                {
                    _isResizing = false;
                    return;
                }

                float deltaX = mouse.x - _resizingStart.x;
                float deltaY = mouse.y - _resizingStart.y;

                float newWidth = Mathf.Clamp(_resizingStartRect.width + deltaX, skin.MinWinSize.x, skin.MaxWinSize.x);
                float newHeight = Mathf.Clamp(_resizingStartRect.height + deltaY, skin.MinWinSize.y, skin.MaxWinSize.y);

                windowRect.width = newWidth;
                windowRect.height = newHeight;
            }

            if (Event.current.type == EventType.MouseUp)
            {
                _isResizing = false;
            }
        }


        public static void LoginSystem(string pass)
        {
            if (!Instance.isLocked)
            {
                AtomicDebug.Console("You are already logged-in!");
                return;
            }
            if (Instance.actualPassword == Instance.password)
            {
                AtomicDebug.Console("Log-in complete!");
                Instance.isLocked = false;
            }
            else
            {
                AtomicDebug.Console("Log-in failed!");
            }
            Instance.actualPassword = "";
        }


        public IEnumerator UpdateFPS()
        {
            while (Diagnostic)
            {
                msec = Time.deltaTime * 1000.0f;
                fps = 1.0f / Time.deltaTime;
                memoryUsage = GC.GetTotalMemory(false) / 1024 / 1024; // Memory in MB
                yield return new WaitForSeconds(0.5f);
            }
        }

        bool SkipFrameUpdate;
        public IEnumerator ForceClose()
        {
            SkipFrameUpdate = false;
            yield return new WaitForEndOfFrame();
            ToggleConsole(!isVisible);
        }


    //Default Console Commands




    
     //close the console!
    [AtomicCommand(name: "Close", description: "Close console", isPasswordProtected: false)]
    public static void Close()
    {
        AtomicConsoleEngine.Instance.ToggleConsole(!AtomicConsoleEngine.Instance.isVisible);
    }

    //Simple fps commands
    [AtomicCommand(group: "System",name: "Diagnostic", description: "Toggle diagnostic tool", isPasswordProtected: false)]
    public static void ToggleFPS()
    {
        AtomicConsoleEngine instance = AtomicConsoleEngine.Instance;
        instance.Diagnostic = !instance.Diagnostic;

        if (instance.Diagnostic)
        {
            instance.StartCoroutine(instance.UpdateFPS());
        }
        else
        {
            instance.StopCoroutine(instance.UpdateFPS());
        }
    }

    //Project info command
    [AtomicCommand("System","ProjectInfo", "Prints project information", false)]
    public static void ProjectInfo()
    {
        AtomicDebug.Log("Project Name: " + Application.productName);
        AtomicDebug.Log("Project Version: " + Application.version);
        AtomicDebug.Log("Unity Version: " + Application.unityVersion);
        AtomicDebug.Log("Platform: " + Application.platform);
        AtomicDebug.Log("Data Path: " + Application.dataPath);
    }

    //clear the console!
    [AtomicCommand(name:"Clear", description: "Clear all logs from console", isPasswordProtected: false)]
    public static void Clear()
    {
        AtomicConsoleEngine.Instance.consoleLogs.Clear();
        AtomicConsoleEngine.Instance.concatenatedLogs = "";
        AtomicConsoleEngine.Instance.logsChanged = true;
    }

    //quit the game!
    [AtomicCommand(name: "Quit", description: "Quit the game", isPasswordProtected: false)]
    public static void Quit()
    {
        Application.Quit();
    }

    [AtomicCommand(group: "GL", name: "Wireframe", description: "Toggle wireframe rendering", isPasswordProtected: true)]
    public static void ToggleWireframe()
    {
        Camera cameraToUse = Camera.main;

        if (cameraToUse == null)
        {
            // If the main camera is not found, find any camera in the scene
            cameraToUse = FindFirstObjectByType<Camera>();
        }

        if (cameraToUse != null)
        {
            AtomicShadeMode handler = cameraToUse.gameObject.GetComponent<AtomicShadeMode>();
            if (handler == null)
            {
                handler = cameraToUse.gameObject.AddComponent<AtomicShadeMode>();
            }

            // Toggle wireframe rendering
            AtomicShadeMode.ToggleWireframe();
            AtomicDebug.Console("Wireframe renderer set to: " + AtomicShadeMode.EnableWireframe);
        }
        else
        {
            Debug.LogError("No camera found for wireframe shade mode.");
        }
    }


    [AtomicCommand(group: "GL", name: "Culling", description: "Toggle culling rendering", isPasswordProtected: true)]
    public static void ToggleCulling()
    {
        Camera cameraToUse = Camera.main;

        if (cameraToUse == null)
        {
            // If the main camera is not found, find any camera in the scene
            cameraToUse = FindFirstObjectByType<Camera>();
        }

        if (cameraToUse != null)
        {
            AtomicShadeMode handler = cameraToUse.gameObject.GetComponent<AtomicShadeMode>();
            if (handler == null)
            {
                handler = cameraToUse.gameObject.AddComponent<AtomicShadeMode>();
            }

            // Toggle culling rendering
            AtomicShadeMode.ToggleCulling();
            AtomicDebug.Console("Culling renderer set to: " + AtomicShadeMode.EnableCulling);
        }
        else
        {
            Debug.LogError("No camera found for culling shade mode.");
        }
    }


    //take a screenshot and save in Picture Folder!
    [AtomicCommand(name: "Screenshot", description: "Takes a screenshot and saves it to the user's Pictures folder", isPasswordProtected: false)]
    public static void screenshot()
    {
        ScreenCapture.CaptureScreenshot(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), 
        "Screenshot_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png"));

        AtomicDebug.Command("Screenshot taken.");
    }


    [AtomicCommand(name: "Help", description: "Lists all available commands", isPasswordProtected: false)]
    public static void Help()
    {
        List<string> allCommands = new List<string>();

        foreach ((MethodInfo method, object obj) in AtomicConsoleEngine.Instance.commandMethods)
        {
            AtomicCommandAttribute attribute = method.GetCustomAttribute(typeof(AtomicCommandAttribute)) as AtomicCommandAttribute;

            if (attribute != null)
            {
                // Skip the command if it's password-protected and the console is locked
                if(AtomicConsoleEngine.Instance.isLocked)
                    {
                        if (attribute.PasswordProtected)
                        {
                            continue;
                        }
                    }

                string commandInfo = string.IsNullOrEmpty(attribute.Group) ? 
                                     $"{attribute.Name}: {attribute.Description}" :
                                     $"{attribute.Group}.{attribute.Name}: {attribute.Description}";
                allCommands.Add(commandInfo);
            }
        }

        allCommands.Sort(); // Sort the commands alphabetically

        StringBuilder helpText = new StringBuilder("Available Commands:\n");
        foreach (string command in allCommands)
        {
            helpText.AppendLine($"   {command}");
        }

        AtomicDebug.Console(helpText.ToString());
    }



    [AtomicCommand("Graphics","Quality", "Set the quality level of the game")]
    public static void Quality(int level)
    {
        if (level >= 0 && level <= QualitySettings.names.Length)
        {
            QualitySettings.SetQualityLevel(level);
            AtomicDebug.Command("Quality level is set to: " + level);
        }
        else
        {
            AtomicDebug.Warning("Invalid quality level. Please enter a number between 0 and " + QualitySettings.names.Length);
        }
    }

    [AtomicCommand("Graphics","VSync", "Set the Vertical Sync count")]
    public static void VSync(int count)
    {
        if (count >= 0 && count <= 4)
        {
            QualitySettings.vSyncCount = count;
            AtomicDebug.Command("VSync count is set to: " + count);
        }
        else
        {
            AtomicDebug.Error("Invalid VSync count. Please enter a number between 0 and 4.");
        }
    }

      [AtomicCommand("System","TimeScale", "Set the time scale of the game", true)]
    public static void TimeScale(float scale)
    {
        Time.timeScale = scale;
        AtomicDebug.Command("Time scale is set to: " + scale);
    }

    [AtomicCommand("Graphics","FrameRate", "Set the target frame rate of the game")]
    public static void FrameRate(int rate)
    {
        Application.targetFrameRate = rate;
        AtomicDebug.Command("Target frame rate is set to: " + rate);
    }

    [AtomicCommand("Graphics","FullScreen", "Set the game to full screen or windowed mode")]
    public static void FullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
        AtomicDebug.Command("Full screen mode is set to: " + isFullScreen);
    }

}

}

