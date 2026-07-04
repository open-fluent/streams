// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams;

/// <summary>
/// Non-generic marker interface for all domain-event-driven hooks.
/// Allows the storage and executor layers to work with hook instances without knowing
/// <typeparamref name="TEvent"/> at compile time.
/// </summary>
public sealed class HookContext<TEvent> : IHookContext<TEvent>
    where TEvent : class
{
    /// <inheritdoc />
    public required Guid StreamId { get; init; }

    /// <inheritdoc />
    public required DateTimeOffset CreatedAt { get; init; }

    /// <inheritdoc />
    public required TEvent Event { get; init; }
}
