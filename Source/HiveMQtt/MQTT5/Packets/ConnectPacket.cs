namespace HiveMQtt.MQTT5.Packets;

using System.IO;
using System.Text;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// An MQTT Connect Control Packet as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901033.
/// </summary>
public class ConnectPacket : ControlPacket
{
    private readonly HiveClientOptions clientOptions;

    private byte flags;

    public ConnectPacket(HiveClientOptions clientOptions) => this.clientOptions = clientOptions;

    public override ControlPacketType ControlPacketType => ControlPacketType.Connect;

    /// <summary>
    /// Encode this packet to be sent on the wire.
    /// </summary>
    /// <returns>An array of bytes ready to be sent.</returns>
    public byte[] Encode()
    {
        this.GatherConnectFlagsAndProperties();

        var stream = new MemoryStream(100)
        {
            Position = 2,
        };

        // Variable Header - starts at byte 2
        stream.WriteByte(0);
        stream.WriteByte(4);
        stream.Write(Encoding.UTF8.GetBytes("MQTT"));
        stream.WriteByte(0x5); // Protocol Version
        stream.WriteByte(this.flags); // Connect Flags
        _ = EncodeTwoByteInteger(stream, this.clientOptions.KeepAlive);
        this.EncodeProperties(stream);

        // Payload
        _ = EncodeUTF8String(stream, this.clientOptions.ClientId);

        var length = stream.Length - 2;

        // Fixed Header - Add to the beginning of the stream
        stream.Position = 0;
        stream.WriteByte(((byte)ControlPacketType.Connect) << 4);
        _ = EncodeVariableByteInteger(stream, (int)length);

        return stream.ToArray();
    }

    internal void GatherConnectFlagsAndProperties()
    {
        this.clientOptions.ValidateOptions();

        this.flags = 0x0;
        if (this.clientOptions.CleanStart is true)
        {
            this.flags |= 0x2;
        }

        // TODO: LWT options
        if (this.clientOptions.UserName != null)
        {
            this.flags |= 0x80;
        }

        if (this.clientOptions.Password != null)
        {
            this.flags |= 0x40;
        }

        // Properties
        this.Properties.SessionExpiryInterval = (UInt32)this.clientOptions.SessionExpiryInterval;

        if (this.clientOptions.ClientReceiveMaximum != null)
        {
            this.Properties.ReceiveMaximum = (UInt16)this.clientOptions.ClientReceiveMaximum;
        }

        if (this.clientOptions.ClientMaximumPacketSize != null)
        {
            this.Properties.MaximumPacketSize = (UInt16)this.clientOptions.ClientMaximumPacketSize;
        }

        if (this.clientOptions.ClientTopicAliasMaximum != null)
        {
            this.Properties.TopicAliasMaximum = (ushort)RangeValidate(
                (int)this.clientOptions.ClientTopicAliasMaximum,
                MQTT5DataType.TwoByteInteger);
        }

        if (this.clientOptions.RequestResponseInformation != null)
        {
            if (this.clientOptions.RequestResponseInformation == true)
            {
                this.Properties.RequestResponseInformation = 1;
            }
            else
            {
                this.Properties.RequestResponseInformation = 0;
            }
        }

        if (this.clientOptions.RequestProblemInformation != null)
        {
            if (this.clientOptions.RequestProblemInformation == true)
            {
                this.Properties.RequestProblemInformation = 1;
            }
            else
            {
                this.Properties.RequestProblemInformation = 0;
            }
        }

        if (this.clientOptions.UserProperties != null)
        {
            this.Properties.UserProperties = this.clientOptions.UserProperties;
        }

        if (this.clientOptions.AuthenticationMethod != null)
        {
            this.Properties.AuthenticationMethod = this.clientOptions.AuthenticationMethod;
        }

        if (this.clientOptions.AuthenticationData != null)
        {
            this.Properties.AuthenticationData = this.clientOptions.AuthenticationData;
        }
    }

    internal static int RangeValidate(int value, MQTT5DataType type)
    {
        var result = value;

        switch (type)
        {
            case MQTT5DataType.TwoByteInteger:
                if (value < 0)
                {
                    result = 0;
                }
                else if (value > 255)
                {
                    result = 255;
                }

                break;
            case MQTT5DataType.FourByteInteger:
                if (value < 0)
                {
                    result = 0;
                }
                else if (value > 65535)
                {
                    result = 65535;
                }

                break;
            case MQTT5DataType.VariableByteInteger:
                if (value < 0)
                {
                    result = 0;
                }
                else if (value > 268435455)
                {
                    result = 268435455;
                }

                break;
            default:
                result = value;
                break;
        }

        return result;
    }

    internal static int RangeValidate(int value, int min, int max)
    {
        var result = value;

        if (value < min)
        {
            result = min;
        }
        else if (value > max)
        {
            result = max;
        }

        return result;
    }
}
