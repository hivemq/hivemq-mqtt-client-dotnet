/*
 * Copyright 2023-present HiveMQ and the HiveMQ Community
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
namespace HiveMQtt.Client;

using System.Text;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// Represents a Last Will and Testament message.
/// </summary>
public class LastWillAndTestament
{
    public LastWillAndTestament(string topic, QualityOfService? qos, string payload, bool retain = false)
    {
        this.Topic = topic;
        this.QoS = qos;
        this.PayloadAsString = payload;
        this.Retain = retain;
        this.UserProperties = new Dictionary<string, string>();
    }

    public LastWillAndTestament(string topic, QualityOfService? qos, byte[] payload, bool retain = false)
    {
        this.Topic = topic;
        this.QoS = qos;
        this.Payload = payload;
        this.Retain = retain;
        this.UserProperties = new Dictionary<string, string>();
    }

    /// <summary>
    /// Gets or sets the topic of this Publish.
    /// </summary>
    public string? Topic { get; set; }

    /// <summary>
    /// Gets or sets the Quality of Service level for this publish.
    /// </summary>
    public QualityOfService? QoS { get; set; }

    /// <summary>
    ///  Gets or sets the UTF-8 encoded payload of this Publish.
    /// </summary>
    public byte[]? Payload { get; set; }

    /// <summary>
    /// Gets or sets the Payload as a string.
    /// </summary>
    public string PayloadAsString
    {
        get
        {
            if (this.Payload is null)
            {
                return string.Empty;
            }

            return Encoding.UTF8.GetString(this.Payload);
        }

        set
        {
            if (value is null)
            {
                this.Payload = null;
            }
            else
            {
                this.Payload = Encoding.UTF8.GetBytes(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this will should be retained by the MQTT broker when published.
    /// <para>
    /// See <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901042">Retain</seealso>.
    /// </para>
    /// </summary>
    public bool Retain { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the delay before the LWT is sent.  The Server delays publishing the Client’s Will Message until the Will Delay Interval has passed or the Session ends, whichever happens first.
    /// <para>
    /// See <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901062">Will Delay Interval</seealso>.
    /// </para>
    /// </summary>
    public Int64? WillDelayInterval { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the format of the Payload.
    /// </summary>
    public byte? PayloadFormatIndicator { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the lifetime of the message in seconds.
    /// </summary>
    public Int64? MessageExpiryInterval { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the content type of the Payload.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the topic to which the client should publish a response.
    /// </summary>
    public string? ResponseTopic { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the correlation data.
    /// </summary>
    public byte[]? CorrelationData { get; set; }

    /// <summary>
    /// Gets or sets a Dictionary containing the User Properties to be sent with the Last Will and Testament message.
    /// </summary>
    public Dictionary<string, string> UserProperties { get; set; }
}
