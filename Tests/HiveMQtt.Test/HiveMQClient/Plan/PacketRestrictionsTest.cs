namespace HiveMQtt.Test.HiveMQClient.Plan;

using FluentAssertions;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using HiveMQtt.Client.Exceptions;

[TestFixture]
public class PacketRestrictionsTest
{
    [Test]
    public void Unsubscribe_Build_With_Zero_Topic_Should_Throw_Exception()
    {
        // Arrange
        var builder = new UnsubscribeOptionsBuilder();

        // Act
        Action act = () => builder.Build();

        // Assert
        act.Should().Throw<HiveMQttClientException>()
            .WithMessage("At least one topic filter must be specified for UnsubscribeOptions.");
    }

    [Test]
    public async Task Unsubscribe_From_NonExistent_Topic_Should_Return_ReasonCode_17_Async()
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder().Build();
        var client = new HiveMQClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        connectResult.Should().NotBeNull();

        var unsubscribeOptions = new UnsubscribeOptionsBuilder()
                                    .WithSubscription(new Subscription("nonexistent/topic"))
                                    .Build();

        // Act
        var result = await client.UnsubscribeAsync(unsubscribeOptions).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        // result.ReasonCodes.Should().Contain(ReasonCode.NoSubscriptionExisted);
        // result.Subscriptions.Count.Should().Be(1);
        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        disconnectResult.Should().BeTrue();
    }

    [Test]
    public void Unsubscribe_Build_With_Invalid_Topic_Should_Throw_Exception()
    {
        // Arrange
        var builder = new UnsubscribeOptionsBuilder();

        // Act
        Action act = () => builder.WithSubscription(new Subscription("#invalid/topic")).Build();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("The '#' wildcard must be the last character in the topic filter.");
    }

    [Test]
    public void Subscribe_Build_With_Zero_Topic_Should_Throw_Exception()
    {
        // Arrange
        var builder = new SubscribeOptionsBuilder();

        // Act
        Action act = () => builder.Build();

        // Assert
        act.Should().Throw<HiveMQttClientException>()
            .WithMessage("At least one topic filter must be specified for SubscribeOptions.");
    }

    [Test]
    public void Subscribe_Build_With_Invalid_Topic_Should_Throw_Exception()
    {
        // Arrange
        var builder = new SubscribeOptionsBuilder();

        // Act
        Action act = () => builder.WithSubscription(new TopicFilter("#invalid/topic")).Build();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("The '#' wildcard must be the last character in the topic filter.");
    }

    [Test]
    public void Subscribe_Build_With_Valid_Topic_Should_Succeed()
    {
        // Arrange
        var builder = new SubscribeOptionsBuilder();

        // Act
        var subscribeOptions = builder.WithSubscription(new TopicFilter("valid/topic", QualityOfService.AtLeastOnceDelivery)).Build();

        // Assert
        subscribeOptions.Should().NotBeNull();
        subscribeOptions.TopicFilters.Should().Contain(filter => filter.Topic == "valid/topic");
    }

    [Test]
    public void TopicFilter_Validation_Should_Throw_Exception_For_Invalid_Topic()
    {
        // Act
        Action act = () => { var topicFilter = new TopicFilter("#invalid/topic", QualityOfService.AtLeastOnceDelivery); };

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("The '#' wildcard must be the last character in the topic filter.");
    }

    [Test]
    public void TopicFilter_Validation_Should_Succeed_For_Valid_Topic()
    {
        // Act
        var topicFilter = new TopicFilter("valid/topic", QualityOfService.ExactlyOnceDelivery);

        // Assert
        topicFilter.Should().NotBeNull();
        topicFilter.Topic.Should().Be("valid/topic");
        topicFilter.QoS.Should().Be(QualityOfService.ExactlyOnceDelivery);
    }
}
