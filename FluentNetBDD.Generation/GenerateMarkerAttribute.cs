using System;

namespace FluentNetBDD.Generation
{

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class GenerateMarkerAttribute : Attribute
    {
        public GenerateMarkerAttribute(string className, string value = null)
        {
            ClassName = className;
            Value = value;
        }

        public string ClassName { get; }
        public string Value { get; }
    }
}
