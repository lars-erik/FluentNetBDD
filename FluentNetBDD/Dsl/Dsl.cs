namespace FluentNetBDD.Dsl
{
    public class Dsl
    {
        public object Given { get; private set; }
        public object When { get; private set; }
        public object Then { get; private set; }

        public Dsl()
        {
            Given = new object();
            When = new object();
            Then = new object();
        }
    }
}
