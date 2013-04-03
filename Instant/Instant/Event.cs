using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instant
{
    public class NoMessage
    {
        private NoMessage()
        {
        }
    }

    public class Event<T> : EventBase
    {
        public enum SubscriptionStatus { Done, ToSend, Sending }

        private class SubscriberInfo
        {
            public Base target;
            public SubscriptionStatus sendStatus;
            public Action<T> action;

            public void Send(T message)
            {
                sendStatus = SubscriptionStatus.Sending;
                Targets.Push(target);
                if (target != null)
                    target.Open();
                try
                {
                    action(message);
                }
                finally
                {
                    if (target != null)
                        target.Close();
                    Targets.Pop();
                    sendStatus = SubscriptionStatus.Done;
                }
            }
        }

        private static bool IsSending;

        private bool areTargetsValid = true;
        private bool isSent;
        private List<SubscriberInfo> subscribers = new List<SubscriberInfo>();
        private T message;
        private Func<T, T, T> aggregator;
        private bool isAggregating;
        private T aggregatedMessage;

        public Event()
        {
        }

        public Event(Func<T, T, T> aggregator)
        {
            this.aggregator = aggregator;
        }

        public void Send()
        {
            Send(default(T));
        }

        public void Send(T message)
        {
            if (IsSending && !IsOpen)
                throw new Exception("Illegal state modification");

            this.message = message;

            if (IsSending)
                InternalSend();
            else
                ExternalSend();
        }

        public void Subscribe(Action<T> action)
        {
            subscribers.Add(new SubscriberInfo() { action = action, sendStatus = SubscriptionStatus.Done });
        }

        public void Subscribe(Action<T> action, Base affectedState)
        {
            subscribers.Add(new SubscriberInfo() { action = action, sendStatus = SubscriptionStatus.Done, target = affectedState });
            affectedState.AddSource(this);
        }

        private void ExternalSend()
        {
            IsSending = true;
            BeginSending();
            try
            {
                InternalSend();
            }
            finally
            {
                DoneSending();
                IsSending = false;
            }
        }

        protected void InternalSend()
        {
            if (aggregator != null)
            {
                if (isAggregating)
                {
                    aggregatedMessage = aggregator(aggregatedMessage, message);
                    return;
                }
                isAggregating = true;
                aggregatedMessage = message;

                foreach (var source in sources)
                    source.EnsureSent(this, true);

                message = aggregatedMessage;
            }
            else if (isSent)
            {
                if (typeof(T) == typeof(NoMessage))
                    return;
                throw new Exception("cannot send the same event with parameter twice");
            }

            foreach (SubscriberInfo subscriber in subscribers)
                subscriber.sendStatus = SubscriptionStatus.ToSend;
            foreach (SubscriberInfo subscriber in subscribers)
            {
                if (subscriber.sendStatus == SubscriptionStatus.ToSend)
                    subscriber.Send(message);
            }

            isSent = true;

            this.message = default(T);
        }


        internal override void EnsureSent(Base state, bool isAggregating)
        {
            if (IsValid || isSent)
                return;

            foreach (var subscriber in subscribers.Where(s => s.target == state))
            {
                if (subscriber.sendStatus == SubscriptionStatus.Sending)
                {
                    if (!isAggregating)
                        throw new Exception("Circular dependency!");
                }
                else if (subscriber.sendStatus == SubscriptionStatus.ToSend)
                {
                    subscriber.Send(message);
                }
                else // SendStatus.Done
                {
                    EnsureValid();
                }
            }
        }

        internal override void BeginSending()
        {
            base.BeginSending();
            if (areTargetsValid)
            {
                areTargetsValid = false;
                foreach (var subscriber in subscribers)
                {
                    if (subscriber.target != null)
                        subscriber.target.BeginSending();
                }
            }
        }

        internal override void DoneSending()
        {
            base.DoneSending();
            if (!areTargetsValid)
            {
                areTargetsValid = true;
                isSent = false;
                foreach (var subscriber in subscribers)
                {
                    if (subscriber.target != null)
                        subscriber.target.DoneSending();
                }
            }
        }
    }
}
