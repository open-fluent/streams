// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams;

/// <summary>
/// Handles a single command.
/// </summary>
/// <remarks>
/// Each command type is expected to have a single registered handler. Implementations
/// are responsible for creating a command execution context, propagating the
/// correlation identifier, and invoking the handler registered for the specified
/// command type.
/// </remarks>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
#pragma warning disable SA1649
public interface ICommandHandler<TCommand> : ICommandHandler
#pragma warning restore SA1649
    where TCommand : notnull
{
    /// <summary>
    /// Handles the specified command asynchronously.
    /// </summary>
    /// <param name="context">The command context to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    ValueTask HandleAsync(CommandContext<TCommand> context, CancellationToken cancellationToken = default);
}
