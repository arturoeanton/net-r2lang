#nullable disable
namespace R2Lang.Core.Ast;

public class TestStep
{
    public string Type { get; set; } // e.g. "Given", "When", "Then", "And"
    public INode Command { get; set; }
}