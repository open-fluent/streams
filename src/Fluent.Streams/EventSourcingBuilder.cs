// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams;

/// <summary>
/// Builds the Fluent Streams command stack from source-generated registrations.
/// </summary>
/// <remarks>
/// Calling the generated <c>WithCommand&lt;THandler&gt;()</c> extension is intentionally both a runtime
/// registration and a compile-time source generator signal. Domain command and
/// handler types do not need marker interfaces or Fluent Streams base types.
/// </remarks>
public sealed class EventSourcingBuilder
{
    private readonly Dictionary<Type, ICommandRegistration> commandRegistrations = [];

    /// <summary>
    /// Builds a command dispatcher from the registered handlers.
    /// </summary>
    /// <returns>A dispatcher using the command handlers registered on this builder.</returns>
    public ICommandDispatcher Build()
    {
        return new CommandDispatcher(commandRegistrations);
    }

    /// <summary>
    /// Registers a generated command handler delegate.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="THandler">The command handler type.</typeparam>
    /// <param name="handler">The generated handler delegate.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when another handler has already been registered for <typeparamref name="TCommand" />.
    /// </exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public EventSourcingBuilder RegisterCommand<TCommand, THandler>(
        Func<THandler, TCommand, CancellationToken, ValueTask> handler
    )
        where TCommand : notnull
        where THandler : class, new()
    {
        ThrowIfCommandAlreadyRegistered<TCommand>();

        commandRegistrations.Add(
            typeof(TCommand),
            new NonResultCommandRegistration<TCommand, THandler>(handler)
        );

        return this;
    }

    /// <summary>
    /// Registers a generated command handler delegate returning a result.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <typeparam name="THandler">The command handler type.</typeparam>
    /// <param name="handler">The generated handler delegate.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when another handler has already been registered for <typeparamref name="TCommand" />.
    /// </exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public EventSourcingBuilder RegisterCommand<TCommand, TResult, THandler>(
        Func<THandler, TCommand, CancellationToken, ValueTask<TResult>> handler
    )
        where TCommand : notnull
        where THandler : class, new()
    {
        ThrowIfCommandAlreadyRegistered<TCommand>();

        commandRegistrations.Add(
            typeof(TCommand),
            new ResultCommandRegistration<TCommand, TResult, THandler>(handler)
        );

        return this;
    }

    private void ThrowIfCommandAlreadyRegistered<TCommand>()
        where TCommand : notnull
    {
        Type commandType = typeof(TCommand);

        if (commandRegistrations.ContainsKey(commandType))
        {
            throw new InvalidOperationException(
                $"A command handler is already registered for '{commandType.FullName}'."
            );
        }
    }
}
