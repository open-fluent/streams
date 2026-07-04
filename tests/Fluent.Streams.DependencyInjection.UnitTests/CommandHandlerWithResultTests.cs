// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams.DependencyInjection.UnitTests;

public sealed class CommandHandlerWithResultTests
{
    [Fact]
    public async Task CommandHandler_ShouldReturnResult_WhenHandlerIsCreatedByDi()
    {
        var command = new RegisterUser { Username = "testuser", Password = "password123" };

        var services = new ServiceCollection();
        services.AddFluentStreams().WithCommand<RegisterUserHandler>();

        await using var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();

        var user = await dispatcher.SendAsync(command);

        user.Should().NotBeNull();
        user.Username.Should().Be(command.Username);
    }

    public sealed record RegisterUser
    {
        public required string Username { get; init; }

        public required string Password { get; init; }
    }

    public sealed record User
    {
        public required Guid Guid { get; init; }

        public required string Username { get; init; }
    }

    public sealed class RegisterUserHandler
    {
        public ValueTask<User> HandleAsync(
            RegisterUser command,
            CancellationToken cancellationToken = default
        )
        {
            return ValueTask.FromResult(new User { Guid = Guid.NewGuid(), Username = command.Username });
        }
    }
}
