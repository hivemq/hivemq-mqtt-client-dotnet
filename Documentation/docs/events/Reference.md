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

## Packet Level Events

Packet level events are triggered by the underlying MQTT packet activity. These events provide a more granular level of control and can be useful for debugging or advanced use cases.


| Event         | EventArgs Class          | Event Arguments      |
| ------------- | ------------------------ | -------------------- |
| OnConnectSent        | `OnConnectSentEventArgs`     |  `ConnectPacket` |
| OnConnAckReceived    | `OnConnAckReceivedEventArgs` |  `ConnAckPacket` |
| OnConnectSent        | `OnConnectSentEventArgs`     |  `ConnectPacket` |
| OnDisconnectReceived | `OnDisconnectReceivedEventArgs` |  `DisconnectPacket` |
| OnDisconnectSent     | `OnDisconnectSentEventArgs`  |  `DisconnectPacket` |
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
| OnSubAckSent         | `OnSubAckSentEventArgs`    |  `SubAckPacket` |
| OnUnsubscribeSent    | `OnUnsubscribeSentEventArgs`    |  `UnsubscribePacket` |
| OnUnsubAckSent       | `OnUnsubAckSentEventArgs`    |  `UnsubAckPacket` |
