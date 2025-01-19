namespace R2Lang.Core.Ast;

public class LetStatement : INode
{
    public string Name { get; set; }
    public INode Value { get; set; }

    public object Eval(Environment env)
    {
        var val = Value?.Eval(env);
        env.Set(Name, val);
        return null;
    }
}
