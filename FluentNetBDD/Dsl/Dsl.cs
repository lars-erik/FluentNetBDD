using FluentNetBDD.Dsl.Builders;

namespace FluentNetBDD.Dsl;

public class Dsl<TGiven, TWhen, TThen>
    where TGiven : class
    where TWhen : class
    where TThen : class
{
    public TGiven Given { get; private set; }
    public TWhen When { get; private set; }
    public TThen Then { get; private set; }

    public Dsl(IServiceProvider provider)
    {
        Given = SubjunctionBuilder.Create<TGiven>(provider);
        When = SubjunctionBuilder.Create<TWhen>(provider);
        Then = SubjunctionBuilder.Create<TThen>(provider);
    }
}

public class DslState
{
    private readonly Dictionary<string, object?> stateBag = new();

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
}