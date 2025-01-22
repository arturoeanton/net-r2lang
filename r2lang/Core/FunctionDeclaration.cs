#nullable disable
using R2Lang.Core.Ast;

namespace R2Lang.Core;

public class FunctionDeclaration : INode
{
    public string Name { get; set; }
    public List<string> Args { get; set; } = new();
    public BlockStatement Body { get; set; }

    public object Eval(Environment env)
    {
        var uf = new UserFunction
        {
            Args = Args,
            Body = Body,
            Env = env,
            IsMethod = false,
            code = Name
        };
        env.Set(Name, uf);
        return null;
    }
}