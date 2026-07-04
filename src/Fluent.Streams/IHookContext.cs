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
/// <typeparam name="TEvent">The domain event type that triggered the hook.</typeparam>
public interface IHookContext<out TEvent>
    where TEvent : class
{
    /// <summary>Gets the identifier of the aggregate stream that produced the triggering event.</summary>
    Guid StreamId { get; }

    /// <summary>Gets the timestamp when this hook was created by the dispatcher.</summary>
    DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets the committed domain event that triggered this hook.
    /// </summary>
    TEvent Event { get; }
}
