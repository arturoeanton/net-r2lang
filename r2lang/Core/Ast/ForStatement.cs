namespace R2Lang.Core.Ast;

public class ForStatement : INode
{
    public INode Init { get; set; }
    public INode Condition { get; set; }
    public INode Post { get; set; }
    public BlockStatement Body { get; set; }

    // flag in
    public bool inFlag = false;
    public string inArray;
    public string inIndexName;

    public object Eval(Environment env)
    {
        // si inFlag => for(... in array)
        // sino => for(init; cond; post)
        var inner = new Environment(env);
        Init?.Eval(inner);

        object result = null;
        while (true)
        {
            if (Condition != null)
            {
                var c = BuiltinOps.ToBool(Condition.Eval(inner));
                if (!c) break;
            }

            var val = Body.Eval(inner);
            if (val is ReturnValue rv)
                return rv;
            result = val;
            Post?.Eval(inner);
        }

        return result;
    }
}