namespace R2Lang.Core;

public static class Builtins
{
    public static void RegisterAll(Environment env)
    {
        // en Go tenías RegisterLib, RegisterStd, etc.
        // Aquí puedes definir lo que gustes. 
        // Ejemplo:
        env.Set("print", (BuiltinFunction)Print);
        // ...
    }

    private static object Print(params object[] args)
    {
        foreach (var a in args)
        {
            Console.Write(a);
            Console.Write(" ");
        }

        Console.WriteLine();
        return null;
    }

    // si quieres mas builtins => expandir
}
