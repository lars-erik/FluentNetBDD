using FluentNetBDD.Dsl.Builders;

namespace FluentNetBDD.Dsl
{
    public class Dsl<TGiven>
        where TGiven : class
    {
        public TGiven Given { get; private set; }
        public object When { get; private set; }
        public object Then { get; private set; }

        public Dsl(IServiceProvider provider)
        {
            Given = SubjunctionBuilder.Create<TGiven>(provider);
            When = new object();
            Then = new object();
        }
    }
}
