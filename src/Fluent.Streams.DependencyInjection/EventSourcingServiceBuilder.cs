// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams.DependencyInjection;

/// <summary>
/// Configures Fluent.Streams command handlers in an <see cref="IServiceCollection" />.
/// </summary>
/// <remarks>
/// This builder is returned from <see cref="ServiceCollectionExtensions.AddFluentStreams(IServiceCollection)" />.
/// Consumers normally use the source-generated <c>WithCommand&lt;THandler&gt;</c> extension methods rather than
/// calling <c>RegisterCommand</c> directly. Each generated registration stores the command shape and lets the
/// DI container create the handler at dispatch time, including any scoped or transient dependencies.
/// </remarks>
/// <example>
/// <code><![CDATA[
/// services.AddFluentStreams()
///     .WithCommand<RegisterUserHandler>()
///     .WithCommand<ArchiveUserHandler>();
/// ]]></code>
/// </example>
/// <param name="services">The service collection receiving generated command registrations.</param>
public sealed class EventSourcingServiceBuilder(IServiceCollection services)
{
    /// <summary>
    /// Registers a generated command handler delegate that does not return a result.
    /// </summary>
    /// <typeparam name="TCommand">The command type handled by <typeparamref name="THandler" />.</typeparam>
    /// <typeparam name="THandler">The handler type resolved from the service provider.</typeparam>
    /// <param name="handler">The generated handler delegate.</param>
    /// <returns>The current builder.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public EventSourcingServiceBuilder RegisterCommand<TCommand, THandler>(
        Func<THandler, TCommand, CancellationToken, ValueTask> handler
    )
        where TCommand : notnull
        where THandler : class
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        services.TryAddTransient<THandler>();
        services.AddSingleton<IServiceCommandRegistration>(
            new NonResultServiceCommandRegistration<TCommand, THandler>(handler)
        );

        return this;
    }

    /// <summary>
    /// Registers a generated command handler delegate that returns a result.
    /// </summary>
    /// <typeparam name="TCommand">The command type handled by <typeparamref name="THandler" />.</typeparam>
    /// <typeparam name="TResult">The handler result type.</typeparam>
    /// <typeparam name="THandler">The handler type resolved from the service provider.</typeparam>
    /// <param name="handler">The generated handler delegate.</param>
    /// <returns>The current builder.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public EventSourcingServiceBuilder RegisterCommand<TCommand, TResult, THandler>(
        Func<THandler, TCommand, CancellationToken, ValueTask<TResult>> handler
    )
        where TCommand : notnull
        where THandler : class
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        services.TryAddTransient<THandler>();
        services.AddSingleton<IServiceCommandRegistration>(
            new ResultServiceCommandRegistration<TCommand, TResult, THandler>(handler)
        );

        return this;
    }
}
