namespace R2Lang.Core.Ast;

public class MapLiteral : INode
{
    public Dictionary<string, INode> Pairs { get; set; } = new();

    public object Eval(Environment env)
    {
        var dict = new Dictionary<string, object>();
        foreach (var kv in Pairs) dict[kv.Key] = kv.Value.Eval(env);

        return dict;
    }
}