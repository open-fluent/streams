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
    public void EventSourcingBuilder_ShouldThrow_WhenCommandHandlerIsRegisteredTwice()
    {
        EventSourcingBuilder builder = new EventSourcingBuilder().WithCommand<RegisterUserHandler>();

        Action action = () => builder.WithCommand<DuplicateRegisterUserHandler>();

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*RegisterUser*");
    }

    [Fact]
    public async Task CommandHandler_ShouldReturnResult_WhenHandlerDoesNotAcceptCancellationToken()
    {
        var command = new CreateUserWithoutCancellation { Username = "testuser" };
        var dispatcher = new EventSourcingBuilder()
            .WithCommand<CreateUserWithoutCancellationHandler>()
            .Build();

        var result = await dispatcher.SendAsync(command);

        var registered = result switch
        {
            UserRegistered value => value,
            RegistrationFailed failure => throw new InvalidOperationException(failure.Message),
        };

        registered.Id.Should().Be(CreateUserWithoutCancellationHandler.RegisteredUserId);
    }

    [Fact]
    public async Task CommandHandler_ShouldReturnResult_WhenHandlerRequiresCancellationToken()
    {
        CreateUserWithRequiredCancellationHandler.LastTokenCanBeCanceled = false;
        var command = new CreateUserWithRequiredCancellation { Username = "testuser" };
        using var cancellationTokenSource = new CancellationTokenSource();
        var dispatcher = new EventSourcingBuilder()
            .WithCommand<CreateUserWithRequiredCancellationHandler>()
            .Build();

        var result = await dispatcher.SendAsync(command, cancellationTokenSource.Token);

        var registered = result switch
        {
            UserRegistered value => value,
            RegistrationFailed failure => throw new InvalidOperationException(failure.Message),
        };

        registered.Id.Should().Be(CreateUserWithRequiredCancellationHandler.RegisteredUserId);
        CreateUserWithRequiredCancellationHandler.LastTokenCanBeCanceled.Should().BeTrue();
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

    public sealed record CreateUserWithoutCancellation
    {
        public required string Username { get; init; }
    }

    public sealed class CreateUserWithoutCancellationHandler
    {
        public static readonly Guid RegisteredUserId = new("9502a599-6e0f-44c4-a0b1-3a6bce9ba99f");

        public ValueTask<UserRegistrationResult> HandleAsync(CreateUserWithoutCancellation command)
        {
            return ValueTask.FromResult<UserRegistrationResult>(
                new UserRegistered { Id = RegisteredUserId }
            );
        }
    }

    public sealed record CreateUserWithRequiredCancellation
    {
        public required string Username { get; init; }
    }

    public sealed class CreateUserWithRequiredCancellationHandler
    {
        public static readonly Guid RegisteredUserId = new("4ed1b56f-5c33-4b87-9c13-0e88df0d32cc");

        public static bool LastTokenCanBeCanceled { get; set; }

        public ValueTask<UserRegistrationResult> HandleAsync(
            CreateUserWithRequiredCancellation command,
            CancellationToken cancellationToken
        )
        {
            LastTokenCanBeCanceled = cancellationToken.CanBeCanceled;

            return ValueTask.FromResult<UserRegistrationResult>(
                new UserRegistered { Id = RegisteredUserId }
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
