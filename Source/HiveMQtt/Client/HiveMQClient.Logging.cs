namespace HiveMQtt.Client;

using System;
using Microsoft.Extensions.Logging;
using HiveMQtt.Client.Internal;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// Source-generated logging methods for HiveMQClient using LoggerMessage pattern.
/// This provides zero-overhead logging when logging is disabled.
/// </summary>
public partial class HiveMQClient
{
    [LoggerMessage(
        EventId = 8001,
        Level = LogLevel.Trace,
        Message = "New client created: Client ID: {ClientId}")]
    private static partial void LogNewClientCreated(ILogger logger, string? clientId);

    [LoggerMessage(
        EventId = 8002,
        Level = LogLevel.Information,
        Message = "Connecting to broker at {Host}:{Port}")]
    private static partial void LogConnectingToBroker(ILogger logger, string host, int port);

    [LoggerMessage(
        EventId = 8003,
        Level = LogLevel.Trace,
        Message = "Queuing CONNECT packet for send.")]
    private static partial void LogQueuingConnectPacket(ILogger logger);

    [LoggerMessage(
        EventId = 8004,
        Level = LogLevel.Error,
        Message = "Connect timeout.  No response received in {TimeoutMs} milliseconds.")]
    private static partial void LogConnectTimeout(ILogger logger, int timeoutMs);

    [LoggerMessage(
        EventId = 8005,
        Level = LogLevel.Warning,
        Message = "DisconnectAsync called but this client is not connected.  State is {State}.")]
    private static partial void LogDisconnectNotConnected(ILogger logger, ConnectState state);

    [LoggerMessage(
        EventId = 8006,
        Level = LogLevel.Information,
        Message = "Disconnecting from broker at {Host}:{Port}")]
    private static partial void LogDisconnectingFromBroker(ILogger logger, string host, int port);

    [LoggerMessage(
        EventId = 8007,
        Level = LogLevel.Trace,
        Message = "Queuing DISCONNECT packet for send.")]
    private static partial void LogQueuingDisconnectPacket(ILogger logger);

    [LoggerMessage(
        EventId = 8008,
        Level = LogLevel.Debug,
        Message = "Reducing message QoS from {OriginalQoS} to broker enforced maximum of {MaximumQoS}")]
    private static partial void LogReducingMessageQoS(ILogger logger, QualityOfService? originalQoS, int maximumQoS);

    [LoggerMessage(
        EventId = 8009,
        Level = LogLevel.Trace,
        Message = "Queuing QoS 0 publish packet for send: {PacketType}")]
    private static partial void LogQueuingQoS0PublishPacket(ILogger logger, string packetType);

    [LoggerMessage(
        EventId = 8010,
        Level = LogLevel.Trace,
        Message = "Queuing QoS 1 publish packet for send: {PacketType} id={PacketId}")]
    private static partial void LogQueuingQoS1PublishPacket(ILogger logger, string packetType, int packetId);

    [LoggerMessage(
        EventId = 8011,
        Level = LogLevel.Debug,
        Message = "PublishAsync: Operation cancelled by user.")]
    private static partial void LogPublishAsyncOperationCancelled(ILogger logger);

    [LoggerMessage(
        EventId = 8012,
        Level = LogLevel.Trace,
        Message = "Queuing QoS 2 publish packet for send: {PacketType} id={PacketId}")]
    private static partial void LogQueuingQoS2PublishPacket(ILogger logger, string packetType, int packetId);

    [LoggerMessage(
        EventId = 8013,
        Level = LogLevel.Error,
        Message = "Subscribe timeout.  No SUBACK response received in time.")]
    private static partial void LogSubscribeTimeout(ILogger logger);

