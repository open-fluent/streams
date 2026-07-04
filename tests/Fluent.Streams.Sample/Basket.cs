// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

using Fluent.Streams.Sample.Events;
using Fluent.Streams.Sample.ValueObjects;

namespace Fluent.Streams.Sample;

public sealed class Basket
{
    public Guid BaskedId { get; private set; }

    public OrderedItem[] Items { get; private set; } = [];

    public Basket Create(Guid id, OrderedItem[] items)
    {
        Apply(new BasketCreated { BasketId = id, Items = items });

        return this;
    }

    private void Apply(object @event) { }
}
