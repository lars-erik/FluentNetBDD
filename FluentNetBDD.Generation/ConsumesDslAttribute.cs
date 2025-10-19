using System;
using System.Collections.Generic;
using System.Text;

namespace FluentNetBDD.Generation
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class ConsumesDslAttribute : Attribute
    {
        public ConsumesDslAttribute(string dslClassName, Type[] givenTypes, Type[] whenTypes, Type[] thenTypes)
        {
            DslClassName = dslClassName;
            GivenTypes = givenTypes;
            WhenTypes = whenTypes;
            ThenTypes = thenTypes;
        }

        public string DslClassName { get; }
        public Type[] GivenTypes { get; }
        public Type[] WhenTypes { get; }
        public Type[] ThenTypes { get; }
    }
}
