#nullable disable
using R2Lang.Core.Ast;

namespace R2Lang.Core;

public class ObjectDeclaration : INode
{
    public string Name { get; set; }
    public string ParentName { get; set; }
    public List<INode> Members { get; set; }

    public object Eval(Environment env)
    {
        var blueprint = new Dictionary<string, object>();
        // si hay padre
        if (!string.IsNullOrEmpty(ParentName))
        {
            var (pval, found) = env.Get(ParentName);
            if (found && pval is Dictionary<string, object> pdict)
            {
                blueprint["super"] = pval;
                // Copiar props excepto ClassName, super, etc.
                foreach (var kv in pdict)
                {
                    if (kv.Key == "ClassName" || kv.Key == "SuperClassName" || kv.Key == "super") continue;
                    blueprint[kv.Key] = kv.Value;
                }

                blueprint["SuperClassName"] = pdict.TryGetValue("ClassName", out var scn) ? scn : null;
            }
        }

        blueprint["ClassName"] = Name;

        foreach (var m in Members)
            if (m is LetStatement ls)
            {
                blueprint[ls.Name] = null;
            }
            else if (m is FunctionDeclaration fd)
            {
                var uf = new UserFunction
                {
                    Args = fd.Args,
                    Body = fd.Body,
                    Env = null,
                    IsMethod = true,
                    code = fd.Name
                };
                blueprint[fd.Name] = uf;
            }

        env.Set(Name, blueprint);
        return null;
    }
}