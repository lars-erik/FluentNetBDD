namespace FluentNetBDD.Dsl;

public class DslState
{
    private readonly Dictionary<string, object?> stateBag = new();

    public IReadOnlyDictionary<string, object?> All => stateBag.AsReadOnly();

    public object? Get(string key)
    {
        return stateBag.GetValueOrDefault(key);
    }

    public void Set(string key, object? value, bool overwrite = true)
    {
        if (!stateBag.ContainsKey(key))
        {
            stateBag.Add(key, value!);
        }
        else if (overwrite)
        {
            stateBag[key] = value!;
        }
    }

    public override string ToString()
    {
        return String.Join(Environment.NewLine, All.Select(p => $"{p.Key}: {p.Value}"));
    }
}