namespace R2Lang.Core.Ast;

public class BlockStatement : INode
{
    public List<INode> Statements { get; set; } = new List<INode>();

    public object Eval(Environment env)
    {
        object result = null;
        foreach (var s in Statements)
        {
            result = s.Eval(env);
            if (result is ReturnValue rv)
            {
                return rv;
            }
        }

        return result;
    }
}
