using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instant
{
    public abstract class EventBase : Base
    {
        internal abstract void EnsureSent(Base state, bool isAggregating);
    }
}
