// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams;

/// <summary>
/// Represents an immutable execution context for a command.
/// </summary>
/// <typeparam name="TCommand">
/// The reference type of the command carried by the context.
/// </typeparam>
/// <remarks>
/// A command context groups the command payload with metadata required during
/// execution. The correlation identifier should be propagated to handlers, hooks,
/// logs, telemetry, and downstream calls to connect all events produced while
/// processing the command.
/// </remarks>
public sealed class CommandContext<TCommand> : ICommandContext<TCommand>
    where TCommand : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandContext{TCommand}"/> class.
    /// </summary>
    /// <param name="command">The command instance executed within this context.</param>
    [SetsRequiredMembers]
    public CommandContext(TCommand command)
#if NET6_0_OR_GREATER
        : this(command, Guid.NewGuid(), TimeProvider.System.GetUtcNow()) { }
#else
        : this(command, Guid.NewGuid(), DateTimeOffset.UtcNow) { }
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandContext{TCommand}"/> class.
    /// </summary>
    /// <param name="command">The command instance executed within this context.</param>
    /// <param name="correlationId">The correlation identifier for this logical operation.</param>
    /// <param name="executedAt">The timestamp when command execution started.</param>
    [SetsRequiredMembers]
    public CommandContext(TCommand command, Guid correlationId, DateTimeOffset executedAt)
    {
        Command = command;
        CorrelationId = correlationId;
        ExecutedAt = executedAt;
    }

    /// <inheritdoc />
    public required Guid CorrelationId { get; init; }

    /// <inheritdoc />
    public required DateTimeOffset ExecutedAt { get; init; }

    /// <inheritdoc />
    public required TCommand Command { get; init; }
}
