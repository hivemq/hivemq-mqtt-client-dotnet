namespace HiveMQtt.Test.HiveMQClient;

using System.Text;
using Xunit;

using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;

public class ClientOptionsBuilderTest
{
    [Theory]
#pragma warning disable CS3016 // Arrays as attribute arguments is not CLS-compliant
    [InlineData("mqtt.example.com", 1883, "myClientId", true, true, true, 60, "UsernamePassword", "authData", "myUserName", "myPassword", true, 10, true, true)]
#pragma warning restore CS3016 // Arrays as attribute arguments is not CLS-compliant
    public void Build_WithValidParameters_ReturnsValidOptions(
        string broker,
        int port,
        string clientId,
        bool allowInvalidCertificates,
        bool useTls,
        bool cleanStart,
        short keepAlive,
        string authMethod,
        string authData,
        string username,
        string password,
        bool preferIPv6,
        int topicAliasMaximum,
        bool requestResponseInfo,
        bool requestProblemInfo)
    {
        // Arrange
        var builder = new HiveMQClientOptionsBuilder()
            .WithBroker(broker)
            .WithPort(port)
            .WithClientId(clientId)
            .WithAllowInvalidBrokerCertificates(allowInvalidCertificates)
            .WithUseTls(useTls)
            .WithCleanStart(cleanStart)
            .WithKeepAlive(keepAlive)
            .WithAuthenticationMethod(authMethod)
            .WithAuthenticationData(Encoding.UTF8.GetBytes(authData))
            .WithUserProperties(new Dictionary<string, string> { { "property1", "value1" }, { "property2", "value2" } })
            .WithLastWillAndTestament(new LastWillAndTestament("lwt/topic", QualityOfService.AtLeastOnceDelivery, "LWT message", true))
            .WithMaximumPacketSize(1024)
            .WithReceiveMaximum(100)
            .WithSessionExpiryInterval(3600)
            .WithUserName(username)
            .WithPassword(password)
            .WithPreferIPv6(preferIPv6)
            .WithTopicAliasMaximum(topicAliasMaximum)
            .WithRequestResponseInformation(requestResponseInfo)
            .WithRequestProblemInformation(requestProblemInfo);

        // Act
        var options = builder.Build();

        // Assert
        Assert.Equal(broker, options.Host);
        Assert.Equal(port, options.Port);
        Assert.Equal(clientId, options.ClientId);
        Assert.Equal(allowInvalidCertificates, options.AllowInvalidBrokerCertificates);
        Assert.Equal(useTls, options.UseTLS);
        Assert.Equal(cleanStart, options.CleanStart);
        Assert.Equal(keepAlive, options.KeepAlive);
        Assert.Equal(authMethod, options.AuthenticationMethod);
        Assert.Equal(Encoding.UTF8.GetBytes(authData), options.AuthenticationData);
        Assert.Equal(username, options.UserName);
        Assert.Equal(password, options.Password);
        Assert.Equal(preferIPv6, options.PreferIPv6);
        Assert.Equal(topicAliasMaximum, options.ClientTopicAliasMaximum);
        Assert.Equal(requestResponseInfo, options.RequestResponseInformation);
        Assert.Equal(requestProblemInfo, options.RequestProblemInformation);
        Assert.NotNull(options.UserProperties);
        Assert.NotNull(options.LastWillAndTestament);
        Assert.Equal(2, options.UserProperties.Count);
        Assert.Equal("LWT message", options.LastWillAndTestament.PayloadAsString);
    }
}
