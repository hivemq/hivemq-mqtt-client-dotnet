/*
 * Copyright 2024-present HiveMQ and the HiveMQ Community
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
namespace HiveMQtt.Test.Internal;

using System;
using System.Linq;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Internal;
using Xunit;

public class ValidatorTest
{
    [Fact]
    public void ValidateClientId_WithValidClientId_DoesNotThrow()
    {
        // Valid client IDs with various alphanumeric combinations
        var validClientIds = new[]
        {
            "client1",
            "Client123",
            "CLIENT456",
            "a",
            "1",
            "A",
            "abc123XYZ",
            "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ",
            new string('a', 1000),
            new string('A', 65535), // Maximum length
        };

        foreach (var clientId in validClientIds)
        {
            var exception = Record.Exception(() => Validator.ValidateClientId(clientId));
            Assert.Null(exception);
        }
    }

    [Fact]
    public void ValidateClientId_WithNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Validator.ValidateClientId(null!));
    }

    [Fact]
    public void ValidateClientId_WithEmptyString_ThrowsHiveMQttClientException()
    {
        var exception = Assert.Throws<HiveMQttClientException>(() => Validator.ValidateClientId(string.Empty));
        Assert.Equal("Client identifier must not be empty.", exception.Message);
    }

    [Fact]
    public void ValidateClientId_WithLengthGreaterThan65535Bytes_ThrowsHiveMQttClientException()
    {
        var longClientId = new string('a', 65536);
        var exception = Assert.Throws<HiveMQttClientException>(() => Validator.ValidateClientId(longClientId));
        Assert.Equal("Client identifier must not be longer than 65535 bytes (UTF-8 encoded).", exception.Message);
    }

    [Fact]
    public void ValidateClientId_WithInvalidCharacters_ThrowsHiveMQttClientException()
    {
        var invalidClientIds = new[]
        {
            "client-id",      // Hyphen
            "client_id",      // Underscore
            "client.id",      // Period
            "client id",      // Space
            "client@id",      // At symbol
            "client#id",       // Hash
            "client$id",       // Dollar
            "client%id",       // Percent
            "client&id",       // Ampersand
            "client(id",      // Parenthesis
            "client)id",      // Parenthesis
            "client[id",      // Bracket
            "client]id",      // Bracket
            "client{id",      // Brace
            "client}id",      // Brace
            "client|id",      // Pipe
            "client\\id",      // Backslash
            "client/id",      // Forward slash
            "client:id",      // Colon
            "client;id",      // Semicolon
            "client\"id",     // Quote
            "'client'",       // Single quote
            "client~id",      // Tilde
            "client!id",      // Exclamation
            "client?id",      // Question mark
            "client<id",      // Less than
            "client>id",      // Greater than
            "client=id",      // Equals
            "client,id",      // Comma
            "client\nid",     // Newline
            "client\tid",     // Tab
            "client\rid",     // Carriage return
            "client\0id",     // Null character
            "日本語",         // Non-ASCII characters
            "клиент",         // Cyrillic
            "cliënt",         // Accented character
        };

        foreach (var clientId in invalidClientIds)
        {
            var exception = Assert.Throws<HiveMQttClientException>(() => Validator.ValidateClientId(clientId));
            Assert.Equal("MQTT Client IDs can only contain: 0-9, a-z, A-Z", exception.Message);
        }
    }

    [Fact]
    public void ValidateClientId_WithExactly65535Bytes_DoesNotThrow()
    {
        var maxLengthClientId = new string('a', 65535);
        var exception = Record.Exception(() => Validator.ValidateClientId(maxLengthClientId));
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateClientId_WithUtf8ByteCounting_ValidatesByteCount()
    {
        // Note: Client IDs are restricted to alphanumeric characters only (0-9, a-z, A-Z),
        // which are all 1-byte UTF-8 characters. This test verifies that byte counting
        // is used instead of character counting (even though for ASCII they're the same).
        // With ASCII characters, 65535 characters = 65535 bytes, so this passes.
        var maxLengthClientId = new string('a', 65535);
        var exception = Record.Exception(() => Validator.ValidateClientId(maxLengthClientId));
        Assert.Null(exception);

        // 65536 characters = 65536 bytes, should fail on byte count
        var tooLongClientId = new string('a', 65536);
        var exception2 = Assert.Throws<HiveMQttClientException>(() => Validator.ValidateClientId(tooLongClientId));
        Assert.Equal("Client identifier must not be longer than 65535 bytes (UTF-8 encoded).", exception2.Message);
    }

    [Fact]
    public void ValidateClientId_WithAllValidCharacters_DoesNotThrow()
    {
        var allValidChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var exception = Record.Exception(() => Validator.ValidateClientId(allValidChars));
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateTopicName_WithValidTopicName_DoesNotThrow()
    {
        var validTopics = new[]
        {
            "topic",
            "a/b/c",
            "sport/tennis/player1",
            "sport/tennis/player1/ranking",
            "sport/tennis/player1/score/wimbledon",
            "$SYS/monitor/Clients",
            "$SYS/broker/clients/total",
            "topic123",
            "TOPIC_NAME",
            "topic-name",
            "topic_name",
            "topic.name",
            "topic@name",
            "topic$name",
            "topic%name",
            "topic&name",
            "topic~name",
            "topic!name",
            "topic*name",
            "topic=name",
            "topic,name",
            "topic:name",
            "topic;name",
            "topic\"name",
            "'topic'",
            "topic<name",
            "topic>name",
            "topic|name",
            "topic\\name",
            "topic/name",
            "topic?name",
            new string('a', 65535), // Maximum length
            "a", // Single character
        };

        foreach (var topic in validTopics)
        {
            var exception = Record.Exception(() => Validator.ValidateTopicName(topic));
            Assert.Null(exception);
        }
    }

    [Fact]
    public void ValidateTopicName_WithNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Validator.ValidateTopicName(null!));
    }

    [Fact]
    public void ValidateTopicName_WithEmptyString_ThrowsHiveMQttClientException()
    {
        var exception = Assert.Throws<HiveMQttClientException>(() => Validator.ValidateTopicName(string.Empty));
        Assert.Equal("A topic string must not be empty.", exception.Message);
    }

    [Fact]
    public void ValidateTopicName_WithLengthGreaterThan65535Bytes_ThrowsHiveMQttClientException()
    {
        var longTopic = new string('a', 65536);
        var exception = Assert.Throws<HiveMQttClientException>(() => Validator.ValidateTopicName(longTopic));
        Assert.Equal("A topic string must not be longer than 65535 bytes (UTF-8 encoded).", exception.Message);
    }

    [Fact]
    public void ValidateTopicName_WithPlusWildcard_ThrowsHiveMQttClientException()
    {
        var invalidTopics = new[]
        {
            "sport+",
            "+sport",
            "sport+/tennis",
            "sport/tennis+",
            "sport/+/tennis",
            "+",
            "sport/+/player1",
        };

        foreach (var topic in invalidTopics)
        {
            var exception = Assert.Throws<HiveMQttClientException>(() => Validator.ValidateTopicName(topic));
            Assert.Equal("A topic name must not contain any wildcard characters.", exception.Message);
        }
    }

    [Fact]
    public void ValidateTopicName_WithHashWildcard_ThrowsHiveMQttClientException()
    {
        var invalidTopics = new[]
        {
            "sport#",
            "#sport",
            "sport#/tennis",
            "sport/tennis#",
            "sport/#/tennis",
            "#",
            "sport/tennis/#",
        };

        foreach (var topic in invalidTopics)
        {
            var exception = Assert.Throws<HiveMQttClientException>(() => Validator.ValidateTopicName(topic));
            Assert.Equal("A topic name must not contain any wildcard characters.", exception.Message);
        }
    }

    [Fact]
    public void ValidateTopicName_WithNullCharacter_ThrowsHiveMQttClientException()
    {
        var topicsWithNull = new[]
        {
            "topic\0name",
            "\0topic",
            "topic\0",
            "topic/\0/name",
        };

        foreach (var topic in topicsWithNull)
        {
            var exception = Assert.Throws<HiveMQttClientException>(() => Validator.ValidateTopicName(topic));
            Assert.Equal("A topic name cannot contain any null characters.", exception.Message);
        }
    }

    [Fact]
    public void ValidateTopicName_WithExactly65535Bytes_DoesNotThrow()
    {
        var maxLengthTopic = new string('a', 65535);
        var exception = Record.Exception(() => Validator.ValidateTopicName(maxLengthTopic));
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateTopicName_WithMultiByteUtf8Characters_ValidatesByteCount()
    {
        // Test with 4-byte UTF-8 characters (emoji)
        // 16383 emoji = 65532 bytes, should pass
        var emoji = "🌍"; // emoji is 4 bytes in UTF-8
        var emojiTopic = string.Concat(Enumerable.Repeat(emoji, 16383));
        var exception = Record.Exception(() => Validator.ValidateTopicName(emojiTopic));
        Assert.Null(exception);

        // 16384 emoji = 65536 bytes, should fail
        var tooLongEmoji = string.Concat(Enumerable.Repeat(emoji, 16384));
        var exception2 = Assert.Throws<HiveMQttClientException>(() => Validator.ValidateTopicName(tooLongEmoji));
        Assert.Equal("A topic string must not be longer than 65535 bytes (UTF-8 encoded).", exception2.Message);

        // Test with 2-byte UTF-8 characters (Cyrillic)
        // 32767 cyrillic characters = 65534 bytes, should pass
        var cyrillicTopic = new string('А', 32767); // 'А' is 2 bytes in UTF-8
        var exception3 = Record.Exception(() => Validator.ValidateTopicName(cyrillicTopic));
        Assert.Null(exception);

        // 32768 cyrillic characters = 65536 bytes, should fail
        var tooLongCyrillic = new string('А', 32768);
        var exception4 = Assert.Throws<HiveMQttClientException>(() => Validator.ValidateTopicName(tooLongCyrillic));
        Assert.Equal("A topic string must not be longer than 65535 bytes (UTF-8 encoded).", exception4.Message);
    }

    [Fact]
    public void ValidateTopicFilter_WithValidTopicFilter_DoesNotThrow()
    {
        var validFilters = new[]
        {
            "topic",
            "a/b/c",
            "sport/tennis/player1",
            "sport/tennis/player1/ranking",
            "sport/tennis/+",              // Single-level wildcard
            "+/tennis/player1",            // Single-level wildcard at start
            "sport/+/player1",             // Single-level wildcard in middle
            "sport/tennis/+",               // Single-level wildcard at end
            "+",                            // Single-level wildcard alone
            "sport/#",                      // Multi-level wildcard
            "#",                            // Multi-level wildcard alone
            "sport/tennis/player1/#",       // Multi-level wildcard at end
            "$SYS/monitor/+",               // Single-level wildcard with $SYS
            "$SYS/#",                       // Multi-level wildcard with $SYS
            "topic123",
            "TOPIC_NAME",
            "topic-name",
            "topic_name",
            "topic.name",
            new string('a', 65535),        // Maximum length
            "a",                            // Single character
        };

        foreach (var filter in validFilters)
        {
            var exception = Record.Exception(() => Validator.ValidateTopicFilter(filter));
            Assert.Null(exception);
        }
    }

    [Fact]
    public void ValidateTopicFilter_WithNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Validator.ValidateTopicFilter(null!));
    }

    [Fact]
    public void ValidateTopicFilter_WithEmptyString_ThrowsHiveMQttClientException()
    {
        var exception = Assert.Throws<HiveMQttClientException>(() => Validator.ValidateTopicFilter(string.Empty));
        Assert.Equal("A topic string must not be empty.", exception.Message);
    }

    [Fact]
    public void ValidateTopicFilter_WithLengthGreaterThan65535Bytes_ThrowsHiveMQttClientException()
    {
        var longFilter = new string('a', 65536);
        var exception = Assert.Throws<HiveMQttClientException>(() => Validator.ValidateTopicFilter(longFilter));
        Assert.Equal("A topic string must not be longer than 65535 bytes (UTF-8 encoded).", exception.Message);
    }

    [Fact]
    public void ValidateTopicFilter_WithNullCharacter_ThrowsHiveMQttClientException()
    {
        var filtersWithNull = new[]
        {
            "topic\0name",
            "\0topic",
            "topic\0",
            "topic/\0/name",
        };

        foreach (var filter in filtersWithNull)
        {
            var exception = Assert.Throws<HiveMQttClientException>(() => Validator.ValidateTopicFilter(filter));
            Assert.Equal("A topic name cannot contain any null characters.", exception.Message);
        }
    }

    [Fact]
    public void ValidateTopicFilter_WithHashNotAtEnd_ThrowsArgumentException()
    {
        var invalidFilters = new[]
        {
            "#sport",
            "sport#/tennis",
            "sport/#/tennis",
            "sport/tennis#/ranking",
        };

        foreach (var filter in invalidFilters)
        {
            var exception = Assert.Throws<ArgumentException>(() => Validator.ValidateTopicFilter(filter));
            Assert.Equal("The '#' wildcard must be the last character in the topic filter.", exception.Message);
        }
    }

    [Fact]
    public void ValidateTopicFilter_WithHashNotPrecededBySeparator_ThrowsArgumentException()
    {
        var invalidFilters = new[]
        {
            "sport#",
            "a#",
            "topic123#",
        };

        foreach (var filter in invalidFilters)
        {
            var exception = Assert.Throws<ArgumentException>(() => Validator.ValidateTopicFilter(filter));
            Assert.Equal("The '#' wildcard must be preceded by a topic level separator or be the only character.", exception.Message);
        }
    }

    [Fact]
    public void ValidateTopicFilter_WithHashAlone_DoesNotThrow()
    {
        var exception = Record.Exception(() => Validator.ValidateTopicFilter("#"));
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateTopicFilter_WithHashAfterSeparator_DoesNotThrow()
    {
        var validFilters = new[]
        {
            "sport/#",
            "a/b/#",
            "sport/tennis/player1/#",
        };

        foreach (var filter in validFilters)
        {
            var exception = Record.Exception(() => Validator.ValidateTopicFilter(filter));
            Assert.Null(exception);
        }
    }

    [Fact]
    public void ValidateTopicFilter_WithPlusInSegment_ThrowsArgumentException()
    {
        var invalidFilters = new[]
        {
            "sport+",
            "+sport",
            "sport+tennis",
            "sport/tennis+player",
            "sport/tennis+",
            "sport+/tennis",
            "sport/tennis+player1",
            "sport/++",
            "sport/+tennis",
            "sport/tennis+player",
        };

        foreach (var filter in invalidFilters)
        {
            var exception = Assert.Throws<ArgumentException>(() => Validator.ValidateTopicFilter(filter));
            Assert.Equal("The '+' wildcard must stand alone and cannot be part of another string.", exception.Message);
        }
    }

    [Fact]
    public void ValidateTopicFilter_WithPlusStandalone_DoesNotThrow()
    {
        var validFilters = new[]
        {
            "+",
            "sport/+",
            "+/tennis",
            "sport/+/player1",
            "sport/tennis/+",
            "$SYS/+",
        };

        foreach (var filter in validFilters)
        {
            var exception = Record.Exception(() => Validator.ValidateTopicFilter(filter));
            Assert.Null(exception);
        }
    }

    [Fact]
    public void ValidateTopicFilter_WithExactly65535Bytes_DoesNotThrow()
    {
        var maxLengthFilter = new string('a', 65535);
        var exception = Record.Exception(() => Validator.ValidateTopicFilter(maxLengthFilter));
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateTopicFilter_WithMultiByteUtf8Characters_ValidatesByteCount()
    {
        // Test with 4-byte UTF-8 characters (emoji)
        // 16383 emoji = 65532 bytes, should pass
        var emoji = "🌍"; // emoji is 4 bytes in UTF-8
        var emojiFilter = string.Concat(Enumerable.Repeat(emoji, 16383));
        var exception = Record.Exception(() => Validator.ValidateTopicFilter(emojiFilter));
        Assert.Null(exception);

        // 16384 emoji = 65536 bytes, should fail
        var tooLongEmoji = string.Concat(Enumerable.Repeat(emoji, 16384));
        var exception2 = Assert.Throws<HiveMQttClientException>(() => Validator.ValidateTopicFilter(tooLongEmoji));
        Assert.Equal("A topic string must not be longer than 65535 bytes (UTF-8 encoded).", exception2.Message);

        // Test with 2-byte UTF-8 characters (Cyrillic) in a valid filter pattern
        // 32767 cyrillic characters = 65534 bytes, should pass
        var cyrillicFilter = new string('А', 32767); // 'А' is 2 bytes in UTF-8
        var exception3 = Record.Exception(() => Validator.ValidateTopicFilter(cyrillicFilter));
        Assert.Null(exception);

        // 32768 cyrillic characters = 65536 bytes, should fail
        var tooLongCyrillic = new string('А', 32768);
        var exception4 = Assert.Throws<HiveMQttClientException>(() => Validator.ValidateTopicFilter(tooLongCyrillic));
        Assert.Equal("A topic string must not be longer than 65535 bytes (UTF-8 encoded).", exception4.Message);
    }

    [Fact]
    public void ValidateTopicFilter_WithComplexValidPatterns_DoesNotThrow()
    {
        var complexValidFilters = new[]
        {
            "sport/tennis/+",
            "sport/+/player1",
            "+/tennis/player1",
            "sport/tennis/player1/#",
            "sport/#",
            "$SYS/monitor/+",
            "$SYS/#",
            "sport/+/+/ranking",
            "+/+",
            "a/b/c/d/e/f/g/h/i/j/+",
        };

        foreach (var filter in complexValidFilters)
        {
            var exception = Record.Exception(() => Validator.ValidateTopicFilter(filter));
            Assert.Null(exception);
        }
    }
}
