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

using System.Threading.Tasks;
using HiveMQtt.Client.Internal;
using Xunit;

public class PacketIDManagerUnitTest
{
    [Fact]
    public async Task Allocate_Then_Free_Reuses_Freed_Id_Async()
    {
        var manager = new PacketIDManager();

        var id1 = await manager.GetAvailablePacketIDAsync().ConfigureAwait(true);
        Assert.Equal(1, id1);
        Assert.Equal(1, manager.Count);
        Assert.Equal(0, manager.FreedCount);

        await manager.MarkPacketIDAsAvailableAsync(id1).ConfigureAwait(true);
        Assert.Equal(0, manager.Count);
        Assert.Equal(1, manager.FreedCount);

        var id2 = await manager.GetAvailablePacketIDAsync().ConfigureAwait(true);
        Assert.Equal(id1, id2);
        Assert.Equal(1, manager.Count);
        Assert.Equal(0, manager.FreedCount);
    }

    [Fact]
    public async Task Free_Never_Allocated_Id_Does_Not_Grow_Queue_Async()
    {
        var manager = new PacketIDManager();

        // Simulate a broker-assigned incoming packet ID being freed into the manager
        await manager.MarkPacketIDAsAvailableAsync(42).ConfigureAwait(true);

        Assert.Equal(0, manager.Count);
        Assert.Equal(0, manager.FreedCount);
    }

    [Fact]
    public async Task Free_Out_Of_Range_Id_Is_No_Op_Async()
    {
        var manager = new PacketIDManager();
        _ = await manager.GetAvailablePacketIDAsync().ConfigureAwait(true);

        await manager.MarkPacketIDAsAvailableAsync(0).ConfigureAwait(true);
        await manager.MarkPacketIDAsAvailableAsync(-1).ConfigureAwait(true);
        await manager.MarkPacketIDAsAvailableAsync(65536).ConfigureAwait(true);

        Assert.Equal(1, manager.Count);
        Assert.Equal(0, manager.FreedCount);
    }

    [Fact]
    public async Task Double_Free_Does_Not_Grow_Queue_Async()
    {
        var manager = new PacketIDManager();

        var id = await manager.GetAvailablePacketIDAsync().ConfigureAwait(true);
        await manager.MarkPacketIDAsAvailableAsync(id).ConfigureAwait(true);
        Assert.Equal(1, manager.FreedCount);

        await manager.MarkPacketIDAsAvailableAsync(id).ConfigureAwait(true);
        Assert.Equal(0, manager.Count);
        Assert.Equal(1, manager.FreedCount);
    }

    [Fact]
    public async Task Reset_Clears_InUse_And_Freed_Queue_Async()
    {
        var manager = new PacketIDManager();

        var id = await manager.GetAvailablePacketIDAsync().ConfigureAwait(true);
        await manager.MarkPacketIDAsAvailableAsync(id).ConfigureAwait(true);
        Assert.Equal(1, manager.FreedCount);

        _ = await manager.GetAvailablePacketIDAsync().ConfigureAwait(true);
        Assert.Equal(1, manager.Count);

        manager.Reset();

        Assert.Equal(0, manager.Count);
        Assert.Equal(0, manager.FreedCount);

        var next = await manager.GetAvailablePacketIDAsync().ConfigureAwait(true);
        Assert.Equal(1, next);
    }
}
