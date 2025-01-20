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
        env.Set("_args",args);
        env.Set("_len_args",args.Length);

        env.Dir = Path.GetDirectoryName(Path.GetFullPath(filename)) ?? "";

        // registrar builtins
        Builtins.RegisterAll(env);

        var parser = new Parser(new Lexer(code));
        parser.SetBaseDir(env.Dir);
        var prog = parser.ParseProgram();

        var result = prog.Eval(env);

        // si hay main, llámalo
        var (mainVal, found) = env.Get("main");
        if (found && mainVal is UserFunction mainFn)
        {
            result = mainFn.Call();
        }

        return result;
    }

    public static object RunCodeFromString(string code, string baseDir = "")
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

        return result;
    }
}