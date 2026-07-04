// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams.DependencyInjection;

/// <summary>
/// Stores the generated delegate for a DI-backed command handler that completes without a result value.
/// </summary>
/// <typeparam name="TCommand">The command type accepted by the handler.</typeparam>
/// <typeparam name="THandler">The handler type resolved from the service provider.</typeparam>
public class NonResultServiceCommandRegistration<TCommand, THandler>(
    Func<THandler, TCommand, CancellationToken, ValueTask> handler
) : IServiceCommandRegistration<TCommand>
    where TCommand : notnull
    where THandler : class
{
    /// <inheritdoc />
    public virtual Type CommandType => typeof(TCommand);

    /// <inheritdoc />
    public virtual ValueTask HandleAsync(
        IServiceProvider serviceProvider,
        TCommand command,
        CancellationToken cancellationToken
    )
    {
        var resolvedHandler = serviceProvider.GetRequiredService<THandler>();

        return handler(resolvedHandler, command, cancellationToken);
    }
}
