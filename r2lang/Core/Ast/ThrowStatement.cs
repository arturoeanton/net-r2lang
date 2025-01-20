#nullable disable
namespace R2Lang.Core.Ast;

public class ThrowStatement : INode
{
    public string Message { get; set; }

    public object Eval(Environment env)
    {
        throw new Exception(Message);
    }
}