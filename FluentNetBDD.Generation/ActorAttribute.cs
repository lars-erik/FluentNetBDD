using System;
using System.Collections.Generic;
using System.Text;

namespace FluentNetBDD.Generation
{
    public class ActorAttribute : Attribute
    {
        public ActorAttribute(string actorName)
        {
            ActorName = actorName;
        }

        public string ActorName { get; }
    }
}
