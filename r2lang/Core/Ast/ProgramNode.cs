namespace R2Lang.Core.Ast;

public class ProgramNode : INode
{
    public List<INode> Statements { get; set; } = new List<INode>();

    public object Eval(Environment env)
    {
        object result = null;
        foreach (var st in Statements)
        {
            result = st.Eval(env);
            if (result is ReturnValue rv)
            {
                return rv.Value;
            }
        }

        return result;
    }
}
