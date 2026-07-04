// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams.UnitTests;

public sealed class CommandHandlerWithResultsTests
{
    [Fact]
    public async Task CommandHandler_ShouldReturnUnionResult()
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

    [Fact]
    public async Task CommandHandler_ShouldReturnAwaitedUnionResult()
    {
        RegisterUser command = new() { Username = "testuser", Password = "password123" };
        ICommandDispatcher dispatcher = new EventSourcingBuilder()
            .WithCommand<RegisterUserHandler>()
            .Build();

        var result = await dispatcher.SendAsync(command) switch
        {
            UserRegistered value => value,
            RegistrationFailed failure => throw new InvalidOperationException(failure.Message),
        };

        result.Id.Should().Be(RegisterUserHandler.RegisteredUserId);
    }

    [Fact]
    public void EventSourcingBuilder_Should_Throw_When_Command_Handler_Is_Registered_Twice()
    {
        EventSourcingBuilder builder = new EventSourcingBuilder().WithCommand<RegisterUserHandler>();

        Action action = () => builder.WithCommand<DuplicateRegisterUserHandler>();

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*RegisterUser*");
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

    public sealed class DuplicateRegisterUserHandler
    {
        public ValueTask<UserRegistrationResult> HandleAsync(
            RegisterUser command,
            CancellationToken cancellationToken = default
        )
        {
            return ValueTask.FromResult<UserRegistrationResult>(
                new RegistrationFailed { Message = $"Duplicate handler for '{command.Username}'." }
            );
        }
    }

    public record UserRegistered
    {
        public required Guid Id { get; init; }
    }

    public record UserAlreadyRegistered
    {
        public required Guid Id { get; init; }
    }

    public record RegistrationFailed
    {
        public required string Message { get; init; }
    }

    public union UserRegistrationResult(UserRegistered, RegistrationFailed);
}
