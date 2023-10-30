# ConnectReceiveAndPublish

This example illustrates connecting to a broker with `CleanStart=false`.  In this case, upon connecting,
the broker will immediately send down any queued messages.  This can be a large amount in some cases.

For this reason, it's critical that the `OnMessageReceived` handler is configured before connecting to the broker.

This example will connect to the broker with `CleanStart=false`, wait 10 seconds for any queued messages and
then begin to publish Quality of Service Level 2 messages periodically.

This example can be using in conjunction with `SendMessageOnLoop` to test `CleanStart` and handling queued
messages.
