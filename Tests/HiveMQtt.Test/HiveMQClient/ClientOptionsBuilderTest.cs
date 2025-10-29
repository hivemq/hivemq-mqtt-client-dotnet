namespace HiveMQtt.Test.HiveMQClient;

using System.Text;
using Xunit;

using HiveMQtt.Client;

public class ClientOptionsBuilderTest
{
    [Theory]
#pragma warning disable CS3016 // Arrays as attribute arguments is not CLS-compliant
    [InlineData("mqtt.example.com", 1883, "myClientId", true, true, true, 60, "UsernamePassword", "authData", "myUserName", "myPassword", true, 10, true, true, "HiveMQClient/TestFiles/hivemq-server-cert.pem")]
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
        bool requestProblemInfo,
        string clientCertificatePath)
    {
        // Arrange
#pragma warning disable CS0618 // Test validates obsolete method still works
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
            .WithLastWillAndTestament(new LastWillAndTestament("lwt/topic", "LWT message"))
            .WithMaximumPacketSize(1024)
            .WithReceiveMaximum(100)
            .WithSessionExpiryInterval(3600)
            .WithUserName(username)
            .WithPassword(password)
            .WithPreferIPv6(preferIPv6)
            .WithTopicAliasMaximum(topicAliasMaximum)
            .WithRequestResponseInformation(requestResponseInfo)
            .WithRequestProblemInformation(requestProblemInfo)
            .WithClientCertificate(clientCertificatePath);
#pragma warning restore CS0618

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

        // Convert SecureString to string for comparison
        string? passwordString = null;
        if (options.Password != null)
        {
            var ptr = IntPtr.Zero;
            try
            {
                ptr = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(options.Password);
                passwordString = System.Runtime.InteropServices.Marshal.PtrToStringUni(ptr);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(ptr);
                }
            }
        }

        Assert.Equal(password, passwordString);
        Assert.Equal(preferIPv6, options.PreferIPv6);
        Assert.Equal(topicAliasMaximum, options.ClientTopicAliasMaximum);
        Assert.Equal(requestResponseInfo, options.RequestResponseInformation);
        Assert.Equal(requestProblemInfo, options.RequestProblemInformation);
        Assert.NotNull(options.UserProperties);
        Assert.NotNull(options.LastWillAndTestament);
        Assert.Equal(2, options.UserProperties.Count);
        Assert.Equal("LWT message", options.LastWillAndTestament.PayloadAsString);
        Assert.Single(options.ClientCertificates);
    }

    [Fact]
    public void WithClientCertificate_NonExistentFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var builder = new HiveMQClientOptionsBuilder();
        var nonExistentFilePath = "nonexistent-file.pem";

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => builder.WithClientCertificate(nonExistentFilePath));
    }

    [Fact]
    public void WithClientCertificate_NonExistentDirectory_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var builder = new HiveMQClientOptionsBuilder();
        var nonExistentDirectoryPath = "/this/nonexistent/file.pem";

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => builder.WithClientCertificate(nonExistentDirectoryPath));
    }
}
