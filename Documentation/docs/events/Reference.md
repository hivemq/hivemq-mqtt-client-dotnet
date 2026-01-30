---
sidebar_position: 1
---
# Event Reference

This document provides a comprehensive list of events supported by the HiveMQtt client library. These events are categorized into two sections: General and Packet Level.

## General Events

General events are triggered by high-level operations such as connecting, subscribing, unsubscribing, and receiving messages.


| Event         | EventArgs Class          | Event Arguments      |
| ------------- | ------------------------ | -------------------- |
| BeforeConnect | `BeforeConnectEventArgs` | `HiveMQClientOptions`  |
| AfterConnect  | `AfterConnectEventArgs` | `ConnectResult` |
| BeforeSubscribe | `BeforeSubscribeEventArgs` | `SubscribeOptions`  |
| AfterSubscribe  | `AfterSubscribeEventArgs` |  `SubscribeResult` |
| BeforeUnsubscribe | `BeforeUnsubscribeEventArgs` | `UnsubscribeOptions`  |
| AfterUnsubscribe  | `AfterUnsubscribeEventArgs` |  `UnsubscribeResult` |
| OnMessageReceived | `OnMessageReceivedEventArgs` |  `MQTT5PublishMessage` |
| BeforeDisconnect | `BeforeDisconnectEventArgs` |  None |
| AfterDisconnect | `AfterDisconnectEventArgs` |  `CleanDisconnect` |

## Packet Level Events

Packet level events are triggered by the underlying MQTT packet activity. These events provide a more granular level of control and can be useful for debugging or advanced use cases.


| Event         | EventArgs Class          | Event Arguments      |
| ------------- | ------------------------ | -------------------- |
| OnConnectSent        | `OnConnectSentEventArgs`     |  `ConnectPacket` |
| OnConnAckReceived    | `OnConnAckReceivedEventArgs` |  `ConnAckPacket` |
| OnDisconnectSent     | `OnDisconnectSentEventArgs`  |  `DisconnectPacket` |
| OnDisconnectReceived | `OnDisconnectReceivedEventArgs` |  `DisconnectPacket` |
| OnPingReqSent        | `OnPingReqSentEventArgs`     |  `PingReqPacket` |
| OnPingRespReceived   | `OnPingRespReceivedEventArgs` |  `PingRespPacket` |
| OnPublishSent        | `OnPublishSentEventArgs`     |  `PublishPacket` |
| OnPublishReceived    | `OnPublishReceivedEventArgs` |  `PublishPacket` |
| OnPubAckSent         | `OnPubAckSentEventArgs`      |  `PubAckPacket` |
| OnPubAckReceived     | `OnPubAckReceivedEventArgs` |  `PubAckPacket` |
| OnPubRecSent         | `OnPubRecSentEventArgs`     |  `PubRecPacket` |
| OnPubRecReceived     | `OnPubRecReceivedEventArgs` |  `PubRecPacket` |
| OnPubRelSent         | `OnPubRelSentEventArgs`     |  `PubRelPacket` |
| OnPubRelReceived     | `OnPubRelReceivedEventArgs` |  `PubRelPacket` |
| OnPubCompSent        | `OnPubCompSentEventArgs`    |  `PubCompPacket` |
| OnPubCompReceived    | `OnPubCompReceivedEventArgs` |  `PubCompPacket` |
| OnSubscribeSent      | `OnSubscribeSentEventArgs`    |  `SubscribePacket` |
| OnSubAckReceived     | `OnSubAckReceivedEventArgs`    |  `SubAckPacket` |
| OnUnsubscribeSent    | `OnUnsubscribeSentEventArgs`    |  `UnsubscribePacket` |
| OnUnsubAckReceived   | `OnUnsubAckReceivedEventArgs`    |  `UnsubAckPacket` |

For `OnPubAckReceived` and `OnPubRecReceived`, the event arguments expose a packet with a `ReasonString` property. When the broker sends a human-readable reason (e.g. for schema validation or other diagnostics), `args.PubAckPacket.ReasonString` or `args.PubRecPacket.ReasonString` will be set; otherwise it is `null`.
