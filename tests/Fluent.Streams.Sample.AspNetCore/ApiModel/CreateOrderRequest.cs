// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

using Fluent.Streams.Sample.ValueObjects;

namespace Fluent.Streams.Sample.AspNetCore.ApiModel;

/// <summary>
/// Represents the HTTP request used by the ASP.NET Core sample to create a new order through the command bus.
/// </summary>
public sealed record CreateOrderRequest
{
    /// <summary>
    /// Gets the ordered items that should be passed to the sample domain command.
    /// </summary>
    public required OrderedItem[] Items { get; init; }
}
