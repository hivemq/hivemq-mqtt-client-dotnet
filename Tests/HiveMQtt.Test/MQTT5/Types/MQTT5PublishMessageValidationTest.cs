/*
 * Copyright 2022-present HiveMQ and the HiveMQ Community
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace HiveMQtt.Test.MQTT5.Types;

using HiveMQtt.Client.Exceptions;
using HiveMQtt.MQTT5.Types;
using Xunit;

public class MQTT5PublishMessageValidationTest
{
    [Fact]
    public void Validate_WhenTopicIsNullAndTopicAliasIsNull_ThrowsException()
    {
        // Arrange
        var message = new MQTT5PublishMessage
        {
            Topic = null,
            TopicAlias = null,
        };

        // Act & Assert
        var exception = Assert.Throws<HiveMQttClientException>(message.Validate);
        Assert.Equal("Either Topic or TopicAlias must be specified.", exception.Message);
    }

    [Fact]
    public void Validate_WhenTopicIsNullAndTopicAliasHasValue_DoesNotThrow()
    {
        // Arrange
        var message = new MQTT5PublishMessage
        {
            Topic = null,
            TopicAlias = 1,
        };

        // Act
        var exception = Record.Exception(message.Validate);

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_WhenTopicHasValueAndTopicAliasIsNull_DoesNotThrow()
    {
        // Arrange
        var message = new MQTT5PublishMessage
        {
            Topic = "test/topic",
            TopicAlias = null,
        };

        // Act
        var exception = Record.Exception(message.Validate);

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_WhenTopicHasValueAndTopicAliasHasValue_DoesNotThrow()
    {
        // Arrange
        var message = new MQTT5PublishMessage
        {
            Topic = "test/topic",
            TopicAlias = 1,
        };

        // Act
        var exception = Record.Exception(message.Validate);

        // Assert
        Assert.Null(exception);
    }
}
