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
namespace HiveMQtt.Test.Packets;

using HiveMQtt.Client.Results;
using HiveMQtt.MQTT5.Packets;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;

/// <summary>
/// API-surface tests for PUBACK/PUBREC ReasonString (issue #300).
/// Real end-to-end validation is in PublishTest and RawClientPublishTest (client + broker).
/// </summary>
public class PubAckPacketTest
{
    [Fact]
    public void PubAckPacket_Constructor_ReasonStringIsNull()
    {
        var packet = new PubAckPacket(1, PubAckReasonCode.Success);
        Assert.Null(packet.ReasonString);
        Assert.Equal(PubAckReasonCode.Success, packet.ReasonCode);
    }

    [Fact]
    public void PublishResult_MessageOnly_QoS1AndQoS2ReasonStringAreNull()
    {
#pragma warning disable IDE0301 // Collection initialization can be simplified
        var message = new MQTT5PublishMessage { Topic = "test/topic", Payload = Array.Empty<byte>() };
#pragma warning restore IDE0301
        var result = new PublishResult(message);
        Assert.Null(result.QoS1ReasonString);
        Assert.Null(result.QoS2ReasonString);
    }
}
