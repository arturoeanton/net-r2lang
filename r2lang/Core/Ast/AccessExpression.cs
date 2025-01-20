#nullable disable
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
            //implement len
            if (Member == "len")
                return (BuiltinFunction)(args =>
                {
                    // 1) Validar que no se pase ningún argumento
                    if (args.Length != 0)
                        throw new Exception("len() no admite argumentos");

                    // 2) Retornar la longitud como double (o int). 
                    return (double)list.Count;
                });
        }

        throw new Exception("AccessExpression: Not supported type => " + objVal?.GetType().Name);
    }
}
