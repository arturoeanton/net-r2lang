#nullable disable
namespace R2Lang.Core.Ast;

public class IndexExpression : INode
{
    public INode Left { get; set; }
    public INode Index { get; set; }

    public object Eval(Environment env)
    {
        var leftVal = Left.Eval(env);
        var idxVal = Index.Eval(env);
        if (leftVal is Dictionary<string, object> d)
        {
            var key = idxVal?.ToString() ?? "";
            if (!d.ContainsKey(key)) return null;
            return d[key];
        }

        if (leftVal is List<object> arr)
        {
            var idx = (int)BuiltinOps.ToFloat(idxVal);
            if (idx < 0) idx = arr.Count + idx;
            if (idx < 0 || idx >= arr.Count)
                throw new Exception($"Index out of range: {idx}");
            return arr[idx];
        }

        throw new Exception("IndexExpression => Not a map or array");
    }
}