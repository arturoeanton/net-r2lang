namespace R2Lang.Core.Ast;

public class AccessExpression : INode
{
    public INode Object { get; set; }
    public string Member { get; set; }

    public object Eval(Environment env)
    {
        var objVal = Object.Eval(env);

        if (objVal is ObjectInstance inst)
        {
            var (v, found) = inst.Env.Get(Member);
            if (!found)
                throw new Exception($"Object does not have property: {Member}");
            return v;
        }

        if (objVal is Dictionary<string, object> dict)
        {
            if (!dict.ContainsKey(Member))
                throw new Exception($"Map does not have key: {Member}");
            return dict[Member];
        }

        if (objVal is List<object> list)
        {
            // Implement len, push, etc. 
            // (No replicamos toda la lógica de Go aquí)
        }

        throw new Exception("AccessExpression: Not supported type => " + objVal?.GetType().Name);
    }
}
