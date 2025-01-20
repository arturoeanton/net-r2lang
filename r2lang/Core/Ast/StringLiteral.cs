#nullable disable
namespace R2Lang.Core.Ast;


public class StringLiteral : INode
{
    public string Value { get; set; }
    public object Eval(Environment env) => Value;
}