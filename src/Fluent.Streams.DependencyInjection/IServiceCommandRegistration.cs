// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams.DependencyInjection;

/// <summary>
/// Describes a generated command registration stored in the service collection.
/// </summary>
public interface IServiceCommandRegistration
{
    /// <summary>
    /// Gets the command type handled by this registration.
    /// </summary>
    Type CommandType { get; }
}

/// <summary>
/// Represents a generated DI-backed registration for a command handler that does not return a result.
/// </summary>
/// <typeparam name="TCommand">The command type accepted by the handler.</typeparam>
public interface IServiceCommandRegistration<in TCommand> : IServiceCommandRegistration
    where TCommand : notnull
{
    /// <summary>
    /// Resolves the handler from <paramref name="serviceProvider" /> and dispatches <paramref name="command" />.
    /// </summary>
    /// <param name="serviceProvider">The provider used to create the handler instance.</param>
    /// <param name="command">The command instance to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task-like value representing the asynchronous dispatch operation.</returns>
    ValueTask HandleAsync(
        IServiceProvider serviceProvider,
        TCommand command,
        CancellationToken cancellationToken
    );
}

/// <summary>
/// Represents a generated DI-backed registration for a command handler that returns a result value.
/// </summary>
/// <typeparam name="TCommand">The command type accepted by the handler.</typeparam>
/// <typeparam name="TResult">The result type produced by the handler.</typeparam>
public interface IServiceCommandRegistration<in TCommand, TResult> : IServiceCommandRegistration
    where TCommand : notnull
{
    /// <summary>
    /// Resolves the handler from <paramref name="serviceProvider" /> and dispatches <paramref name="command" />.
    /// </summary>
    /// <param name="serviceProvider">The provider used to create the handler instance.</param>
    /// <param name="command">The command instance to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The result value returned by the handler.</returns>
    ValueTask<TResult> HandleAsync(
        IServiceProvider serviceProvider,
        TCommand command,
        CancellationToken cancellationToken
    );
}
