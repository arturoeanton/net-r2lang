#nullable disable
namespace R2Lang.Core.Ast;

public class ExpressionStatement : INode
{
    public INode Expr { get; set; }

    public object Eval(Environment env)
    {
        return Expr?.Eval(env);
    }
}