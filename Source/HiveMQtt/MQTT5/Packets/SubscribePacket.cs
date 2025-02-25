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
namespace HiveMQtt.MQTT5.Packets;

using System.IO;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// An MQTT Subscribe Control Packet.
///
/// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901205">
/// Subscribe Control Packet</seealso>.
/// </summary>
public class SubscribePacket : ControlPacket
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SubscribePacket"/> class
    /// with the options to be used for the publish.
    /// </summary>
    /// <param name="options">The raw packet data off the wire.</param>
    /// <param name="packetIdentifier">A unique packet identifier for the packet to be created.</param>
    /// <param name="userProperties">User properties to be sent with the packet.</param>
    public SubscribePacket(SubscribeOptions options, ushort packetIdentifier, Dictionary<string, string>? userProperties = null)
    {
        this.PacketIdentifier = packetIdentifier;
        this.Options = options;
        if (userProperties != null)
        {
            this.Properties.UserProperties = userProperties;
        }

        // Setup the TaskCompletionSource so users can simply call
        //
        //   await SubscribePacket.OnCompleteTCS
        //
        // to wait for the subscribe transaction to complete.
        this.OnComplete += (sender, args) => this.OnCompleteTCS.SetResult(args.SubAckPacket);
    }

    /// <summary>
    /// Gets or sets the options for an outgoing Subscribe packet.
    /// </summary>
    public SubscribeOptions Options { get; set; }

    /// <inheritdoc/>
    public override ControlPacketType ControlPacketType => ControlPacketType.Subscribe;

    /// <summary>
    /// Valid for outgoing Subscribe packets.  An event that is fired after the the subscribe transaction is complete.
    /// </summary>
    public event EventHandler<OnSubAckReceivedEventArgs> OnComplete = new((client, e) => { });

    internal virtual void OnCompleteEventLauncher(SubAckPacket packet)
    {
        if (this.OnComplete != null && this.OnComplete.GetInvocationList().Length > 0)
        {
            var eventArgs = new OnSubAckReceivedEventArgs(packet);
            Logger.Trace("SubscribePacket.OnCompleteEventLauncher");
            _ = Task.Run(() => this.OnComplete?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        if (t.Exception is not null)
                        {
                            Logger.Error("SubscribePacket.OnCompleteEventLauncher exception: " + t.Exception.Message);
                            foreach (var ex in t.Exception.InnerExceptions)
                            {
                                Logger.Error("SubscribePacket.OnCompleteEventLauncher inner exception: " + ex.Message);
                            }
                        }
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Gets the awaitable TaskCompletionSource for the subscribe transaction.
    /// <para>
    /// Valid for outgoing subscribe packets.  A TaskCompletionSource that is set when the subscribe transaction is complete.
    /// </para>
    /// </summary>
    public TaskCompletionSource<SubAckPacket> OnCompleteTCS { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Encode this packet to be sent on the wire.
    /// </summary>
    /// <returns>An array of bytes ready to be sent.</returns>
    public byte[] Encode()
    {
        using (var vhAndPayloadStream = new MemoryStream())
        {
            // Variable Header
            EncodeTwoByteInteger(vhAndPayloadStream, this.PacketIdentifier);
            this.EncodeProperties(vhAndPayloadStream);

            // Payload
            foreach (var tf in this.Options.TopicFilters)
            {
                EncodeUTF8String(vhAndPayloadStream, tf.Topic);

                var optionsByte = (byte)tf.QoS;
                if (tf.NoLocal is true)
                {
                    optionsByte |= 0x4;
                }

                if (tf.RetainAsPublished is true)
                {
                    optionsByte |= 0x8;
                }

                if (tf.RetainHandling is RetainHandling.SendAtSubscribeIfNewSubscription)
                {
                    optionsByte |= 0x10;
                }
                else if (tf.RetainHandling is RetainHandling.DoNotSendAtSubscribe)
                {
                    optionsByte |= 0x20;
                }

                vhAndPayloadStream.WriteByte(optionsByte);
            }

            // Construct the final packet
            var constructedPacket = new MemoryStream((int)vhAndPayloadStream.Length + 5);

            // Write the Fixed Header
            var byte1 = (byte)ControlPacketType.Subscribe << 4;
            byte1 |= 0x2;
            constructedPacket.WriteByte((byte)byte1);
            _ = EncodeVariableByteInteger(constructedPacket, (int)vhAndPayloadStream.Length);

            // Copy the Variable Header and Payload
            vhAndPayloadStream.Position = 0;
            vhAndPayloadStream.CopyTo(constructedPacket);

            return constructedPacket.ToArray();
        }
    }

    /// <summary>
    /// Gather the flags and properties for a Subscribe packet from <see cref="SubscribeOptions"/>
    /// as data preparation for encoding in <see cref="SubscribePacket"/>.
    /// </summary>
    internal void GatherSubscribeFlagsAndProperties() => this.Options.Validate();
}
