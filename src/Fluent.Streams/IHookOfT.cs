// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams;

/// <summary>
/// Executes a hook of type <typeparamref name="TEvent"/> - the payload deserialized from a <see cref="PendingHook"/>.
/// Implement this interface to define what actually happens when the hook runs
/// (e.g. an HTTP call to an external system, sending an email, etc.).
/// </summary>
/// <typeparam name="TEvent">Event after which the hook is performed.</typeparam>
#pragma warning disable SA1649
public interface IHook<TEvent> : IHook
#pragma warning restore SA1649
    where TEvent : class
{
    /// <summary>
    /// Executes the hc. Throw an exception to signal a transient failure - the executor will
    /// increment <c>Attempts</c> and the background service will retry later.
    /// </summary>
    ValueTask HandleAsync(HookContext<TEvent> hc, CancellationToken cancellationToken = default);
}
