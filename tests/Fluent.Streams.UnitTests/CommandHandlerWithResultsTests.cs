// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams.UnitTests;

public sealed class CommandHandlerWithResultsTests
{
    [Fact]
    public async Task CommandHandler_Should_Return_Union_Result()
    {
        RegisterUser command = new() { Username = "testuser", Password = "password123" };
        ICommandDispatcher dispatcher = new EventSourcingBuilder()
            .WithCommand<RegisterUserHandler>()
            .Build();

        UserRegistrationResult result = await dispatcher.SendAsync(command);

        UserRegistered registered = result switch
        {
            UserRegistered value => value,
            RegistrationFailed failure => throw new InvalidOperationException(failure.Message),
        };

        registered.Id.Should().Be(RegisterUserHandler.RegisteredUserId);
    }

    public sealed record RegisterUser
    {
        public required string Username { get; init; }

        public required string Password { get; init; }
    }

    public sealed class RegisterUserHandler
    {
        public static readonly Guid RegisteredUserId = new("6003306e-1c46-433d-813f-cfcf5cf2c529");

        public ValueTask<UserRegistrationResult> HandleAsync(
            RegisterUser command,
            CancellationToken cancellationToken = default
        )
        {
            return ValueTask.FromResult<UserRegistrationResult>(
                new UserRegistered { Id = RegisteredUserId }
            );
        }
    }

    public record UserRegistered
    {
        public required Guid Id { get; init; }
    }

    public record RegistrationFailed
    {
        public required string Message { get; init; }
    }

    public union UserRegistrationResult(UserRegistered, RegistrationFailed);
}
