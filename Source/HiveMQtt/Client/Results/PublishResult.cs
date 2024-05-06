/*
 * Copyright 2022-present HiveMQ and the HiveMQ Community
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace HiveMQtt.Client.Results;

using HiveMQtt.MQTT5.Packets;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// Results of the Publish operation.
/// </summary>
public class PublishResult
{
    /// <summary>
    /// The on the wire PubAck packet received in response to a QoS 1 publish.
    /// </summary>
    private readonly PubAckPacket? pubAckPacket;

    /// <summary>
    /// Gets the on the wire PubRec packet received in response to a QoS 2 publish.
    /// </summary>
    private readonly PubRecPacket? pubRecPacket;

    public PublishResult(MQTT5PublishMessage message) => this.Message = message;

    public PublishResult(MQTT5PublishMessage message, PubAckPacket pubAckPacket)
    {
        this.Message = message;
        this.pubAckPacket = pubAckPacket;
        this.QoS1ReasonCode = pubAckPacket.ReasonCode;
    }

    public PublishResult(MQTT5PublishMessage message, PubRecPacket pubRecPacket)
    {
        this.Message = message;
        this.pubRecPacket = pubRecPacket;
        this.QoS2ReasonCode = pubRecPacket.ReasonCode;
    }

    /// <summary>
    /// Gets or sets the reason code of the PubAck packet for QoS 1 publishes.
    /// </summary>
    public PubAckReasonCode? QoS1ReasonCode { get; set; }

    /// <summary>
    /// Gets or sets the reason code of the PubRec packet for QoS 2 publishes.
    /// </summary>
    public PubRecReasonCode? QoS2ReasonCode { get; set; }

    /// <summary>
    /// Gets the message that was published.
    /// </summary>
    public MQTT5PublishMessage Message { get; }

    /// <summary>
    /// For Quality of Service levels 1 and 2, there will be a reason code in response to the publish. These
    /// reason codes are stored in QoS2ReasonCode and QoS1ReasonCode.  For Quality of Service level 0,
    /// there will be no reason code.
    /// </summary>
    /// <returns>The reason code integer value for QoS 1 and 2 publishes, or 0 for QoS 0 publishes.</returns>
    public int ReasonCode()
    {
        if (this.pubAckPacket != null)
        {
            return (int)this.pubAckPacket.ReasonCode;
        }

        if (this.pubRecPacket != null)
        {
            return (int)this.pubRecPacket.ReasonCode;
        }

        return 0;
    }
}
