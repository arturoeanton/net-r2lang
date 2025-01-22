namespace R2Lang.Core.Ast;

public class BooleanLiteral : INode
{
    public bool Value { get; set; }

    public object Eval(Environment env)
    {
        return Value;
    }
}