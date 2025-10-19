using System;

namespace FluentNetBDD.Generation
{
    /// <summary>
    /// Specifies which generated member of a conjunction the interface extends.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class ActorAttribute : Attribute
    {
        public ActorAttribute(string actorName)
        {
            ActorName = actorName;
        }

        public string ActorName { get; }
    }
}
