#nullable disable
namespace R2Lang.Core;

public class ObjectInstance
{
    public Environment Env { get; set; }

    // instantiateObject
    public static ObjectInstance InstantiateObject(Environment env, Dictionary<string, object> blueprint,
        params object[] argVals)
    {
        var objEnv = new Environment(env);
        var instance = new ObjectInstance { Env = objEnv };

        // Copiamos props
        foreach (var kv in blueprint)
        {
            if (kv.Value is UserFunction uf)
            {
                var newUF = new UserFunction
                {
                    Args = uf.Args,
                    Body = uf.Body,
                    Env = objEnv,
                    IsMethod = true,
                    code = uf.code
                };
                objEnv.Set(kv.Key, newUF);
            }
            else
            {
                objEnv.Set(kv.Key, kv.Value);
            }
        }

        objEnv.Set("self", instance);
        objEnv.Set("this", instance);

        // llama constructor si existe
        var (ctorVal, found) = objEnv.Get("constructor");
        if (found && ctorVal is UserFunction ctor)
        {
            ctor.Call(argVals);
        }

        return instance;
    }
}
