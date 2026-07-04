// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams.Sample.Commands;

public sealed class CreateNewCommandHandler(TimeProvider timeProvider)
{
    public ValueTask<Order> HandleAsync(CreateNew command)
    {
        var now = timeProvider.GetUtcNow();

        return new ValueTask<Order>(Order.Create(Guid.NewGuid(), now, command.Items));
    }
}