    // Event launcher logging methods
    [LoggerMessage(
        EventId = 8101,
        Level = LogLevel.Trace,
        Message = "BeforeConnectEventLauncher")]
    private static partial void LogBeforeConnectEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8102,
        Level = LogLevel.Error,
        Message = "BeforeConnect Handler exception")]
    private static partial void LogBeforeConnectHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8103,
        Level = LogLevel.Trace,
        Message = "AfterConnectEventLauncher")]
    private static partial void LogAfterConnectEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8104,
        Level = LogLevel.Error,
        Message = "AfterConnect Handler exception")]
    private static partial void LogAfterConnectHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8105,
        Level = LogLevel.Trace,
        Message = "BeforeDisconnectEventLauncher")]
    private static partial void LogBeforeDisconnectEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8106,
        Level = LogLevel.Error,
        Message = "BeforeDisconnect Handler exception")]
    private static partial void LogBeforeDisconnectHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8107,
        Level = LogLevel.Trace,
        Message = "AfterDisconnectEventLauncher")]
    private static partial void LogAfterDisconnectEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8108,
        Level = LogLevel.Error,
        Message = "AfterDisconnect Handler exception")]
    private static partial void LogAfterDisconnectHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8109,
        Level = LogLevel.Trace,
        Message = "BeforeSubscribeEventLauncher")]
    private static partial void LogBeforeSubscribeEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8110,
        Level = LogLevel.Error,
        Message = "BeforeSubscribe Handler exception")]
    private static partial void LogBeforeSubscribeHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8111,
        Level = LogLevel.Trace,
        Message = "AfterSubscribeEventLauncher")]
    private static partial void LogAfterSubscribeEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8112,
        Level = LogLevel.Error,
        Message = "AfterSubscribe Handler exception")]
    private static partial void LogAfterSubscribeHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8113,
        Level = LogLevel.Trace,
        Message = "BeforeUnsubscribeEventLauncher")]
    private static partial void LogBeforeUnsubscribeEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8114,
        Level = LogLevel.Error,
        Message = "BeforeUnsubscribe Handler exception")]
    private static partial void LogBeforeUnsubscribeHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8115,
        Level = LogLevel.Trace,
        Message = "AfterUnsubscribeEventLauncher")]
    private static partial void LogAfterUnsubscribeEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8116,
        Level = LogLevel.Error,
        Message = "AfterUnsubscribe Handler exception")]
    private static partial void LogAfterUnsubscribeHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8117,
        Level = LogLevel.Trace,
        Message = "OnMessageReceivedEventLauncher")]
    private static partial void LogOnMessageReceivedEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8118,
        Level = LogLevel.Error,
        Message = "OnMessageReceived Handler exception")]
    private static partial void LogOnMessageReceivedHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8119,
        Level = LogLevel.Error,
        Message = "per-subscription MessageReceivedEventLauncher faulted ({Topic})")]
    private static partial void LogPerSubscriptionMessageReceivedFaulted(ILogger logger, Exception ex, string topic);

    [LoggerMessage(
        EventId = 8120,
        Level = LogLevel.Warning,
        Message = "Lost Application Message ({Topic}): No global or subscription message handler found.  Register an event handler (before Subscribing) to receive all messages incoming.")]
    private static partial void LogLostApplicationMessage(ILogger logger, string topic);

    [LoggerMessage(
        EventId = 8121,
        Level = LogLevel.Trace,
        Message = "OnConnectSentEventLauncher")]
    private static partial void LogOnConnectSentEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8122,
        Level = LogLevel.Error,
        Message = "OnConnectSent Handler exception")]
    private static partial void LogOnConnectSentHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8123,
        Level = LogLevel.Trace,
        Message = "OnConnAckReceivedEventLauncher")]
    private static partial void LogOnConnAckReceivedEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8124,
        Level = LogLevel.Error,
        Message = "OnConnAckReceived Handler exception")]
    private static partial void LogOnConnAckReceivedHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8125,
        Level = LogLevel.Trace,
        Message = "OnDisconnectSentEventLauncher")]
    private static partial void LogOnDisconnectSentEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8126,
        Level = LogLevel.Error,
        Message = "OnDisconnectSent Handler exception")]
    private static partial void LogOnDisconnectSentHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8127,
        Level = LogLevel.Trace,
        Message = "OnDisconnectReceivedEventLauncher: ReasonCode: {ReasonCode} ReasonString: {ReasonString}")]
    private static partial void LogOnDisconnectReceivedEventLauncher(ILogger logger, HiveMQtt.MQTT5.ReasonCodes.DisconnectReasonCode reasonCode, string? reasonString);

    [LoggerMessage(
        EventId = 8128,
        Level = LogLevel.Error,
        Message = "OnDisconnectReceived Handler exception")]
    private static partial void LogOnDisconnectReceivedHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8129,
        Level = LogLevel.Trace,
        Message = "OnPingReqSentEventLauncher")]
    private static partial void LogOnPingReqSentEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8130,
        Level = LogLevel.Error,
        Message = "OnPingReqSent Handler exception")]
    private static partial void LogOnPingReqSentHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8131,
        Level = LogLevel.Trace,
        Message = "OnPingRespReceivedEventLauncher")]
    private static partial void LogOnPingRespReceivedEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8132,
        Level = LogLevel.Error,
        Message = "OnPingRespReceived Handler exception")]
    private static partial void LogOnPingRespReceivedHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8133,
        Level = LogLevel.Trace,
        Message = "OnSubscribeSentEventLauncher")]
    private static partial void LogOnSubscribeSentEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8134,
        Level = LogLevel.Error,
        Message = "OnSubscribeSent Handler exception")]
    private static partial void LogOnSubscribeSentHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8135,
        Level = LogLevel.Trace,
        Message = "OnSubAckReceivedEventLauncher")]
    private static partial void LogOnSubAckReceivedEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8136,
        Level = LogLevel.Error,
        Message = "OnSubAckReceived Handler exception")]
    private static partial void LogOnSubAckReceivedHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8137,
        Level = LogLevel.Trace,
        Message = "OnUnsubscribeSentEventLauncher")]
    private static partial void LogOnUnsubscribeSentEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8138,
        Level = LogLevel.Error,
        Message = "OnUnsubscribeSent Handler exception")]
    private static partial void LogOnUnsubscribeSentHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8139,
        Level = LogLevel.Trace,
        Message = "OnUnsubAckReceivedEventLauncher")]
    private static partial void LogOnUnsubAckReceivedEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8140,
        Level = LogLevel.Error,
        Message = "OnUnsubAckReceived Handler exception")]
    private static partial void LogOnUnsubAckReceivedHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8141,
        Level = LogLevel.Trace,
        Message = "OnPublishReceivedEventLauncher")]
    private static partial void LogOnPublishReceivedEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8142,
        Level = LogLevel.Error,
        Message = "OnPublishReceived Handler exception")]
    private static partial void LogOnPublishReceivedHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8143,
        Level = LogLevel.Trace,
        Message = "OnPublishSentEventLauncher")]
    private static partial void LogOnPublishSentEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8144,
        Level = LogLevel.Error,
        Message = "OnPublishSent Handler exception")]
    private static partial void LogOnPublishSentHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8145,
        Level = LogLevel.Trace,
        Message = "OnPubAckReceivedEventLauncher")]
    private static partial void LogOnPubAckReceivedEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8146,
        Level = LogLevel.Error,
        Message = "OnPubAckReceived Handler exception")]
    private static partial void LogOnPubAckReceivedHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8147,
        Level = LogLevel.Trace,
        Message = "OnPubAckSentEventLauncher")]
    private static partial void LogOnPubAckSentEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8148,
        Level = LogLevel.Error,
        Message = "OnPubAckSent Handler exception")]
    private static partial void LogOnPubAckSentHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8149,
        Level = LogLevel.Trace,
        Message = "OnPubRecReceivedEventLauncher")]
    private static partial void LogOnPubRecReceivedEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8150,
        Level = LogLevel.Error,
        Message = "OnPubRecReceived Handler exception")]
    private static partial void LogOnPubRecReceivedHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8151,
        Level = LogLevel.Trace,
        Message = "OnPubRecSentEventLauncher")]
    private static partial void LogOnPubRecSentEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8152,
        Level = LogLevel.Error,
        Message = "OnPubRecSent Handler exception")]
    private static partial void LogOnPubRecSentHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8153,
        Level = LogLevel.Trace,
        Message = "OnPubRelReceivedEventLauncher")]
    private static partial void LogOnPubRelReceivedEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8154,
        Level = LogLevel.Error,
        Message = "OnPubRelReceived Handler exception")]
    private static partial void LogOnPubRelReceivedHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8155,
        Level = LogLevel.Trace,
        Message = "OnPubRelSentEventLauncher")]
    private static partial void LogOnPubRelSentEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8156,
        Level = LogLevel.Error,
        Message = "OnPubRelSent Handler exception")]
    private static partial void LogOnPubRelSentHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8157,
        Level = LogLevel.Trace,
        Message = "PubCompReceivedEventLauncher")]
    private static partial void LogPubCompReceivedEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8158,
        Level = LogLevel.Error,
        Message = "OnPubCompReceived Handler exception")]
    private static partial void LogOnPubCompReceivedHandlerException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 8159,
        Level = LogLevel.Trace,
        Message = "PubCompSentEventLauncher")]
    private static partial void LogPubCompSentEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 8160,
        Level = LogLevel.Error,
        Message = "OnPubCompSent Handler exception")]
    private static partial void LogOnPubCompSentHandlerException(ILogger logger, Exception? ex);

    // HiveMQClientUtil.cs logging methods
    [LoggerMessage(
        EventId = 8201,
        Level = LogLevel.Trace,
        Message = "Disposing HiveMQClient")]
    private static partial void LogDisposingHiveMQClient(ILogger logger);

    [LoggerMessage(
        EventId = 8202,
        Level = LogLevel.Trace,
        Message = "HiveMQClient Dispose: Disconnecting connected client.")]
    private static partial void LogDisposeDisconnectingClient(ILogger logger);

    [LoggerMessage(
        EventId = 8203,
        Level = LogLevel.Warning,
        Message = "Disconnect operation timed out during dispose")]
    private static partial void LogDisconnectTimeoutDuringDispose(ILogger logger);

    [LoggerMessage(
        EventId = 8204,
        Level = LogLevel.Warning,
        Message = "Error disconnecting during dispose")]
    private static partial void LogErrorDisconnectingDuringDispose(ILogger logger, Exception ex);

    // HiveMQClientConnection.cs logging methods
    [LoggerMessage(
        EventId = 8301,
        Level = LogLevel.Debug,
        Message = "AutomaticReconnectHandler: Clean disconnect.  No need to reconnect.")]
    private static partial void LogAutomaticReconnectCleanDisconnect(ILogger logger);

    [LoggerMessage(
        EventId = 8302,
        Level = LogLevel.Warning,
        Message = "AutomaticReconnectHandler: Sender(client) is null.  Cannot reconnect.")]
    private static partial void LogAutomaticReconnectSenderNull(ILogger logger);

    [LoggerMessage(
        EventId = 8303,
        Level = LogLevel.Information,
        Message = "--> Attempting to reconnect to broker.  Attempt #{AttemptNumber}.")]
    private static partial void LogAutomaticReconnectAttempting(ILogger logger, int attemptNumber);

    [LoggerMessage(
        EventId = 8304,
        Level = LogLevel.Information,
        Message = "--> Failed to reconnect to broker: {ReasonCode}/{ReasonString}")]
    private static partial void LogAutomaticReconnectFailed(ILogger logger, ConnAckReasonCode reasonCode, string? reasonString);

    [LoggerMessage(
        EventId = 8305,
        Level = LogLevel.Debug,
        Message = "--> Will delay for {DelaySeconds} seconds until next try.")]
    private static partial void LogAutomaticReconnectDelay(ILogger logger, int delaySeconds);

    [LoggerMessage(
        EventId = 8306,
        Level = LogLevel.Information,
        Message = "--> Reconnected successfully.")]
    private static partial void LogAutomaticReconnectSuccess(ILogger logger);

    [LoggerMessage(
        EventId = 8307,
        Level = LogLevel.Information,
        Message = "--> Failed to reconnect")]
    private static partial void LogAutomaticReconnectException(ILogger logger, Exception ex);
}
