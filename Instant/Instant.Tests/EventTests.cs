using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Instant.Tests
{
    [TestClass]
    public class EventTests
    {
        [TestMethod]
        public void When_ReadingState_Then_EventsModifyingThatStateHaveBeenCalled()
        {
            var e = new Instant.Event<NoMessage>();
            var state = new Instant.State<int>(1);

            var wasCalled = false;

            e.Subscribe(x =>
                {
                    wasCalled = true;
                    Assert.AreEqual(2, state.Value);
                });

            e.Subscribe(x =>
                {
                    state.Value = 2;
                }, state);

            e.Send();

            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        public void When_ReadingState_Then_ChainedEventsModifyingThatStateHaveBeenCalled()
        {
            var e = new Instant.Event<NoMessage>();
            var e2 = new Instant.Event<NoMessage>();
            var state = new Instant.State<int>(1);

            var wasCalled = false;

            e.Subscribe(x =>
            {
                wasCalled = true;
                Assert.AreEqual(2, state.Value);
            });

            e.Subscribe(x =>
                {
                    e2.Send();
                }, e2);

            e2.Subscribe(x =>
            {
                state.Value = 2;
            }, state);

            e.Send();

            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        public void Given_SubscriptionWithoutAffected_When_ModifyingState_ThenException()
        {
            var e = new Instant.Event<NoMessage>();
            var state = new Instant.State<int>(1);

            var wasCalled = false;

            e.Subscribe(x =>
            {
                try
                {
                    state.Value = 2;
                    Assert.Fail("exception should be thrown");
                }
                catch(Exception ex)
                {
                    Assert.AreEqual("Illegal state modification", ex.Message);
                }
                wasCalled = true;
            });

            e.Send();

            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        public void Given_SubscriptionWithoutAffected_When_Sending_ThenException()
        {
            var e = new Instant.Event<NoMessage>();
            var e2 = new Instant.Event<NoMessage>();

            var wasCalled = false;

            e.Subscribe(x =>
            {
                try
                {
                    e2.Send();
                    Assert.Fail("exception should be thrown");
                }
                catch (Exception ex)
                {
                    Assert.AreEqual("Illegal state modification", ex.Message);
                }
                wasCalled = true;
            });

            e.Send();

            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        public void When_SendingSameEventTwice_Then_SubscriberOnlyCalledOnce()
        {
            var e = new Instant.Event<NoMessage>();
            var e2 = new Instant.Event<NoMessage>();

            int nrOfCalls = 0;

            e.Subscribe(x =>
            {
                e2.Send();
            }, e2);

            e.Subscribe(x =>
            {
                e2.Send();
            }, e2);

            e2.Subscribe(x =>
                {
                    nrOfCalls++;
                });

            e.Send();

            Assert.AreEqual(1, nrOfCalls);
        }

        [TestMethod]
        public void EventThatModifiesStateCanReadTheOldValueOfTheState()
        {
            var e = new Instant.Event<NoMessage>();
            var state = new Instant.State<int>(1);

            var wasCalled = false;

            e.Subscribe(x =>
            {
                wasCalled = true;
                Assert.AreEqual(1, state.Value);
                state.Value = 2;
            }, state);

            e.Send();

            Assert.IsTrue(wasCalled);
        }


        [TestMethod]
        public void CircularDependencyThrowsException()
        {
            var e = new Instant.Event<NoMessage>();
            var state = new Instant.State<int>(1);
            var state2 = new Instant.State<int>(2);

            var wasCalled = false;

            e.Subscribe(x =>
            {
                state.Value = state2.Value;
            }, state);

            e.Subscribe(x =>
            {
                state2.Value = state.Value;
            }, state2);

            try
            {
                e.Send();
            }
            catch (Exception ex)
            {
                wasCalled = true;
            }

            Assert.IsTrue(wasCalled);
        }
    
    }
}
