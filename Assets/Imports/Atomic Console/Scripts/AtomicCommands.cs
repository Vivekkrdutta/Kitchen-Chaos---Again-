using System.Collections.Generic;
using System.Reflection;

namespace AtomicAssembly.GeneratedCommands
{
    public static class AtomicCommands
    {
        public static List<MethodInfo> commandMethods = new List<MethodInfo>
        {
            typeof(AtomicConsole.Engine.AtomicConsoleEngine).GetMethod("Close"),
        typeof(AtomicConsole.Engine.AtomicConsoleEngine).GetMethod("ToggleFPS"),
        typeof(AtomicConsole.Engine.AtomicConsoleEngine).GetMethod("ProjectInfo"),
        typeof(AtomicConsole.Engine.AtomicConsoleEngine).GetMethod("Clear"),
        typeof(AtomicConsole.Engine.AtomicConsoleEngine).GetMethod("Quit"),
        typeof(AtomicConsole.Engine.AtomicConsoleEngine).GetMethod("ToggleWireframe"),
        typeof(AtomicConsole.Engine.AtomicConsoleEngine).GetMethod("ToggleCulling"),
        typeof(AtomicConsole.Engine.AtomicConsoleEngine).GetMethod("screenshot"),
        typeof(AtomicConsole.Engine.AtomicConsoleEngine).GetMethod("Help"),
        typeof(AtomicConsole.Engine.AtomicConsoleEngine).GetMethod("Quality"),
        typeof(AtomicConsole.Engine.AtomicConsoleEngine).GetMethod("VSync"),
        typeof(AtomicConsole.Engine.AtomicConsoleEngine).GetMethod("TimeScale"),
        typeof(AtomicConsole.Engine.AtomicConsoleEngine).GetMethod("FrameRate"),
        typeof(AtomicConsole.Engine.AtomicConsoleEngine).GetMethod("FullScreen")
        };

        public static List<FieldInfo> setFields = new List<FieldInfo>
        {
            
        };
    }
}