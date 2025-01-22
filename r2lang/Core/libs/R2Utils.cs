#nullable disable
namespace R2Lang.Core.Libs;

public class R2Utils
{
    public static void RegisterAll(Environment env)
    {
        env.Set("clear_console", new BuiltinFunction(ClearConsole));
    }


    private static object ClearConsole(params object[] args)
    {
        Console.Clear();
        return null;
    }
}