namespace HiveMQtt.Test.HiveMQClient.Plan;

using FluentAssertions;
using HiveMQtt.Client;
using HiveMQtt.Client.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using NUnit.Framework;
using Xunit;

[TestFixture]
[Collection("Broker")]
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
    public async Task Publish_ResponseTopic_Should_Allow_0_To_65535_Characters_Async()
    {
        var responseTopic = GenerateUtf8String(65535);
        var publishMessage = new PublishMessageBuilder()
                    .WithTopic("test/publish")
                    .WithPayload("test message")
                    .WithQualityOfService(QualityOfService.AtMostOnceDelivery)
                    .WithResponseTopic(responseTopic)
                    .Build();

        var options = new HiveMQClientOptionsBuilder().Build();
        var client = new HiveMQClient(options);

        client.Should().NotBeNull();

        var publishResult = await client.PublishAsync(publishMessage).ConfigureAwait(false);

        publishResult.Should().NotBeNull();
        publishResult.ReasonCode().Should().Be((int)QoS1ReasonCode.Success);
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

    [Test]
    public async Task Publish_ContentType_Should_Allow_0_To_65535_Characters_Async()
    {
        var contentType = GenerateUtf8String(65535);
        var publishMessage = new PublishMessageBuilder()
                    .WithTopic("test/publish")
                    .WithPayload("test message")
                    .WithQualityOfService(QualityOfService.AtMostOnceDelivery)
                    .WithContentType(contentType)
                    .Build();

        var options = new HiveMQClientOptionsBuilder().Build();
        var client = new HiveMQClient(options);

        client.Should().NotBeNull();

        var publishResult = await client.PublishAsync(publishMessage).ConfigureAwait(false);

        publishResult.Should().NotBeNull();
        publishResult.ReasonCode().Should().Be((int)QoS1ReasonCode.Success);
    }

    [Test]
    public async Task Disconnect_ReasonString_Should_Allow_0_To_65535_Characters_Async()
    {
        var reasonString = GenerateUtf8String(65535);
        var disconnectOptions = new DisconnectOptionsBuilder()
                                .WithReasonString(reasonString)
                                .Build();

        var options = new HiveMQClientOptionsBuilder().Build();
        var client = new HiveMQClient(options);

        client.Should().NotBeNull();

        // Now connect, test, disconnect and test
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        connectResult.Should().NotBeNull();

        var disconnectResult = await client.DisconnectAsync(disconnectOptions).ConfigureAwait(false);
        disconnectResult.Should().BeTrue();
    }

    [Test]
    public async Task Connect_Authentication_Method_Should_Allow_0_To_65535_Characters_Async()
    {
        var authenticationMethod = GenerateUtf8String(65535);
        var options = new HiveMQClientOptionsBuilder()
                        .WithAuthenticationMethod(authenticationMethod)
                        .Build();
        var client = new HiveMQClient(options);

        client.Should().NotBeNull();

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        connectResult.Should().NotBeNull();

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        disconnectResult.Should().BeTrue();
    }

    [Test]
    public async Task Publish_Topic_Should_Allow_0_To_65535_Characters_Async()
    {
        var topic = GenerateUtf8String(65535);
        var publishMessage = new PublishMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload("test message")
                    .WithQualityOfService(QualityOfService.AtMostOnceDelivery)
                    .Build();

        var options = new HiveMQClientOptionsBuilder().Build();
        var client = new HiveMQClient(options);
        client.Should().NotBeNull();

        var publishResult = await client.PublishAsync(publishMessage).ConfigureAwait(false);

        publishResult.Should().NotBeNull();
        publishResult.ReasonCode().Should().Be((int)QoS1ReasonCode.Success);
    }

    [Test]
    public async Task TopicFilter_Should_Abide_By_Utf8_Encoded_String_Limits_Async()
    {
        var validTopicFilter = GenerateUtf8String(65535);
        var options = new HiveMQClientOptionsBuilder().Build();
        var client = new HiveMQClient(options);

        client.Should().NotBeNull();

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        connectResult.Should().NotBeNull();

        var subscribeOptions = new SubscribeOptionsBuilder()
                                .WithSubscription(new TopicFilter(validTopicFilter, QualityOfService.AtMostOnceDelivery))
                                .Build();

        var subscribeResult = await client.SubscribeAsync(subscribeOptions).ConfigureAwait(false);
        subscribeResult.Should().NotBeNull();

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        disconnectResult.Should().BeTrue();
    }

    [Test]
    public async Task TopicFilter_Should_Allow_Plus_Wildcard_Anywhere_Async()
    {
        var topicFilter = "test/+/topic";
        var options = new HiveMQClientOptionsBuilder().Build();
        var client = new HiveMQClient(options);

        client.Should().NotBeNull();

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        connectResult.Should().NotBeNull();

        var subscribeOptions = new SubscribeOptionsBuilder()
                                .WithSubscription(new TopicFilter(topicFilter, QualityOfService.AtMostOnceDelivery))
                                .Build();

        var subscribeResult = await client.SubscribeAsync(subscribeOptions).ConfigureAwait(false);
        subscribeResult.Should().NotBeNull();

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        disconnectResult.Should().BeTrue();
    }

    [Test]
    public async Task TopicFilter_Should_Allow_Hash_Wildcard_Only_At_End_Async()
    {
        var topicFilter = "test/topic/#";
        var options = new HiveMQClientOptionsBuilder().Build();
        var client = new HiveMQClient(options);

        client.Should().NotBeNull();

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        connectResult.Should().NotBeNull();

        var subscribeOptions = new SubscribeOptionsBuilder()
                                .WithSubscription(new TopicFilter(topicFilter, QualityOfService.AtMostOnceDelivery))
                                .Build();

        var subscribeResult = await client.SubscribeAsync(subscribeOptions).ConfigureAwait(false);
        subscribeResult.Should().NotBeNull();

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        disconnectResult.Should().BeTrue();
    }

    [Test]
    public async Task TopicFilter_Should_Not_Allow_Invalid_Plus_Wildcard_Async()
    {
        var topicFilter = "asd+";
        var options = new HiveMQClientOptionsBuilder().Build();
        var client = new HiveMQClient(options);

        client.Should().NotBeNull();

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        connectResult.Should().NotBeNull();

        Func<Task> act = async () =>
        {
            var subscribeOptions = new SubscribeOptionsBuilder()
                                    .WithSubscription(new TopicFilter(topicFilter, QualityOfService.AtMostOnceDelivery))
                                    .Build();

            await client.SubscribeAsync(subscribeOptions).ConfigureAwait(false);
        };

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("The '+' wildcard must stand alone and cannot be part of another string.").ConfigureAwait(true);

        topicFilter = "asd#";
        act = async () =>
        {
            var subscribeOptions = new SubscribeOptionsBuilder()
                                    .WithSubscription(new TopicFilter(topicFilter, QualityOfService.AtMostOnceDelivery))
                                    .Build();

            await client.SubscribeAsync(subscribeOptions).ConfigureAwait(false);
        };

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("The '#' wildcard must be preceded by a topic level separator or be the only character.").ConfigureAwait(true);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        disconnectResult.Should().BeTrue();
    }

#pragma warning disable IDE0090 // Use 'new(...)'
    private static string GenerateUtf8String(int length) => new string('a', length);
#pragma warning restore IDE0090 // Use 'new(...)'
}
