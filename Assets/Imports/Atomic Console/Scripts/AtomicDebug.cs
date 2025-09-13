using System;

namespace AtomicConsole.debug
{

public static class AtomicDebug
{
    public enum LogType
    {
        Log,
        CommandWarning,
        CommandError,
        CommandException,
        Command,
        Console,
        Network,
        NetworkError,
        NetworkWarning,
        Object,
        Material,
        Info,
        System,
        Audio,
        Critical,
        GameState,
        Physics,
        AI,
        Input
    }

    public static Action<string, LogType> OnLogReceived;

    public static void Log(string message)
    {
        OnLogReceived?.Invoke(message, LogType.Log);
    }

    public static void Warning(string message)
    {
        OnLogReceived?.Invoke(message, LogType.CommandWarning);
    }

    public static void Error(string message)
    {
        OnLogReceived?.Invoke(message, LogType.CommandError);
    }

    public static void Exception(string message)
    {
        OnLogReceived?.Invoke(message, LogType.CommandException);
    }

    public static void Command(string message)
    {
        OnLogReceived?.Invoke(message, LogType.Command);
    }

    public static void Console(string message)
    {
        OnLogReceived?.Invoke(message, LogType.Console);
    }


    public static void Network(string message)
    {
        OnLogReceived?.Invoke(message, LogType.Network);
    }

    public static void NetworkError(string message)
    {
        OnLogReceived?.Invoke(message, LogType.NetworkError);
    }

    public static void NetworkWarnign(string message)
    {
        OnLogReceived?.Invoke(message, LogType.NetworkWarning);
    }

     public static void Object(string message)
    {
        OnLogReceived?.Invoke(message, LogType.Object);
    }

     public static void Material(string message)
    {
        OnLogReceived?.Invoke(message, LogType.Material);
    }

     public static void Info(string message)
    {
        OnLogReceived?.Invoke(message, LogType.Info);
    }

     public static void System(string message)
    {
        OnLogReceived?.Invoke(message, LogType.System);
    }

     public static void Audio(string message)
    {
        OnLogReceived?.Invoke(message, LogType.Audio);
    }

     public static void Critical(string message)
    {
        OnLogReceived?.Invoke(message, LogType.Critical);
    }

     public static void GameState(string message)
    {
        OnLogReceived?.Invoke(message, LogType.GameState);
    }

     public static void Physics(string message)
    {
        OnLogReceived?.Invoke(message, LogType.Physics);
    }

     public static void AI(string message)
    {
        OnLogReceived?.Invoke(message, LogType.AI);
    }

     public static void Input(string message)
    {
        OnLogReceived?.Invoke(message, LogType.Input);
    }
}
}