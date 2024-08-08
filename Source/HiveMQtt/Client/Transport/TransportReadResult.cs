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
namespace HiveMQtt.Client.Transport;

using System.Buffers;

public class TransportReadResult
{
    public bool Failed { get; set; }

    public ReadOnlySequence<byte> Buffer { get; set; }

    public TransportReadResult(ReadOnlySequence<byte> buffer)
    {
        this.Failed = false;
        this.Buffer = buffer;
    }

    public TransportReadResult(bool failed)
    {
        this.Failed = failed;
        this.Buffer = default;
    }
}
