#nullable disable
namespace R2Lang.Core.Ast;

public class TryStatement : INode
{
    public BlockStatement Body { get; set; }
    public BlockStatement CatchBlock { get; set; }
    public BlockStatement FinallyBlock { get; set; }
    public string ExceptionVar { get; set; } = "$e";

    public object Eval(Environment env)
    {
        object result = null;
        try
        {
            result = Body?.Eval(env);
        }
        catch (Exception ex)
        {
            if (CatchBlock != null)
            {
                var catchEnv = new Environment(env);
                catchEnv.Set(ExceptionVar, ex.Message);
                result = CatchBlock.Eval(catchEnv);
            }
        }
        finally
        {
            if (FinallyBlock != null) result = FinallyBlock.Eval(env);
        }

        return result;
    }
}