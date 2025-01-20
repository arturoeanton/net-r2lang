#nullable disable
namespace R2Lang.Core.Ast;

public class BinaryExpression : INode
{
    public INode Left { get; set; }
    public string Op { get; set; }
    public INode Right { get; set; }

    public object Eval(Environment env)
    {
        var lv = Left.Eval(env);
        var rv = Right.Eval(env);

        switch (Op)
        {
            case "+": return BuiltinOps.AddValues(lv, rv);
            case "-": return BuiltinOps.SubValues(lv, rv);
            case "*": return BuiltinOps.MulValues(lv, rv);
            case "/": return BuiltinOps.DivValues(lv, rv);
            case "<": return BuiltinOps.ToFloat(lv) < BuiltinOps.ToFloat(rv);
            case ">": return BuiltinOps.ToFloat(lv) > BuiltinOps.ToFloat(rv);
            case "<=": return BuiltinOps.ToFloat(lv) <= BuiltinOps.ToFloat(rv);
            case ">=": return BuiltinOps.ToFloat(lv) >= BuiltinOps.ToFloat(rv);
            case "==": return BuiltinOps.Equals(lv, rv);
            case "!=": return !BuiltinOps.Equals(lv, rv);
            default:
                throw new Exception("Unsupported binary operator: " + Op);
        }
    }
}