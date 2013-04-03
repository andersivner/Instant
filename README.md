# Instant Events

This is an implementation of the publish/subscribe pattern that ensures that events are fired in an order so that they appear to be instantaneous, even in situations where subscribers themselves fire other events.

Current version is 0.1. It is currently an early experiment and there will be breaking changes.

Future changes:

* Subscribe using weak references
* Subscriptions with multiple affected targets
* Remove use of static fields
* Add a Bag event aggregator to collect all messages
* Handle placing new subscriptions / cancelling subscriptions during a send consistently.

For example usage, see the testcases in the Instant.Tests test project.

Sounds interesting? Contact me at anders dot ivner at gmail dot com, or @andersivner (twitter).




