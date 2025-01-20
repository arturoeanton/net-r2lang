#nullable disable
namespace R2Lang.Core.Ast;

public class ReturnStatement : INode
{
    public INode Value { get; set; }

    public object Eval(Environment env)
    {
        object val = Value?.Eval(env);
        return new ReturnValue { Value = val };
    }
}
