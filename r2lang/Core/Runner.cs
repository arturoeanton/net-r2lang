namespace R2Lang.Core;

public static class Runner
{
    public static object RunCode(string filename)
    {
        var argsEmpty = new string[0];
        return RunCode(filename, argsEmpty);
    }

    public static object RunCode(string filename, string[] args)
    {
        var code = File.ReadAllText(filename);
        var env = new Environment();
        env.Set("_args", args);
        env.Set("_len_args", args.Length);

        env.Dir = Path.GetDirectoryName(Path.GetFullPath(filename)) ?? "";

        var out1 = RunCodeFromString(code, env.Dir);

        return out1;
    }

    public static object RunCodeFromString(string code, string baseDir = "")
    {
        try
        {
            var env = new Environment();
            env.Dir = baseDir;
            Builtins.RegisterAll(env);

            var parser = new Parser(new Lexer(code));
            parser.SetBaseDir(baseDir);

            var prog = parser.ParseProgram();
            var result = prog.Eval(env);

            // check main
            var (mainVal, found) = env.Get("main");
            if (found && mainVal is UserFunction mainFn)
            {
                result = mainFn.Call();
            }

            Libs.R2Thread.WaitAll();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] " + ex.Message);
            return null;
        }
    }
}