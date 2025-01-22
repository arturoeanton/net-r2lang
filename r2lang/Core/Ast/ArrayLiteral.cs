#nullable disable
namespace R2Lang.Core.Ast;

public class ArrayLiteral : INode
{
    public List<INode> Elements { get; set; } = new();

    public object Eval(Environment env)
    {
        var arr = new List<object>();
        foreach (var e in Elements) arr.Add(e.Eval(env));

        return arr;
    }
}