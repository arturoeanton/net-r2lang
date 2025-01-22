using R2Lang.Core;
using R2Lang.Core.Ast;

namespace Program;

internal class Program
{
    private static void Main(string[] args)
    {
        var filename = "main.r2";
        // Si no se pasaron argumentos:
        if (args.Length >= 1)
            // El primer argumento se asume el archivo
            filename = args[0];

        try
        {
            var rest = args.Skip(1).ToArray();
            var result = Runner.RunCode(filename, rest);
            if (result is ReturnValue rv) Console.WriteLine("Return  => " + rv.Value);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] " + ex.Message);
        }
    }
}