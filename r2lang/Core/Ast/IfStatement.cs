namespace R2Lang.Core.Ast;

public class IfStatement : INode
{
    public INode Condition { get; set; }
    public BlockStatement Consequence { get; set; }
    public BlockStatement Alternative { get; set; }

    public object Eval(Environment env)
    {
        var cond = Condition.Eval(env);
        if (BuiltinOps.ToBool(cond))
        {
            return Consequence?.Eval(env);
        }
        else
        {
            return Alternative?.Eval(env);
        }
    }
}
