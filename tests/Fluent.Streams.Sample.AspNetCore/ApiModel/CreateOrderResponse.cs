// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

using Fluent.Streams.Sample.ValueObjects;

namespace Fluent.Streams.Sample.AspNetCore.ApiModel;

public sealed record CreateOrderResponse
{
    /// <summary>
    /// Gets the ordered items that should be passed to the sample domain command.
    /// </summary>
    public required Guid OrderId { get; init; }

    /// <summary>
    /// Gets the ordered items that should be passed to the sample domain command.
    /// </summary>
    public required OrderedItem[] Items { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}
