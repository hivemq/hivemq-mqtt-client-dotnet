namespace HiveMQtt.Test;

using HiveMQtt;
using Xunit;

public class ClientTest
{
    [Fact]
    public void Given_When_Then()
    {
        var client = new Client();

        Assert.NotNull(client);
    }
}
