#nullable disable
namespace R2Lang.Core.Ast;

public class WhileStatement : INode
{
    public INode Condition { get; set; }
    public BlockStatement Body { get; set; }

    public object Eval(Environment env)
    {
        object result = null;
        while (BuiltinOps.ToBool(Condition.Eval(env)))
        {
            var val = Body.Eval(env);
            if (val is ReturnValue rv)
                return rv;
            result = val;
        }

        return result;
    }
}