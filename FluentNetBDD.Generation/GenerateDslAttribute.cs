using System;

namespace FluentNetBDD.Generation
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class GenerateDslAttribute : Attribute
    {
        public GenerateDslAttribute(string featureName, Type[] givenTypes, Type[] whenTypes, Type[] thenTypes)
        {
            FeatureName = featureName;
            GivenTypes = givenTypes;
            WhenTypes = whenTypes;
            ThenTypes = thenTypes;
        }

        public string FeatureName { get; }
        public Type[] GivenTypes { get; }
        public Type[] WhenTypes { get; }
        public Type[] ThenTypes { get; }
    }
}
