#nullable disable
namespace R2Lang.Core.Ast;

public class ImportStatement : INode
{
    public string _Path { get; set; }
    public string Alias { get; set; }

    public object Eval(Environment env)
    {
        var baseDir = env.Dir ?? "";
        var fullPath = Path.Combine(baseDir, _Path);
        if (!File.Exists(fullPath))
            throw new Exception("Import file not found: " + fullPath);

        var code = File.ReadAllText(fullPath);
        var parser = new Parser(new Lexer(code));
        parser.SetBaseDir(Path.GetDirectoryName(fullPath) ?? "");

        var program = parser.ParseProgram();

        var moduleEnv = new Environment(env);
        program.Eval(moduleEnv);

        // alias => env[alias]= moduleEnv.store
        // sino => exportAll a env
        if (!string.IsNullOrEmpty(Alias))
        {
            env.Set(Alias, moduleEnv.ExportAll());
        }
        else
        {
            foreach (var kv in moduleEnv.ExportAll())
            {
                env.Set(kv.Key, kv.Value);
            }
        }

        return null;
    }
}