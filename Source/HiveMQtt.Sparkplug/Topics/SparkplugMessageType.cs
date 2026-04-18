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

namespace HiveMQtt.Sparkplug.Topics;

/// <summary>
/// Sparkplug B message types as defined in the specification.
/// </summary>
public enum SparkplugMessageType
{
    /// <summary>
    /// Node Birth Certificate - Published when an Edge Node comes online.
    /// </summary>
    NBIRTH,

    /// <summary>
    /// Node Death Certificate - Published when an Edge Node goes offline (typically via LWT).
    /// </summary>
    NDEATH,

    /// <summary>
    /// Node Data - Published by an Edge Node to report metric value changes.
    /// </summary>
    NDATA,

    /// <summary>
    /// Node Command - Published by a Host Application to send commands to an Edge Node.
    /// </summary>
    NCMD,

    /// <summary>
    /// Device Birth Certificate - Published when a Device comes online.
    /// </summary>
    DBIRTH,

    /// <summary>
    /// Device Death Certificate - Published when a Device goes offline.
    /// </summary>
    DDEATH,

    /// <summary>
    /// Device Data - Published by an Edge Node to report Device metric value changes.
    /// </summary>
    DDATA,

    /// <summary>
    /// Device Command - Published by a Host Application to send commands to a Device.
    /// </summary>
    DCMD,

    /// <summary>
    /// State message - Published by Host Applications to indicate online/offline state.
    /// </summary>
    STATE,
}
