// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams;

/// <summary>
/// Provides contextual data for a hook triggered by a committed domain event.
/// </summary>
/// <typeparam name="TEvent">The domain event type that triggered the hook.</typeparam>
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
