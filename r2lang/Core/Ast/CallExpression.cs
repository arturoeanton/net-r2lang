namespace R2Lang.Core.Ast;

public class CallExpression : INode
{
    public INode Callee { get; set; }
    public List<INode> Args { get; set; } = new List<INode>();

    public object Eval(Environment env)
    {
        bool flagSuper = false;
        if (Callee is AccessExpression ae
            && ae.Object is Identifier id
            && id.Name == "super")
        {
            flagSuper = true;
        }

        var calleeVal = Callee.Eval(env);
        var argVals = new List<object>();
        foreach (var a in Args)
        {
            argVals.Add(a.Eval(env));
        }

        switch (calleeVal)
        {
            case BuiltinFunction bf:
                return bf(argVals.ToArray());
            case UserFunction uf:
                if (flagSuper)
                {
                    return uf.SuperCall(env, argVals.ToArray());
                }

                return uf.Call(argVals.ToArray());
            case Dictionary<string, object> blueprint:
                // Instanciar
                return ObjectInstance.InstantiateObject(env, blueprint, argVals.ToArray());
            default:
                throw new Exception($"CallExpression: Not a function or blueprint: {calleeVal?.GetType().Name}");
        }
    }
}
