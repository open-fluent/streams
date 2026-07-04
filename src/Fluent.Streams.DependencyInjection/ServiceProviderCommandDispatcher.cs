// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams.DependencyInjection;

/// <summary>
/// Dispatches markerless command records to handlers resolved from an <see cref="IServiceProvider" />.
/// </summary>
/// <remarks>
/// The dispatcher is registered by <see cref="ServiceCollectionExtensions.AddFluentStreams(IServiceCollection)" />
/// and uses the generated <see cref="IServiceCommandRegistration" /> entries to find the handler for a command type.
/// Handler instances are resolved for every dispatch, so scoped dependencies are honored by the active service scope.
/// </remarks>
internal sealed class ServiceProviderCommandDispatcher(
    IServiceProvider serviceProvider,
    IEnumerable<IServiceCommandRegistration> commandRegistrations
) : ICommandDispatcher
{
    private readonly IReadOnlyDictionary<Type, IServiceCommandRegistration> _commandRegistrations =
        commandRegistrations.ToDictionary(static registration => registration.CommandType);

    /// <inheritdoc />
    public ValueTask DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : notnull
    {
        IServiceCommandRegistration<TCommand> registration = GetRequiredRegistration<
            IServiceCommandRegistration<TCommand>
        >(typeof(TCommand));

        return registration.HandleAsync(serviceProvider, command, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask<TResult> DispatchAsync<TCommand, TResult>(
        TCommand command,
        CancellationToken cancellationToken = default
    )
        where TCommand : notnull
    {
        IServiceCommandRegistration<TCommand, TResult> registration = GetRequiredRegistration<
            IServiceCommandRegistration<TCommand, TResult>
        >(typeof(TCommand));

        return registration.HandleAsync(serviceProvider, command, cancellationToken);
    }

    /// <summary>
    /// Gets the generated registration for the requested command/result shape.
    /// </summary>
    /// <typeparam name="TRegistration">The registration interface expected by the dispatch overload.</typeparam>
    /// <param name="commandType">The command type being dispatched.</param>
    /// <returns>The typed registration matching the command and result shape.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the command is not registered or was registered with a different result shape.
    /// </exception>
    private TRegistration GetRequiredRegistration<TRegistration>(Type commandType)
        where TRegistration : class, IServiceCommandRegistration
    {
        return _commandRegistrations.TryGetValue(commandType, out IServiceCommandRegistration? registration)
            ? registration as TRegistration
                ?? throw new InvalidOperationException(
                    $"Command '{commandType.FullName}' is registered with a different result shape."
                )
            : throw new InvalidOperationException(
                $"No command handler registration was found for '{commandType.FullName}'."
            );
    }
}
