namespace R2Lang.Core.Ast;

public class NumberLiteral : INode
{
    public double Value { get; set; }
    public object Eval(Environment env) => Value;
}