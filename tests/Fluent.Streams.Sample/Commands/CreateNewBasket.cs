// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

using Fluent.Streams.Sample.ValueObjects;

namespace Fluent.Streams.Sample.Commands;

public sealed record CreateNewBasket
{
    public required OrderedItem[] Items { get; init; }
}
