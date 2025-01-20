#nullable disable
namespace R2Lang.Core.Ast;

public class Identifier : INode
{
    public string Name { get; set; }

    public object Eval(Environment env)
    {
        var (val, ok) = env.Get(Name);
        if (!ok)
            throw new Exception("Undeclared variable: " + Name);
        return val;
    }
}