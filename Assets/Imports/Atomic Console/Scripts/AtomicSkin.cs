using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace AtomicConsole.Skin.editor
{

[CreateAssetMenu(fileName = "CustomSkin", menuName = "Atomic Skin Editor")]
public class AtomicSkin : ScriptableObject
{   
    public GUISkin defaultSkin;

    public bool ChangeInRealTime = false;

    public bool ShowArgName = false;

    public KeyCode OpenWindow = KeyCode.F1;
    public KeyCode AutoFill = KeyCode.Tab;
    public KeyCode Previous_cmd = KeyCode.UpArrow;

    public string Header = "Developer Console";
    public string Button_Send = "Send";
    public string Button_Clear = "Clear";
    public bool Show_Clear_Button = true;
    public bool Show_Send_Button = true;
    public bool ShowCloseBtn = false;

    public Font font;
    public Vector2Int MinWinSize = new Vector2Int(512, 254);
    public Vector2Int MaxWinSize = new Vector2Int(1280, 640);
    public bool CanResize = true;
    public WindowType windowType;
    public Rect windowRect;
    public float animationSpeed = 0.5f;
    public LogPrefixType logPrefixType = LogPrefixType.DateTime;
    public string logPrefix = "";

    public int MaxCommandHistory = 50;
    public int MaxSuggestion = 4;

    public float CaretSpeed = -1;

    public Color Output_Command = Color.green;
    public Color Output_Log = Color.white;
    public Color Output_Warning = Color.yellow;
    public Color Output_Error = Color.red;
    public Color Output_Exception = Color.red;
    public Color Output_CommandWarning = new Color(1, 0.5f, 0);
    public Color Output_CommandError = Color.red;
    public Color Output_CommandException = Color.red; 
    public Color Output_Console = Color.green;
    public Color Output_Network = Color.blue; 
    public Color Output_NetworkError = Color.red;
    public Color Output_NetworkWarning = new Color(1, 0.5f, 0); 
    public Color Output_Object = Color.white; 
    public Color Output_Material = Color.magenta;
    public Color Output_Info = Color.white; 
    public Color Output_System = Color.white; 
    public Color Output_Audio = Color.cyan; 
    public Color Output_Critical = Color.red; 
    public Color Output_GameState = Color.yellow; 
    public Color Output_Physics = Color.green; 
    public Color Output_AI = new Color(1, 0, 1); 
    public Color Output_Input = new Color(0.75f, 0.75f, 0.75f);


    public bool InverseSelectionColor = false;
    public Color CaretColor = Color.red;
    public Color SelectionColor = Color.yellow;

    public header_Style Header_Style;
    public log_Style Log_Style;
    public input_Style Input_Style;
    public output_Style Output_Style;
    public button_Style Button_Style;
    public command_Style Command_Style;
    public closeButton_Style CloseButton_Style;
    public scrollbar_Style Scrollbar_Style;

    public LogTypes logTypesToShow = 
    LogTypes.Log |
    LogTypes.CommandWarning |
    LogTypes.CommandError |
    LogTypes.CommandException |
    LogTypes.Command |
    LogTypes.Console |
    LogTypes.Network |
    LogTypes.NetworkError |
    LogTypes.NetworkWarning |
    LogTypes.Object |
    LogTypes.Material |
    LogTypes.Info |
    LogTypes.System |
    LogTypes.Audio |
    LogTypes.Critical |
    LogTypes.GameState |
    LogTypes.Physics |
    LogTypes.AI |
    LogTypes.Input |
    LogTypes.Warning |
    LogTypes.Error |
    LogTypes.Exception;



[Flags]
public enum LogTypes
{
    None = 0,
    Log = 1 << 0,
    Warning = 1 << 1,
    Error = 1 << 2,
    Exception = 1 << 3,
    Command = 1 << 4,
    CommandWarning = 1 << 5,
    CommandError = 1 << 6,
    CommandException = 1 << 7,
    Console = 1 << 8,
    Network = 1 << 9,
    NetworkError = 1 << 10,
    NetworkWarning = 1 << 11,
    Object = 1 << 12,
    Material = 1 << 13,
    Info = 1 << 14,
    System = 1 << 15,
    Audio = 1 << 16,
    Critical = 1 << 17,
    GameState = 1 << 18,
    Physics = 1 << 19,
    AI = 1 << 20,
    Input = 1 << 21
}


    public enum WindowType
    {
        Draggable
    }

    public enum LogPrefixType
    {
        Null,
        DateTime,
        Custom
    }

    public void ResetToDefault()
    {
        OpenWindow = KeyCode.F1;
        AutoFill = KeyCode.Tab;
        Previous_cmd = KeyCode.UpArrow;

        Header = "Developer Console";
        Button_Send = "Send";
        Button_Clear = "Clear";

        CanResize = true;

        Output_Log = Color.white;
        Output_Warning = Color.yellow;
        Output_Error = Color.red;

        Log_Style = new log_Style
        {
            background_color = Color.grey,
            onNormal_background_color = Color.grey,
            background_position = ImagePosition.ImageLeft,
            text_clip = TextClipping.Overflow,
            content_offset = new Vector2(0, 2),
            margin = new Vector4(0, 0, 0, 0),
            padding = new Vector4(0, 0, 0, 0),
            overflow = new Vector4(0, 0, 0, 0),
        };

        Input_Style = new input_Style
        {
            font_style = FontStyle.Normal,
            font_color = Color.white,
            alignment = TextAnchor.MiddleLeft,
            background_color = Color.grey,
            onNormal_background_color = Color.grey,
            background_position = ImagePosition.ImageLeft,
            text_clip = TextClipping.Overflow,
            content_offset = new Vector2(5, 8),
            margin = new Vector4(5, 0, 0, 0),
            padding = new Vector4(0, 0, -10, 15),
            overflow = new Vector4(0, 0, 5, -5),
        };

        Button_Style = new button_Style
        {
            font_style = FontStyle.Normal,
            font_color = Color.white,
            alignment = TextAnchor.MiddleCenter,
            background_color = Color.grey,
            onNormal_background_color = Color.grey,
            background_position = ImagePosition.ImageLeft,
            text_clip = TextClipping.Overflow,
            content_offset = new Vector2(0, -2),
            margin = new Vector4(5, 5, 0, 0),
            padding = new Vector4(0, 0, 0, 0),
            overflow = new Vector4(0, 0, 5, 0),
        };

        Output_Style = new output_Style
        {
            font_style = FontStyle.Normal,
            font_color = Color.white,
            alignment = TextAnchor.UpperLeft,
            background_color = Color.grey,
            onNormal_background_color = Color.grey,
            background_position = ImagePosition.ImageLeft,
            text_clip = TextClipping.Overflow,
            content_offset = new Vector2(5, 0),
            margin = new Vector4(0, 0, 3, 0),
            padding = new Vector4(0, 0, 5, 0),
            overflow = new Vector4(0, 0, 0, 5),
        };

        Command_Style = new command_Style
        {
            font_style = FontStyle.Italic,
            font_color = Color.white,
            alignment = TextAnchor.UpperLeft,
            font_size = 12,
            background_color = Color.grey,
            onNormal_background_color = Color.grey,
            background_position = ImagePosition.ImageLeft,
            text_clip = TextClipping.Overflow,
            content_offset = new Vector2(5, 1),
            margin = new Vector4(0, 0, 0, 0),
            padding = new Vector4(0, 0, 0, 2),
            overflow = new Vector4(0, 0, 0, 0),
        };
    }


}


[System.Serializable]
public class log_Style
{

    public Texture2D background;
    public Texture2D on_background;
    public float transparent = 1;
    public Vector4 border = new Vector4(6, 6, 6, 6);
    public Color background_color = Color.grey;
    public Color onNormal_background_color = Color.grey;
    public ImagePosition background_position = ImagePosition.ImageLeft;
    public TextClipping text_clip = TextClipping.Overflow;
    public Vector2 content_offset = new Vector2(0, 2);

    public Vector4 margin = new Vector4(0, 0, 0, 0);
    public Vector4 padding = new Vector4(0, 0, 0, 0);
    public Vector4 overflow = new Vector4(0,0,0,0);
}

[System.Serializable]
public class header_Style
{
    public FontStyle font_style = FontStyle.Bold;
    public Color font_color = Color.white;
    public TextAnchor alignment = TextAnchor.UpperCenter;
    public int font_size = 0;
    public int HeaderSize = 20;
    public bool useWindowPosition = true;
    public Texture2D background;
    public Texture2D on_background;
    public float transparent = 1;
    public Vector4 border = new Vector4(6, 6, 6, 6);
    public Color background_color = Color.grey;
    public Color onNormal_background_color = Color.grey;
    public ImagePosition background_position = ImagePosition.ImageLeft;
    public TextClipping text_clip = TextClipping.Overflow;
    public Vector2 content_offset = new Vector2(0, 2);

    public Vector4 margin = new Vector4(0, 0, 0, 0);
    public Vector4 padding = new Vector4(0, 0, 0, 0);
    public Vector4 overflow = new Vector4(0,0,0,0);
    public Vector2 WH = new Vector2(0,0);
}


[System.Serializable]
public class scrollbar_Style
{
    public Texture2D Background_Horizontal;
    public Texture2D On_Background_Horizontal;
    public Texture2D Thumb_Horizontal;
    public Texture2D on_Thumb_Horizontal;

    public Texture2D Background_Vertical;
    public Texture2D On_Background_Vertical;
    public Texture2D Thumb_Vertical;
    public Texture2D on_Thumb_Vertical;

    public Color Background_Color = Color.white;
    public Color Thumb_Color = Color.white;
    public Color HoverThumb_Color = Color.black;

    public float transparent = 1;

    public Vector4 Horizontal_Background_Border = new Vector4(9,9,0,0);
    public Vector4 Horizontal_Thumb_Border = new Vector4(6,6,6,6);

    public Vector4 Vertical_Background_Border = new Vector4(9, 9, 0, 0);
    public Vector4 Vertical_Thumb_Border = new Vector4(6, 6, 6, 6);

    public bool Horizontal_Stretch_Width = true;
    public bool Horizontal_Stretch_Height = true;

    public bool Vertical_Stretch_Width = true;
    public bool Vertical_Stretch_Height = false;

    public int Horizontal_Background_Fixed_Height = 15;
    public int Horizontal_Background_Fixed_Width = 15;

    public int Vertical_Background_Fixed_Height = 15;
    public int Vertical_Background_Fixed_Width = 15;

    public int Horizontal_Thumb_Fixed_Height = 15;
    public int Horizontal_Thumb_Fixed_Width = 15;

    public int Vertical_Thumb_Fixed_Height = 15;
    public int Vertical_Thumb_Fixed_Width = 15;

    public TextClipping Horizontal_Text_clip = TextClipping.Clip;
    public ImagePosition Horizontal_Image_position = ImagePosition.ImageOnly;
    public TextAnchor Horizontal_Alignment = TextAnchor.UpperLeft;

    public TextClipping Vertical_Text_clip = TextClipping.Clip;
    public ImagePosition Vertical_Image_position = ImagePosition.ImageOnly;
    public TextAnchor Vertical_Alignment = TextAnchor.UpperLeft;

    public Vector4 Horizontal_Background_margin = new Vector4(0, 0, 0, 0);
    public Vector4 Horizontal_Background_padding = new Vector4(0, 0, 0, 0);
    public Vector4 Horizontal_Background_overflow = new Vector4(0, 0, 0, 0);

    public Vector4 Vertical_Background_margin = new Vector4(1, 4, 4, 4);
    public Vector4 Vertical_Background_padding = new Vector4(0, 0, 1, 1);
    public Vector4 Vertical_Background_overflow = new Vector4(0, 0, 0, 0);

    public Vector4 Horizontal_Thumb_margin = new Vector4(0, 0, 0, 0);
    public Vector4 Horizontal_Thumb_padding = new Vector4(0, 0, 0, 0);
    public Vector4 Horizontal_Thumb_overflow = new Vector4(0, 0, 0, 0);

    public Vector4 Vertical_Thumb_margin = new Vector4(0, 0, 0, 0);
    public Vector4 Vertical_Thumb_padding = new Vector4(0, 0, 0, 0);
    public Vector4 Vertical_Thumb_overflow = new Vector4(0, 0, 0, 0);

    public Vector2 Horizontal_Background_Contect_Offset = new Vector2(0,0);
    public Vector2 Horizontal_Thumb_Contect_Offset = new Vector2(0, 0);

    public Vector2 Vertical_Background_Contect_Offset = new Vector2(0, 0);
    public Vector2 Vertical_Thumb_Contect_Offset = new Vector2(0, 0);
}


[System.Serializable]
public class closeButton_Style
{
    public FontStyle font_style = FontStyle.Bold;
    public Color font_color = Color.white;
    public TextAnchor alignment = TextAnchor.UpperCenter;
    public int font_size = 0;

    public string text = "x";

    public Rect Button_Rect = new Rect(-32, 5, 15, 15);

    public Texture2D background;
    public Texture2D on_background;
    public float transparent = 1;
    public Vector4 border = new Vector4(6, 6, 6, 6);
    public Color background_color = Color.grey;
    public Color onNormal_background_color = Color.grey;
    public ImagePosition background_position = ImagePosition.ImageLeft;
    public TextClipping text_clip = TextClipping.Overflow;
    public Vector2 content_offset = new Vector2(0, 0);

    public Vector4 margin = new Vector4(0, 0, 0, 0);
    public Vector4 padding = new Vector4(0, 0, 0, 0);
    public Vector4 overflow = new Vector4(0, 0, 0, 0);
}

[System.Serializable]
public class input_Style
{
    public FontStyle font_style = FontStyle.Normal;
    public Color font_color = Color.white;
    public TextAnchor alignment = TextAnchor.MiddleLeft;
    public int font_size = 0;

    public Texture2D background;
    public Texture2D on_background;
    public float transparent = 1;
    public Vector4 border = new Vector4(6, 6, 6, 6);
    public Color background_color = Color.grey;
    public Color onNormal_background_color = Color.grey;
    public ImagePosition background_position = ImagePosition.ImageLeft;
    public TextClipping text_clip = TextClipping.Overflow;
    public Vector2 content_offset = new Vector2(5, 8);

    public Vector4 margin = new Vector4(5, 0, 0, 0);
    public Vector4 padding = new Vector4(0, 0, -10, 15);
    public Vector4 overflow = new Vector4(0, 0, 5, -5);

    public Vector2 WH = new Vector2(0,0);
}

[System.Serializable]
public class button_Style
{
    public FontStyle font_style = FontStyle.Normal;
    public Color font_color = Color.white;
    public TextAnchor alignment = TextAnchor.MiddleCenter;
    public int font_size = 0;

    public Texture2D background;
    public Texture2D on_background;
    public float transparent = 1;
    public Vector4 border = new Vector4(6, 6, 6, 6);
    public Color background_color = Color.grey;
    public Color onNormal_background_color = Color.grey;
    public ImagePosition background_position = ImagePosition.ImageLeft;
    public TextClipping text_clip = TextClipping.Overflow;
    public Vector2 content_offset = new Vector2(0, -2);

    public Vector4 margin = new Vector4(5, 5, 0, 0);
    public Vector4 padding = new Vector4(0, 0, 0, 0);
    public Vector4 overflow = new Vector4(0, 0, 5, 0);

    public Vector2 WH = new Vector2(0,0);
}


[System.Serializable]
public class output_Style
{
    public FontStyle font_style = FontStyle.Normal;
    public Color font_color = Color.white;
    public TextAnchor alignment = TextAnchor.UpperLeft;
    public int font_size = 0;

    public Texture2D background;
    public Texture2D on_background;
    public float transparent = 1;
    public Vector4 border = new Vector4(6, 6, 6, 6);
    public Color background_color = Color.grey;
    public Color onNormal_background_color = Color.grey;
    public ImagePosition background_position = ImagePosition.ImageLeft;
    public TextClipping text_clip = TextClipping.Overflow;
    public Vector2 content_offset = new Vector2(5, 0);

    public Vector4 margin = new Vector4(0, 0, 3, 0);
    public Vector4 padding = new Vector4(0, 0, 5, 0);
    public Vector4 overflow = new Vector4(0, 0, 0, 5);
}


[System.Serializable]
public class command_Style
{
    public FontStyle font_style = FontStyle.Italic;
    public Color font_color = Color.white;
    public TextAnchor alignment = TextAnchor.MiddleLeft;
    public int font_size = 12;

    public Texture2D background;
    public Texture2D on_background;
    public float transparent = 1;
    public Vector4 border = new Vector4(6, 6, 6, 6);
    public Color background_color = Color.grey;
    public Color onNormal_background_color = Color.grey;
    public ImagePosition background_position = ImagePosition.ImageLeft;
    public TextClipping text_clip = TextClipping.Overflow;
    public Vector2 content_offset = new Vector2(5, 1);

    public Vector4 margin = new Vector4(0, 0, 0, 0);
    public Vector4 padding = new Vector4(0, 0, 0, 2);
    public Vector4 overflow = new Vector4(0, 0, 0, 0);

    public Vector2 WH = new Vector2(0,0);
}



[System.Serializable]
public class highLightCommand_Style
{
    public Texture2D background;
    public Texture2D on_background;
    public float transparent = 1;
    public Vector4 border = new Vector4(6, 6, 6, 6);
    public Color background_color = Color.grey;
    public Color onNormal_background_color = Color.grey;
    public ImagePosition background_position = ImagePosition.ImageLeft;
    public TextClipping text_clip = TextClipping.Overflow;
    public Vector2 content_offset = new Vector2(5, 1);

    public Vector4 margin = new Vector4(0, 0, 0, 0);
    public Vector4 padding = new Vector4(0, 0, 0, 2);
    public Vector4 overflow = new Vector4(0, 0, 0, 0);

    public Vector2 WH = new Vector2(0,0);
}
}