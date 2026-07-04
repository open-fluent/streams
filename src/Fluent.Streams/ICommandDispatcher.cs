// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams;

/// <summary>
/// Dispatches command instances to handlers discovered by the source generator.
/// </summary>
/// <remarks>
/// Commands are plain types. Their command semantics come from matching handler
/// methods, not from marker interfaces implemented by the command itself.
/// </remarks>
public interface ICommandDispatcher
{
    /// <summary>
    /// Dispatches a command handled with the native exception pattern.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to dispatch.</typeparam>
    /// <param name="command">The command instance.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task-like value representing the asynchronous dispatch operation.</returns>
    ValueTask DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : notnull;

    /// <summary>
    /// Dispatches a command handled with a native result value.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to dispatch.</typeparam>
    /// <typeparam name="TResult">The type of result returned by the handler.</typeparam>
    /// <param name="command">The command instance.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The result returned by the matching command handler.</returns>
    ValueTask<TResult> DispatchAsync<TCommand, TResult>(
        TCommand command,
        CancellationToken cancellationToken = default
    )
        where TCommand : notnull;
}
