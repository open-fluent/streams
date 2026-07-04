// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams;

/// <summary>
/// Executes a hook of type <typeparamref name="TEvent"/> using the event payload stored for later processing.
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
    /// Executes the hook. Throw an exception to signal a transient failure - the executor will
    /// increment <c>Attempts</c> and the background service will retry later.
    /// </summary>
    /// <param name="hc">The hook execution context containing the stream identifier, creation timestamp, and event payload.</param>
    /// <param name="cancellationToken">A token used to cancel the hook execution.</param>
    /// <returns>A task-like value that completes when the hook processing finishes.</returns>
    ValueTask HandleAsync(HookContext<TEvent> hc, CancellationToken cancellationToken = default);
}
