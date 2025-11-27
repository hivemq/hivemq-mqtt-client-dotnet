namespace HiveMQtt.Client.Connection;

using System;
using Microsoft.Extensions.Logging;
using HiveMQtt.Client.Internal;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// Source-generated logging methods for ConnectionManager using LoggerMessage pattern.
/// This provides zero-overhead logging when logging is disabled.
/// </summary>
public partial class ConnectionManager
{
    // ConnectionMonitor logging
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(CM)- Starting...{State}")]
    private static partial void LogConnectionMonitorStarting(ILogger logger, string clientId, ConnectState state);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Debug,
        Message = "{ClientId}-(CM)- KeepAlive is 0.  No pings will be sent.")]
    private static partial void LogKeepAliveZero(ILogger logger, string clientId);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(CM)- --> PingReq")]
    private static partial void LogSendingPingReq(ILogger logger, string clientId);

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Debug,
        Message = "{ClientId}-(CM)- {State}: last communications {Elapsed} ago")]
    private static partial void LogConnectionMonitorState(ILogger logger, string clientId, ConnectState state, TimeSpan elapsed);

    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Debug,
        Message = "{ClientId}-(CM)- SendQueue:...............{Count}")]
    private static partial void LogSendQueueCount(ILogger logger, string clientId, int count);

    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Debug,
        Message = "{ClientId}-(CM)- ReceivedQueue:...........{Count}")]
    private static partial void LogReceivedQueueCount(ILogger logger, string clientId, int count);

    [LoggerMessage(
        EventId = 1007,
        Level = LogLevel.Debug,
        Message = "{ClientId}-(CM)- OutgoingPublishQueue:....{Count}")]
    private static partial void LogOutgoingPublishQueueCount(ILogger logger, string clientId, int count);

    [LoggerMessage(
        EventId = 1008,
        Level = LogLevel.Debug,
        Message = "{ClientId}-(CM)- OPubTransactionQueue:....{Count}/{Capacity}")]
    private static partial void LogOPubTransactionQueueCount(ILogger logger, string clientId, int count, int capacity);

    [LoggerMessage(
        EventId = 1009,
        Level = LogLevel.Debug,
        Message = "{ClientId}-(CM)- IPubTransactionQueue:....{Count}/{Capacity}")]
    private static partial void LogIPubTransactionQueueCount(ILogger logger, string clientId, int count, int capacity);

    [LoggerMessage(
        EventId = 1010,
        Level = LogLevel.Debug,
        Message = "{ClientId}-(CM)- # of Subscriptions:......{Count}")]
    private static partial void LogSubscriptionsCount(ILogger logger, string clientId, int count);

    [LoggerMessage(
        EventId = 1011,
        Level = LogLevel.Debug,
        Message = "{ClientId}-(CM)- PacketIDsInUse:..........{Count}")]
    private static partial void LogPacketIDsInUseCount(ILogger logger, string clientId, int count);

    [LoggerMessage(
        EventId = 1012,
        Level = LogLevel.Debug,
        Message = "{ClientId}-(CM)- Stopped by cancellation token")]
    private static partial void LogConnectionMonitorCancelled(ILogger logger, string clientId);

    [LoggerMessage(
        EventId = 1013,
        Level = LogLevel.Error,
        Message = "{ClientId}-(CM)- Exception")]
    private static partial void LogConnectionMonitorException(ILogger logger, Exception ex, string clientId);

    [LoggerMessage(
        EventId = 1014,
        Level = LogLevel.Warning,
        Message = "{ClientId}-(CM)- Exception during disconnection")]
    private static partial void LogConnectionMonitorDisconnectException(ILogger logger, Exception ex, string clientId);

    [LoggerMessage(
        EventId = 1015,
        Level = LogLevel.Information,
        Message = "{ClientId}-(CM)- {TaskName} is not running.")]
    private static partial void LogTaskNotRunning(ILogger logger, string clientId, string taskName);

    [LoggerMessage(
        EventId = 1016,
        Level = LogLevel.Error,
        Message = "{ClientId}-(CM)- {TaskName} Faulted")]
    private static partial void LogTaskFaulted(ILogger logger, System.Exception ex, string clientId, string taskName);

    [LoggerMessage(
        EventId = 1017,
        Level = LogLevel.Error,
        Message = "{ClientId}-(CM)- {TaskName} died.  Disconnecting.")]
    private static partial void LogTaskDied(ILogger logger, string clientId, string taskName);

    [LoggerMessage(
        EventId = 1018,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(CM)- Already disconnected, skipping disconnection.")]
    private static partial void LogAlreadyDisconnected(ILogger logger, string clientId);

    [LoggerMessage(
        EventId = 1019,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(CM)- Disconnection already in progress, skipping duplicate call.")]
    private static partial void LogDisconnectionInProgress(ILogger logger, string clientId);

    [LoggerMessage(
        EventId = 1020,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(CM)- Already disconnected after acquiring semaphore.")]
    private static partial void LogAlreadyDisconnectedAfterSemaphore(ILogger logger, string clientId);

    [LoggerMessage(
        EventId = 1021,
        Level = LogLevel.Error,
        Message = "{ClientId}-(CM)- Exception during disconnection from health check")]
    private static partial void LogHealthCheckDisconnectException(ILogger logger, System.Exception ex, string clientId);

    // ConnectionPublishWriter logging
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(PW)- Starting...{State}")]
    private static partial void LogConnectionPublishWriterStarting(ILogger logger, string clientId, ConnectState state);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(PW)- Not connected.  Waiting for connect...")]
    private static partial void LogPublishWriterWaitingForConnect(ILogger logger, string clientId);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(PW)- --> Sending QoS={QoS} PublishPacket id={PacketId}")]
    private static partial void LogSendingQoSPublishPacket(ILogger logger, string clientId, QualityOfService? qos, int packetId);

    [LoggerMessage(
        EventId = 2004,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(PW)- --> Sending QoS 0 PublishPacket")]
    private static partial void LogSendingQoS0PublishPacket(ILogger logger, string clientId);

    [LoggerMessage(
        EventId = 2005,
        Level = LogLevel.Warning,
        Message = "Duplicate packet ID detected {PacketId} while queueing to transaction queue for an outgoing QoS {QoS} publish.")]
    private static partial void LogDuplicatePacketId(ILogger logger, int packetId, QualityOfService? qos);

    [LoggerMessage(
        EventId = 2006,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(PW)- ConnectionPublishWriter: Failed to write to transport.")]
    private static partial void LogPublishWriterWriteFailed(ILogger logger, string clientId);

    [LoggerMessage(
        EventId = 2007,
        Level = LogLevel.Debug,
        Message = "{ClientId}-(PW)- ConnectionPublishWriter: unexpected exit.  Disconnecting...")]
    private static partial void LogPublishWriterUnexpectedExit(ILogger logger, string clientId);

    [LoggerMessage(
        EventId = 2008,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(PW)- Cancelled & existing with {Count} publish packets remaining.")]
    private static partial void LogPublishWriterCancelled(ILogger logger, string clientId, int count);

    [LoggerMessage(
        EventId = 2009,
        Level = LogLevel.Error,
        Message = "{ClientId}-(PW)- Exception")]
    private static partial void LogPublishWriterException(ILogger logger, System.Exception ex, string clientId);

    [LoggerMessage(
        EventId = 2010,
        Level = LogLevel.Warning,
        Message = "{ClientId}-(PW)- Exception during disconnection")]
    private static partial void LogPublishWriterDisconnectException(ILogger logger, System.Exception ex, string clientId);

    [LoggerMessage(
        EventId = 2011,
        Level = LogLevel.Debug,
        Message = "{ClientId}-(PW)- ConnectionPublishWriter Exiting...{State}, cancellationRequested={CancellationRequested}")]
    private static partial void LogPublishWriterExiting(ILogger logger, string clientId, ConnectState state, bool cancellationRequested);

    // ConnectionWriter logging
    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(W)- Starting...{State}")]
    private static partial void LogConnectionWriterStarting(ILogger logger, string clientId, ConnectState state);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(W)- Not connected.  Waiting for connect...")]
    private static partial void LogWriterWaitingForConnect(ILogger logger, string clientId);

    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(W)- --> Sending ConnectPacket")]
    private static partial void LogSendingConnectPacket(ILogger logger, string clientId);

    [LoggerMessage(
        EventId = 3004,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(W)- --> Sending DisconnectPacket")]
    private static partial void LogSendingDisconnectPacket(ILogger logger, string clientId);

    [LoggerMessage(
        EventId = 3005,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(W)- --> Sending SubscribePacket id={PacketId}")]
    private static partial void LogSendingSubscribePacket(ILogger logger, string clientId, int packetId);

    [LoggerMessage(
        EventId = 3006,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(W)- --> Sending UnsubscribePacket id={PacketId}")]
    private static partial void LogSendingUnsubscribePacket(ILogger logger, string clientId, int packetId);

    [LoggerMessage(
        EventId = 3007,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(W)- --> Sending PubAckPacket id={PacketId} reason={ReasonCode}")]
    private static partial void LogSendingPubAckPacket(ILogger logger, string clientId, int packetId, PubAckReasonCode reasonCode);

    [LoggerMessage(
        EventId = 3008,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(W)- --> Sending PubRecPacket id={PacketId} reason={ReasonCode}")]
    private static partial void LogSendingPubRecPacket(ILogger logger, string clientId, int packetId, PubRecReasonCode reasonCode);

    [LoggerMessage(
        EventId = 3009,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(W)- --> Sending PubRelPacket id={PacketId} reason={ReasonCode}")]
    private static partial void LogSendingPubRelPacket(ILogger logger, string clientId, int packetId, PubRelReasonCode reasonCode);

    [LoggerMessage(
        EventId = 3010,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(W)- --> Sending PubCompPacket id={PacketId} reason={ReasonCode}")]
    private static partial void LogSendingPubCompPacket(ILogger logger, string clientId, int packetId, PubCompReasonCode reasonCode);

    [LoggerMessage(
        EventId = 3011,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(W)- --> Sending PingReqPacket")]
    private static partial void LogSendingPingReqPacket(ILogger logger, string clientId);

    [LoggerMessage(
        EventId = 3012,
        Level = LogLevel.Error,
        Message = "{ClientId}-(W)- Write failed.  Disconnecting...")]
    private static partial void LogWriterWriteFailed(ILogger logger, string clientId);

    [LoggerMessage(
        EventId = 3013,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(W)- Cancelled & exiting with {Count} packets remaining.")]
    private static partial void LogWriterCancelled(ILogger logger, string clientId, int count);

    [LoggerMessage(
        EventId = 3014,
        Level = LogLevel.Error,
        Message = "{ClientId}-(W)- Exception")]
    private static partial void LogWriterException(ILogger logger, System.Exception ex, string clientId);

    [LoggerMessage(
        EventId = 3015,
        Level = LogLevel.Warning,
        Message = "{ClientId}-(W)- Exception during disconnection")]
    private static partial void LogWriterDisconnectException(ILogger logger, System.Exception ex, string clientId);

    [LoggerMessage(
        EventId = 3016,
        Level = LogLevel.Debug,
        Message = "{ClientId}-(W)- ConnectionWriter Exiting...{State}, cancellationRequested={CancellationRequested}")]
    private static partial void LogWriterExiting(ILogger logger, string clientId, ConnectState state, bool cancellationRequested);

    // ConnectionReader logging
    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(R)- ConnectionReader Starting...{State}")]
    private static partial void LogConnectionReaderStarting(ILogger logger, string clientId, ConnectState state);

    [LoggerMessage(
        EventId = 4002,
        Level = LogLevel.Debug,
        Message = "{ClientId}-(R)- ConnectionReader exiting: Read from transport failed.")]
    private static partial void LogConnectionReaderReadFailed(ILogger logger, string clientId);

    [LoggerMessage(
        EventId = 4003,
        Level = LogLevel.Error,
        Message = "Malformed packet received.  Disconnecting...")]
    private static partial void LogMalformedPacket(ILogger logger);

    [LoggerMessage(
        EventId = 4004,
        Level = LogLevel.Debug,
        Message = "{ClientId}-(R)- Malformed packet received: {Packet}")]
    private static partial void LogMalformedPacketDetails(ILogger logger, string clientId, string packet);

    [LoggerMessage(
        EventId = 4005,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(R)- ConnectionReader: PacketDecoder.TryDecode returned false.  Waiting for more data...")]
    private static partial void LogWaitingForMoreData(ILogger logger, string clientId);

    [LoggerMessage(
        EventId = 4006,
        Level = LogLevel.Error,
        Message = "Received a packet that exceeds the requested maximum of {MaxPacketSize}.  Disconnecting.")]
    private static partial void LogPacketTooLarge(ILogger logger, long maxPacketSize);

    [LoggerMessage(
        EventId = 4007,
        Level = LogLevel.Debug,
        Message = "{ClientId}-(RPH)- Received packet size {PacketSize} for packet {PacketType}")]
    private static partial void LogPacketSizeDetails(ILogger logger, string clientId, long packetSize, string packetType);

    [LoggerMessage(
        EventId = 4008,
        Level = LogLevel.Debug,
        Message = "{ClientId}-(R)- Received a retransmitted publish packet with id={PacketId}.  Removing any prior transaction chain.")]
    private static partial void LogRetransmittedPublish(ILogger logger, string clientId, int packetId);

    [LoggerMessage(
        EventId = 4009,
        Level = LogLevel.Error,
        Message = "Received a publish with a duplicate packet identifier {PacketId} for a transaction already in progress.  Disconnecting.")]
    private static partial void LogDuplicatePublishPacketId(ILogger logger, int packetId);

    [LoggerMessage(
        EventId = 4010,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(R)- <-- Received {PacketType} id: {PacketId}.  Adding to receivedQueue.")]
    private static partial void LogReceivedPacket(ILogger logger, string clientId, string packetType, int packetId);

    [LoggerMessage(
        EventId = 4011,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(R)- Cancelled & exiting...")]
    private static partial void LogConnectionReaderCancelled(ILogger logger, string clientId);

    [LoggerMessage(
        EventId = 4012,
        Level = LogLevel.Error,
        Message = "{ClientId}-(R)- Exception")]
    private static partial void LogConnectionReaderException(ILogger logger, System.Exception ex, string clientId);

    [LoggerMessage(
        EventId = 4013,
        Level = LogLevel.Warning,
        Message = "{ClientId}-(R)- Exception during disconnection")]
    private static partial void LogConnectionReaderDisconnectException(ILogger logger, System.Exception ex, string clientId);

    [LoggerMessage(
        EventId = 4014,
        Level = LogLevel.Debug,
        Message = "{ClientId}-(R)- ConnectionReader Exiting...{State}, cancellationRequested={CancellationRequested}")]
    private static partial void LogConnectionReaderExiting(ILogger logger, string clientId, ConnectState state, bool cancellationRequested);

    // ReceivedPacketsHandler logging
    [LoggerMessage(
        EventId = 5001,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(RPH)- Starting...{State}")]
    private static partial void LogReceivedPacketsHandlerStarting(ILogger logger, string clientId, ConnectState state);

    [LoggerMessage(
        EventId = 5002,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(RPH)- <-- Received SubAck id={PacketId}")]
    private static partial void LogReceivedSubAck(ILogger logger, string clientId, int packetId);

    [LoggerMessage(
        EventId = 5003,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(RPH)- <-- Received UnsubAck id={PacketId}")]
    private static partial void LogReceivedUnsubAck(ILogger logger, string clientId, int packetId);

    [LoggerMessage(
        EventId = 5004,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(RPH)- <-- Received PingResp")]
    private static partial void LogReceivedPingResp(ILogger logger, string clientId);

    [LoggerMessage(
        EventId = 5005,
        Level = LogLevel.Error,
        Message = "{ClientId}-(RPH)- Incorrectly received Disconnect packet in ReceivedPacketsHandlerAsync")]
    private static partial void LogIncorrectDisconnectPacket(ILogger logger, string clientId);

    [LoggerMessage(
        EventId = 5006,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(RPH)- <-- Received Unknown packet type.  Will discard.")]
    private static partial void LogUnknownPacketType(ILogger logger, string clientId);

    [LoggerMessage(
        EventId = 5007,
        Level = LogLevel.Error,
        Message = "Unrecognized packet received.  Will discard. {Packet}")]
    private static partial void LogUnrecognizedPacket(ILogger logger, string packet);

    [LoggerMessage(
        EventId = 5008,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(RPH)- Cancelled with {Count} received packets remaining.  Exiting...")]
    private static partial void LogReceivedPacketsHandlerCancelled(ILogger logger, string clientId, int count);

    [LoggerMessage(
        EventId = 5009,
        Level = LogLevel.Error,
        Message = "{ClientId}-(RPH)- Exception")]
    private static partial void LogReceivedPacketsHandlerException(ILogger logger, System.Exception ex, string clientId);

    [LoggerMessage(
        EventId = 5010,
        Level = LogLevel.Warning,
        Message = "{ClientId}-(RPH)- Exception during disconnection")]
    private static partial void LogReceivedPacketsHandlerDisconnectException(ILogger logger, System.Exception ex, string clientId);

    [LoggerMessage(
        EventId = 5011,
        Level = LogLevel.Debug,
        Message = "{ClientId}-(RPH)- ReceivedPacketsHandler Exiting...{State}, cancellationRequested={CancellationRequested}")]
    private static partial void LogReceivedPacketsHandlerExiting(ILogger logger, string clientId, ConnectState state, bool cancellationRequested);

    // ConnectionManager initialization logging
    [LoggerMessage(
        EventId = 6001,
        Level = LogLevel.Trace,
        Message = "Trace Level Logging Legend:")]
    private static partial void LogTraceLevelLoggingLegend(ILogger logger);

    [LoggerMessage(
        EventId = 6002,
        Level = LogLevel.Trace,
        Message = "    -(W)-   == ConnectionWriter")]
    private static partial void LogLegendConnectionWriter(ILogger logger);

    [LoggerMessage(
        EventId = 6003,
        Level = LogLevel.Trace,
        Message = "    -(PW)-  == ConnectionPublishWriter")]
    private static partial void LogLegendConnectionPublishWriter(ILogger logger);

    [LoggerMessage(
        EventId = 6004,
        Level = LogLevel.Trace,
        Message = "    -(R)-   == ConnectionReader")]
    private static partial void LogLegendConnectionReader(ILogger logger);

    [LoggerMessage(
        EventId = 6005,
        Level = LogLevel.Trace,
        Message = "    -(CM)-  == ConnectionMonitor")]
    private static partial void LogLegendConnectionMonitor(ILogger logger);

    [LoggerMessage(
        EventId = 6006,
        Level = LogLevel.Trace,
        Message = "    -(RPH)- == ReceivedPacketsHandler")]
    private static partial void LogLegendReceivedPacketsHandler(ILogger logger);

    // ConnectionManagerHandlers logging
    [LoggerMessage(
        EventId = 7001,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(RPH)- <-- Received ConnAck")]
    private static partial void LogReceivedConnAck(ILogger logger, string clientId);

    [LoggerMessage(
        EventId = 7002,
        Level = LogLevel.Debug,
        Message = "{ClientId}-(RPH)- <-- Broker ReceiveMaximum is {ReceiveMaximum}.")]
    private static partial void LogBrokerReceiveMaximum(ILogger logger, string clientId, ushort? receiveMaximum);

    [LoggerMessage(
        EventId = 7003,
        Level = LogLevel.Error,
        Message = "--> Disconnect received <--: {ReasonCode} {ReasonString}")]
    private static partial void LogDisconnectReceived(ILogger logger, DisconnectReasonCode reasonCode, string? reasonString);

    [LoggerMessage(
        EventId = 7004,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(RPH)- <-- Received QoS 0 Publish")]
    private static partial void LogReceivedQoS0Publish(ILogger logger, string clientId);

    [LoggerMessage(
        EventId = 7005,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(RPH)- <-- Received QoS 1 Publish id={PacketId}")]
    private static partial void LogReceivedQoS1Publish(ILogger logger, string clientId, int packetId);

    [LoggerMessage(
        EventId = 7006,
        Level = LogLevel.Error,
        Message = "QoS1: Couldn't update Publish --> PubAck QoS1 Chain for packet identifier {PacketId}. Discarded.")]
    private static partial void LogQoS1UpdateChainFailed(ILogger logger, int packetId);

    [LoggerMessage(
        EventId = 7007,
        Level = LogLevel.Error,
        Message = "QoS1: Received Publish with an unknown packet identifier {PacketId}.")]
    private static partial void LogQoS1UnknownPacketId(ILogger logger, int packetId);

    [LoggerMessage(
        EventId = 7008,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(RPH)- <-- Received QoS 2 Publish id={PacketId}")]
    private static partial void LogReceivedQoS2Publish(ILogger logger, string clientId, int packetId);

    [LoggerMessage(
        EventId = 7009,
        Level = LogLevel.Error,
        Message = "QoS2: Couldn't update Publish --> PubRec QoS2 Chain for packet identifier {PacketId}. Discarded.")]
    private static partial void LogQoS2UpdateChainFailed(ILogger logger, int packetId);

    [LoggerMessage(
        EventId = 7010,
        Level = LogLevel.Error,
        Message = "QoS2: Received Publish with an unknown packet identifier {PacketId}.")]
    private static partial void LogQoS2UnknownPacketId(ILogger logger, int packetId);

    [LoggerMessage(
        EventId = 7011,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(RPH)- <-- Received PubAck id={PacketId} reason={ReasonCode}")]
    private static partial void LogReceivedPubAck(ILogger logger, string clientId, int packetId, PubAckReasonCode reasonCode);

    [LoggerMessage(
        EventId = 7012,
        Level = LogLevel.Warning,
        Message = "QoS1: Received PubAck with an unknown packet identifier {PacketId}. Discarded.")]
    private static partial void LogQoS1UnknownPubAckPacketId(ILogger logger, int packetId);

    [LoggerMessage(
        EventId = 7013,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(RPH)- <-- Received PubRec id={PacketId} reason={ReasonCode}")]
    private static partial void LogReceivedPubRec(ILogger logger, string clientId, int packetId, PubRecReasonCode reasonCode);

    [LoggerMessage(
        EventId = 7014,
        Level = LogLevel.Error,
        Message = "QoS2: Couldn't update PubRec --> PubRel QoS2 Chain for packet identifier {PacketId}.")]
    private static partial void LogQoS2PubRecUpdateFailed(ILogger logger, int packetId);

    [LoggerMessage(
        EventId = 7015,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(RPH)- <-- Received PubRel id={PacketId} reason={ReasonCode}")]
    private static partial void LogReceivedPubRel(ILogger logger, string clientId, int packetId, PubRelReasonCode reasonCode);

    [LoggerMessage(
        EventId = 7016,
        Level = LogLevel.Warning,
        Message = "QoS2: Couldn't update PubRel --> PubComp QoS2 Chain for packet identifier {PacketId}.")]
    private static partial void LogQoS2PubRelUpdateFailed(ILogger logger, int packetId);

    [LoggerMessage(
        EventId = 7017,
        Level = LogLevel.Warning,
        Message = "QoS2: Received PubRel with an unknown packet identifier {PacketId}. Responding with PubComp PacketIdentifierNotFound.")]
    private static partial void LogQoS2UnknownPubRelPacketId(ILogger logger, int packetId);

    [LoggerMessage(
        EventId = 7018,
        Level = LogLevel.Warning,
        Message = "QoS1: Couldn't remove PubAck --> Publish QoS1 Chain for packet identifier {PacketId}.")]
    private static partial void LogQoS1RemoveChainFailed(ILogger logger, int packetId);

    [LoggerMessage(
        EventId = 7019,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(RPH)- <-- Sent PubComp id={PacketId} reason={ReasonCode}")]
    private static partial void LogSentPubComp(ILogger logger, string clientId, int packetId, PubCompReasonCode reasonCode);

    [LoggerMessage(
        EventId = 7020,
        Level = LogLevel.Trace,
        Message = "{ClientId}-(RPH)- <-- Received PubComp id={PacketId} reason={ReasonCode}")]
    private static partial void LogReceivedPubComp(ILogger logger, string clientId, int packetId, PubCompReasonCode reasonCode);

    [LoggerMessage(
        EventId = 7021,
        Level = LogLevel.Warning,
        Message = "QoS2: Received PubComp with an unknown packet identifier {PacketId}. Discarded.")]
    private static partial void LogQoS2UnknownPubCompPacketId(ILogger logger, int packetId);

    [LoggerMessage(
        EventId = 7022,
        Level = LogLevel.Trace,
        Message = "HandleDisconnection: Already disconnected.")]
    private static partial void LogHandleDisconnectionAlreadyDisconnected(ILogger logger);

    [LoggerMessage(
        EventId = 7023,
        Level = LogLevel.Debug,
        Message = "HandleDisconnection: Handling disconnection. clean={Clean}.")]
    private static partial void LogHandleDisconnection(ILogger logger, bool clean);

    [LoggerMessage(
        EventId = 7024,
        Level = LogLevel.Warning,
        Message = "HandleDisconnection: Send queue not empty. {Count} packets pending but we are disconnecting.")]
    private static partial void LogHandleDisconnectionSendQueueNotEmpty(ILogger logger, int count);

    [LoggerMessage(
        EventId = 7025,
        Level = LogLevel.Warning,
        Message = "HandleDisconnection: Outgoing publish queue not empty. {Count} packets pending but we are disconnecting.")]
    private static partial void LogHandleDisconnectionOutgoingPublishQueueNotEmpty(ILogger logger, int count);

    // ConnectionManager.cs logging methods
    [LoggerMessage(
        EventId = 9001,
        Level = LogLevel.Error,
        Message = "Failed to connect to broker")]
    private static partial void LogFailedToConnectToBroker(ILogger logger);

    [LoggerMessage(
        EventId = 9002,
        Level = LogLevel.Trace,
        Message = "All background tasks completed successfully")]
    private static partial void LogAllBackgroundTasksCompleted(ILogger logger);

    [LoggerMessage(
        EventId = 9003,
        Level = LogLevel.Warning,
        Message = "Background tasks did not complete within timeout. {TaskCount} task(s) may still be running.")]
    private static partial void LogBackgroundTasksTimeout(ILogger logger, int taskCount);

    [LoggerMessage(
        EventId = 9004,
        Level = LogLevel.Warning,
        Message = "Exception while waiting for background tasks to complete")]
    private static partial void LogExceptionWhileWaitingForBackgroundTasks(ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 9005,
        Level = LogLevel.Warning,
        Message = "Task faulted during cancellation")]
    private static partial void LogTaskFaultedDuringCancellation(ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 9006,
        Level = LogLevel.Trace,
        Message = "Disposing ConnectionManager")]
    private static partial void LogDisposingConnectionManager(ILogger logger);
}
