// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams.DependencyInjection.UnitTests;

public sealed class CommandHandlerWithExceptionTests
{
    [Fact]
    public async Task CommandHandler_Should_Throw_Exception_When_Handler_Is_Created_By_DI()
    {
        var command = new RegisterUser { Username = "testuser", Password = "password123" };

        var services = new ServiceCollection();
        services.AddSingleton(
            new RejectedUsernames(new HashSet<string>(StringComparer.Ordinal) { command.Username })
        );
        services.AddFluentStreams().WithCommand<RegisterUserHandler>();

        await using var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();

        var action = async () => await dispatcher.SendAsync(command);

        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    public sealed record RegisterUser
    {
        public required string Username { get; init; }

        public required string Password { get; init; }
    }

    public sealed class RegisterUserHandler(RejectedUsernames rejectedUsernames)
    {
        public ValueTask HandleAsync(RegisterUser command, CancellationToken cancellationToken = default)
        {
            if (rejectedUsernames.Values.Contains(command.Username))
            {
                throw new InvalidOperationException($"User '{command.Username}' cannot be registered.");
            }

            return ValueTask.CompletedTask;
        }
    }

    public sealed record RejectedUsernames(IReadOnlySet<string> Values);
}
