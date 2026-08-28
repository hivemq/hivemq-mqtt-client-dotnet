// Copyright 2026-present HiveMQ and the HiveMQ Community
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace HiveMQtt.Sparkplug.Test.HostApplication;

using System.Text;
using FluentAssertions;
using HiveMQtt.Sparkplug.HostApplication;
using NUnit.Framework;

[TestFixture]
public class SparkplugStatePayloadTest
{
    [Test]
    public void ToUtf8Bytes_Uses_Sparkplug_CamelCase_Wire_Format()
    {
        var online = new SparkplugStatePayload(online: true, timestamp: 1710000000000);
        Encoding.UTF8.GetString(online.ToUtf8Bytes())
            .Should().Be("{\"online\":true,\"timestamp\":1710000000000}");

        var offline = SparkplugStatePayload.CreateOffline(timestampMs: 0);
        Encoding.UTF8.GetString(offline.ToUtf8Bytes())
            .Should().Be("{\"online\":false,\"timestamp\":0}");
    }

    [Test]
    public void TryDecode_Reads_Spec_CamelCase_Json()
    {
        var bytes = Encoding.UTF8.GetBytes("{\"online\":true,\"timestamp\":1710000000000}");

        SparkplugStatePayload.TryDecode(bytes, out var payload).Should().BeTrue();
        payload!.Online.Should().BeTrue();
        payload.Timestamp.Should().Be(1710000000000);
    }
}
