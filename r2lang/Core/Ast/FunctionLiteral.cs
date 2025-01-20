#nullable disable
namespace R2Lang.Core.Ast;

public class FunctionLiteral : INode
{
    public List<string> Args { get; set; } = new List<string>();
    public BlockStatement Body { get; set; }

    public object Eval(Environment env)
    {
        var uf = new UserFunction
        {
            Args = Args,
            Body = Body,
            Env = env,
            IsMethod = false
        };
        return uf;
    }
}
