using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instant
{
    public abstract class Base
    {
        protected static readonly Stack<Base> Targets = new Stack<Base>();

        protected readonly List<EventBase> sources = new List<EventBase>();
        private bool isValid = true;
        private bool isOpen;

        internal void AddSource(EventBase e)
        {
            sources.Add(e);
        }

        protected void EnsureValid()
        {
            if (isValid)
                return;

            sources.ForEach(e => e.EnsureSent(this, false));
            isValid = true;
        }

        internal virtual void BeginSending()
        {
            isValid = false;
        }

        internal virtual void DoneSending()
        {
            isValid = true;
        }

        internal bool IsValid { get { return isValid; } }

        internal bool IsOpen { get { return isOpen; } }

        internal void Open()
        {
            isOpen = true;
        }

        internal void Close()
        {
            isOpen = false;
        }
    }
}
