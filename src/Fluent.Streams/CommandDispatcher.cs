// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams;

/// <summary>
/// Default command dispatcher backed by source-generated command registrations.
/// </summary>
/// <param name="commandRegistrations">The generated command registrations.</param>
internal sealed class CommandDispatcher(IReadOnlyDictionary<Type, ICommandRegistration> commandRegistrations)
    : ICommandDispatcher
{
    /// <inheritdoc />
    public ValueTask DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : notnull
    {
        ICommandRegistration<TCommand> registration = GetRequiredRegistration<ICommandRegistration<TCommand>>(
            typeof(TCommand)
        );
        return registration.HandleAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask<TResult> DispatchAsync<TCommand, TResult>(
        TCommand command,
        CancellationToken cancellationToken = default
    )
        where TCommand : notnull
    {
        ICommandRegistration<TCommand, TResult> registration = GetRequiredRegistration<
            ICommandRegistration<TCommand, TResult>
        >(typeof(TCommand));
        return registration.HandleAsync(command, cancellationToken);
    }

    private TRegistration GetRequiredRegistration<TRegistration>(Type commandType)
        where TRegistration : class, ICommandRegistration
    {
        return commandRegistrations.TryGetValue(commandType, out ICommandRegistration? registration)
            ? registration as TRegistration
                ?? throw new InvalidOperationException(
                    $"Command '{commandType.FullName}' is registered with a different result shape."
                )
            : throw new InvalidOperationException(
                $"No command handler registration was found for '{commandType.FullName}'."
            );
    }
}
