using System.Runtime.CompilerServices;

namespace FluentNetBDD.Dsl.Builders;

public abstract class DriverSequenceBuilderBase<TBuilder, TDriver>
    where TBuilder : DriverSequenceBuilderBase<TBuilder, TDriver>
{
    private readonly TDriver driver;
    private readonly List<Func<Task>> actions = new();
    private Task? task;

    public TBuilder And => (TBuilder)this;

    protected TDriver Driver => driver;

    protected DriverSequenceBuilderBase(TDriver driver)
    {
        this.driver = driver;
    }

    protected TBuilder AddAction(Func<Task> addedTask)
    {
        actions.Add(async () => await Task.Run(addedTask));
        return (TBuilder)this;
    }

    protected Task ToTask() => task ??= actions.Aggregate
    (
        Task.CompletedTask,
        (prev, next) => prev.ContinueWith(_ => next()).Unwrap()
    );

    public ConfiguredTaskAwaitable ConfigureAwait(bool continueOnCapturedContext) => ToTask().ConfigureAwait(continueOnCapturedContext);

    public TaskAwaiter GetAwaiter() => ToTask().GetAwaiter();

    public static implicit operator Task(DriverSequenceBuilderBase<TBuilder, TDriver> builder) => builder.ToTask();

}
