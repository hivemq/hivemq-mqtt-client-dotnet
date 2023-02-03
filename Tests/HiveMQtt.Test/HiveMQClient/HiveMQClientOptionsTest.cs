namespace HiveMQtt.Test.HiveMQClient;

using HiveMQtt.Client.Options;
using Xunit;

public class HiveMQClientOptionsTest
{
    [Fact]
    public void WithBadKeepAlive()
    {
        var options = new HiveMQClientOptions
        {
            KeepAlive = -300,
        };
        options.ValidateOptions();

        Assert.Equal(0, options.KeepAlive);

        options.KeepAlive = int.MaxValue;
        options.ValidateOptions();

        Assert.Equal(UInt16.MaxValue, options.KeepAlive);
    }

    [Fact]
    public void WithBadSessionExpiryInterval()
    {
        var options = new HiveMQClientOptions
        {
            SessionExpiryInterval = -300,
        };
        options.ValidateOptions();

        Assert.Equal(0, options.SessionExpiryInterval);

        options.SessionExpiryInterval = long.MaxValue;
        options.ValidateOptions();

        Assert.Equal(UInt32.MaxValue, options.SessionExpiryInterval);
    }

    [Fact]
    public void WithBadClientReceiveMaximum()
    {
        var options = new HiveMQClientOptions
        {
            ClientReceiveMaximum = -300,
        };
        options.ValidateOptions();
        Assert.Equal(0, options.ClientReceiveMaximum);

        options.ClientReceiveMaximum = int.MaxValue;
        options.ValidateOptions();
        Assert.Equal(UInt16.MaxValue, options.ClientReceiveMaximum);
    }

    [Fact]
    public void WithBadClientMaximumPacketSize()
    {
        var options = new HiveMQClientOptions
        {
            ClientMaximumPacketSize = -300,
        };
        options.ValidateOptions();
        Assert.Equal(0, options.ClientMaximumPacketSize);

        options.ClientMaximumPacketSize = long.MaxValue;
        options.ValidateOptions();
        Assert.Equal(UInt32.MaxValue, options.ClientMaximumPacketSize);
    }

    [Fact]
    public void WithBadClientTopicAliasMaximum()
    {
        var options = new HiveMQClientOptions
        {
            ClientTopicAliasMaximum = -300,
        };
        options.ValidateOptions();
        Assert.Equal(0, options.ClientTopicAliasMaximum);

        options.ClientTopicAliasMaximum = UInt16.MaxValue;
        options.ValidateOptions();
        Assert.Equal(UInt16.MaxValue, options.ClientTopicAliasMaximum);
    }

    [Fact]
    public void WithCustomUserProperties()
    {
        var userProperties = new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", "value2" },
        };

        var options = new HiveMQClientOptions
        {
            UserProperties = userProperties,
        };
        options.ValidateOptions();

        Assert.Equal(userProperties, options.UserProperties);
        Assert.True(options.UserProperties.ContainsKey("key1"));
        Assert.True(options.UserProperties.ContainsKey("key2"));
        Assert.True(options.UserProperties.ContainsValue("value1"));
        Assert.True(options.UserProperties.ContainsValue("value2"));
    }

    [Fact]
    public void WithNullifiedClientID()
    {
        var options = new HiveMQClientOptions
        {
            ClientId = null,
        };
        options.ValidateOptions();

        Assert.NotNull(options.ClientId);
        Assert.True(options.ClientId.Length < 24);
    }

    // TODO: Add tests for Authentication Method and Authentication Data
}
