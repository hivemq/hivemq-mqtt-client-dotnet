namespace HiveMQtt.Test;

using HiveMQtt;
using Xunit;

public class ClientOptionsTest
{
    [Fact]
    public void WithBadKeepAlive()
    {
        var options = new ClientOptions
        {
            KeepAlive = -300,
        };
        options.ValidateOptions();

        Assert.Equal(0, options.KeepAlive);
    }

    [Fact]
    public void WithNullifiedClientID()
    {
        var options = new ClientOptions
        {
            ClientId = null,
        };
        options.ValidateOptions();

        Assert.NotNull(options.ClientId);
        Assert.True(options.ClientId.Length < 24);
    }
}
