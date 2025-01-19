namespace R2Lang.Core.Ast;

// =========================================================
// 3) INODE
// =========================================================
public interface INode
{
    object Eval(Environment env);
}