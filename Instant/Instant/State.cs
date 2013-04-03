using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Instant
{

    public class State<T> : Base
    {
        private T value;

        public State(T initialValue)
        {
            value = initialValue;
        }

        public T Value 
        {
            get
            {
                if (Targets.Peek() != this)
                    EnsureValid();
                return value;
            }
            set
            {
                if (Targets.Peek() != this)
                    throw new Exception("Illegal state modification");
                this.value = value;
            }
        }
    }
}
