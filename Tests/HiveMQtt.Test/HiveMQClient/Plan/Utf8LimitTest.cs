namespace HiveMQtt.Test.HiveMQClient.Plan;

using FluentAssertions;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;
using NUnit.Framework;

[TestFixture]
public class Utf8LimitTest
{
    [Test]
    public void ClientId_Should_Allow_0_To_65535_Characters()
    {
        var clientId = GenerateUtf8String(65535);
        var options = new HiveMQClientOptionsBuilder()
                        .WithClientId(clientId)
                        .Build();
        var client = new HiveMQClient(options);

        client.Should().NotBeNull();
    }

    [Test]
    public void ClientId_Should_Disallow_Exceeding_65535_Characters()
    {
        var clientId = GenerateUtf8String(65536);
        Action act = () => new HiveMQClientOptionsBuilder().WithClientId(clientId).Build();

        act.Should().Throw<ArgumentException>().WithMessage("Client Id must be between 0 and 65535 characters.");
    }

    [Test]
    public void UserPropertiesKey_And_Value_Should_Allow_0_To_65535_Characters()
    {
        var key = GenerateUtf8String(65535);
        var value = GenerateUtf8String(65535);
        var options = new HiveMQClientOptionsBuilder()
                        .WithUserProperty(key, value)
                        .Build();
        var client = new HiveMQClient(options);

        client.Should().NotBeNull();
    }

    [Test]
    public void LastWillAndTestament_ResponseTopic_Should_Allow_0_To_65535_Characters()
    {
        var responseTopic = GenerateUtf8String(65535);
        var lwt = new LastWillAndTestamentBuilder()
                .WithTopic("last/will")
                .WithPayload("last will message")
                .WithQualityOfServiceLevel(QualityOfService.AtLeastOnceDelivery)
                .WithResponseTopic(responseTopic)
                .Build();

        var options = new HiveMQClientOptionsBuilder()
                    .WithLastWillAndTestament(lwt)
                    .Build();

        var client = new HiveMQClient(options);

        client.Should().NotBeNull();
    }

    [Test]
    public void UserName_Should_Allow_0_To_65535_Characters()
    {
        var userName = GenerateUtf8String(65535);
        var options = new HiveMQClientOptionsBuilder()
                        .WithUserName(userName)
                        .Build();
        var client = new HiveMQClient(options);

        client.Should().NotBeNull();
    }

    [Test]
    public void LastWillAndTestament_ContentType_Should_Allow_0_To_65535_Characters()
    {
        var contentType = GenerateUtf8String(65535);
        var lwt = new LastWillAndTestamentBuilder()
                .WithTopic("last/will")
                .WithPayload("last will message")
                .WithQualityOfServiceLevel(QualityOfService.AtLeastOnceDelivery)
                .WithContentType(contentType)
                .Build();

        var options = new HiveMQClientOptionsBuilder()
                    .WithLastWillAndTestament(lwt)
                    .Build();

        var client = new HiveMQClient(options);

        client.Should().NotBeNull();
    }

    // [Test]
    // public void ReasonString_Should_Allow_0_To_65535_Characters()
    // {
    //     var reasonString = GenerateUtf8String(65535);
    //     var options = new HiveMQClientOptionsBuilder()
    //                   .WithReasonString(reasonString)
    //                   .Build();
    //     var client = new Client.HiveMQClient(options);

    //     client.Should().NotBeNull();
    // }

    [Test]
    public void AuthenticationMethod_Should_Allow_0_To_65535_Characters()
    {
        var authMethod = GenerateUtf8String(65535);
        var options = new HiveMQClientOptionsBuilder()
                        .WithAuthenticationMethod(authMethod)
                        .Build();
        var client = new HiveMQClient(options);

        client.Should().NotBeNull();
    }

    private static string GenerateUtf8String(int length) => new string('a', length);

}
