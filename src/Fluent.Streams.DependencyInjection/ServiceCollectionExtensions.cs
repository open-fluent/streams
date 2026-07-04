// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams.DependencyInjection;

/// <summary>
/// Provides registration helpers for Fluent.Streams services.
/// </summary>
/// <remarks>
/// Use this extension in the composition root to register the DI-backed command bus and then call the
/// source-generated <c>WithCommand&lt;THandler&gt;</c> methods for every handler that should be available
/// through <see cref="ICommandDispatcher" />.
/// </remarks>
/// <example>
/// <code><![CDATA[
/// ServiceCollection services = [];
/// services.AddFluentStreams()
///     .WithCommand<RegisterUserHandler>();
///
/// await using ServiceProvider provider = services.BuildServiceProvider();
/// ICommandDispatcher dispatcher = provider.GetRequiredService<ICommandDispatcher>();
/// await dispatcher.SendAsync(new RegisterUser { Username = "alice" });
/// ]]></code>
/// </example>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Fluent.Streams command dispatching backed by Microsoft.Extensions.DependencyInjection.
    /// </summary>
    /// <param name="services">The service collection to add Fluent.Streams services to.</param>
    /// <returns>A builder used by the source generator to register markerless command handlers.</returns>
    public static EventSourcingServiceBuilder AddFluentStreams(this IServiceCollection services)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.TryAddScoped<ICommandDispatcher, ServiceProviderCommandDispatcher>();

        return new EventSourcingServiceBuilder(services);
    }
}
