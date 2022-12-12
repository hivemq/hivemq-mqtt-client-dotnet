namespace HiveMQtt.Test;

using HiveMQtt.Client;
using Xunit;

public class HiveClientOptionsTest
{
    [Fact]
    public void WithBadKeepAlive()
    {
        var options = new HiveClientOptions
        {
            KeepAlive = -300,
        };
        options.ValidateOptions();

        Assert.Equal(0, options.KeepAlive);
    }

    [Fact]
    public void WithNullifiedClientID()
    {
        var options = new HiveClientOptions
        {
            ClientId = null,
        };
        options.ValidateOptions();

        Assert.NotNull(options.ClientId);
        Assert.True(options.ClientId.Length < 24);
    }
}
