#nullable disable
using R2Lang.Core.MembersMethod;

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
            var found = MethodsArray.Methods.TryGetValue(Member, out var method);
            if (found) return method(list);
        }


        throw new Exception("AccessExpression: Not supported type => " + objVal?.GetType().Name);
    }
}