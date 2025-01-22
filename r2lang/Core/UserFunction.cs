#nullable disable
using R2Lang.Core.Ast;

namespace R2Lang.Core;

public class UserFunction
{
    public List<string> Args { get; set; } = new();
    public BlockStatement Body { get; set; }
    public Environment Env { get; set; }
    public bool IsMethod { get; set; } = false;
    public string code { get; set; } = "";

    public object NativeCall(Environment currentEnv, params object[] args)
    {
        var newEnv = currentEnv == null ? new Environment(Env) : currentEnv;
        if (IsMethod)
        {
            // manejar self, this, super, etc.
            if (Env != null)
            {
                var (sv, found) = Env.Get("self");
                if (found)
                {
                    newEnv.Set("self", sv);
                    newEnv.Set("this", sv);
                }
            }
            else
            {
                var (sv2, found2) = newEnv.Get("self");
                if (found2)
                {
                    newEnv.Set("self", sv2);
                    newEnv.Set("this", sv2);
                    if (newEnv.Get("super") is (var supVal, true) && supVal is Dictionary<string, object> supMap)
                        if (supMap.ContainsKey("super"))
                            newEnv.Set("super", supMap["super"]);
                }
            }
        }

        for (var i = 0; i < Args.Count; i++) newEnv.Set(Args[i], i < args.Length ? args[i] : null);

        var val = Body.Eval(newEnv);
        if (val is ReturnValue rv) return rv.Value;

        return val;
    }

    public object Call(params object[] arguments)
    {
        var tmp = Env.CurrenFx;
        Env.CurrenFx = code;
        var outVal = NativeCall(null, arguments);
        Env.CurrenFx = tmp;
        return outVal;
    }


    public object SuperCall(Environment env, params object[] args)
    {
        var tmp = env.CurrenFx;
        env.CurrenFx = code;
        var outVal = NativeCall(env, args);
        env.CurrenFx = tmp;
        return outVal;
    }

    public object CallStep(Environment env, params object[] args)
    {
        var tmp = env.CurrenFx;
        env.CurrenFx = code;
        var outVal = NativeCall(env, args);
        env.CurrenFx = tmp;
        return outVal;
    }
}