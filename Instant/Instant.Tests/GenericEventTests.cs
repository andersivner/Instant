using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Instant.Tests
{
    [TestClass]
    public class GenericEventTests
    {
        [TestMethod]
        public void When_ReadingState_Then_EventsModifyingThatStateHaveBeenCalled()
        {
            var e = new Instant.Event<int>();
            var state = new Instant.State<int>(1);

            var wasCalled = false;

            e.Subscribe((x) =>
            {
                wasCalled = true;
                Assert.AreEqual(2, state.Value);
            });

            e.Subscribe((x) =>
            {
                state.Value = 2;
            }, state);

            e.Send(1);

            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        public void When_ReadingState_Then_ChainedEventsModifyingThatStateHaveBeenCalled()
        {
            var e = new Instant.Event<int>();
            var e2 = new Instant.Event<int>();
            var state = new Instant.State<int>(1);

            var wasCalled = false;

            e.Subscribe((x) =>
            {
                wasCalled = true;
                Assert.AreEqual(2, state.Value);
            });

            e.Subscribe((x) =>
            {
                e2.Send(1);
            }, e2);

            e2.Subscribe((x) =>
            {
                state.Value = 2;
            }, state);

            e.Send(1);

            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        public void Given_SubscriptionWithoutAffected_When_ModifyingState_ThenException()
        {
            var e = new Instant.Event<int>();
            var state = new Instant.State<int>(1);

            var wasCalled = false;

            e.Subscribe((x) =>
            {
                try
                {
                    state.Value = 2;
                    Assert.Fail("exception should be thrown");
                }
                catch (Exception ex)
                {
                    Assert.AreEqual("Illegal state modification", ex.Message);
                }
                wasCalled = true;
            });

            e.Send(1);

            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        public void Given_SubscriptionWithoutAffected_When_Sending_ThenException()
        {
            var e = new Instant.Event<int>();
            var e2 = new Instant.Event<int>();

            var wasCalled = false;

            e.Subscribe((x) =>
            {
                try
                {
                    e2.Send(1);
                    Assert.Fail("exception should be thrown");
                }
                catch (Exception ex)
                {
                    Assert.AreEqual("Illegal state modification", ex.Message);
                }
                wasCalled = true;
            });

            e.Send(1);

            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        public void When_SendingSameEventTwice_Then_Exception()
        {
            var e = new Instant.Event<int>();
            var e2 = new Instant.Event<int>();

            var nrOfCalls = 0;
            var exception = false;

            e.Subscribe((x) =>
            {
                e2.Send(1);
            }, e2);

            e.Subscribe((x) =>
            {
                e2.Send(1);
            }, e2);

            e2.Subscribe((x) =>
            {
                nrOfCalls++;
            });

            try
            {
                e.Send(1);
            }
            catch
            {
                exception = true;
            }

            Assert.AreEqual(1, nrOfCalls);
            Assert.IsTrue(exception);
        }

        [TestMethod]
        public void EventWithParameter()
        {
            var e = new Instant.Event<int>();

            var wasCalled = false;

            e.Subscribe(x =>
            {
                Assert.AreEqual(2, x);
                wasCalled = true;
            });

            e.Send(2);

            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        public void AggregatedEvent()
        {
            var e = new Instant.Event<int>((x, y) => x + y);
            var e2 = new Instant.Event<NoMessage>();

            e2.Subscribe(x =>
            {
                e.Send(1);
            }, e);

            e2.Subscribe(x =>
            {
                e.Send(1);
            }, e);

            var wasCalled = false;

            e.Subscribe(x =>
            {
                Assert.AreEqual(2, x);
                wasCalled = true;
            });

            e2.Send();

            Assert.IsTrue(wasCalled);
        }

    }
}
