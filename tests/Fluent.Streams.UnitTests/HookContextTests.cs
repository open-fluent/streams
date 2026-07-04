// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams.UnitTests;

public sealed class HookContextTests
{
    [Fact]
    public void CanCreate()
    {
        var context = new HookContext<SomeEvent>()
        {
            Event = new SomeEvent(),
            CreatedAt = DateTimeOffset.UtcNow,
            StreamId = Guid.NewGuid(),
        };

        context.StreamId.Should().NotBeEmpty();
        context.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        context.Event.Should().NotBeNull();
    }

    private record SomeEvent;
}
