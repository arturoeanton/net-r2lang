#nullable disable
namespace R2Lang.Core;

public class Environment
{
    private readonly Environment outer;
    private readonly Dictionary<string, object> store;

    public Environment(Environment parent = null)
    {
        store = new Dictionary<string, object>();
        outer = parent;
        if (parent != null) Dir = parent.Dir;
    }

    public string Dir { get; set; } = "";
    public string CurrenFx { get; set; } = "";

    public Environment GetOuter()
    {
        return outer;
    }

    public void Set(string name, object value)
    {
        store[name] = value;
    }

    public (object, bool) Get(string name)
    {
        if (store.ContainsKey(name))
            return (store[name], true);
        if (outer != null)
            return outer.Get(name);
        return (null, false);
    }

    public Dictionary<string, object> ExportAll()
    {
        return new Dictionary<string, object>(store);
    }
}