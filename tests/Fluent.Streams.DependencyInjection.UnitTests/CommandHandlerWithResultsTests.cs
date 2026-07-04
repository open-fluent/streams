// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams.DependencyInjection.UnitTests;

public sealed class CommandHandlerWithResultsTests
{
    [Fact]
    public async Task CommandHandler_ShouldReturnUnionResult_WhenHandlerIsCreatedByDi()
    {
        var command = new RegisterUser { Username = "testuser", Password = "password123" };

        var services = new ServiceCollection();
        services.AddSingleton(new RegisteredUserIds(new Guid("6003306e-1c46-433d-813f-cfcf5cf2c529")));
        services.AddSingleton<HandlerLifecycleProbe>();
        services.AddFluentStreams().WithCommand<RegisterUserHandler>();

        await using var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();
        var probe = serviceProvider.GetRequiredService<HandlerLifecycleProbe>();

        probe.CreatedHandlers.Should().Be(0);
        probe.HandledCommands.Should().Be(0);

        var result = await dispatcher.SendAsync(command);

        var registered = result switch
        {
            UserRegistered value => value,
            RegistrationFailed failure => throw new InvalidOperationException(failure.Message),
        };

        registered.Id.Should().Be(serviceProvider.GetRequiredService<RegisteredUserIds>().NextId);
        probe.CreatedHandlers.Should().Be(1);
        probe.HandledCommands.Should().Be(1);
        probe.LastUsername.Should().Be(command.Username);
    }

    [Fact]
    public void AddFluentStreams_ShouldThrow_WhenCommandHandlerIsRegisteredTwice()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new RegisteredUserIds(Guid.NewGuid()));
        services.AddSingleton<HandlerLifecycleProbe>();
        var builder = services.AddFluentStreams()
            .WithCommand<RegisterUserHandler>();

        var action = () => builder.WithCommand<DuplicateRegisterUserHandler>();

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*RegisterUser*");
    }

    [Fact]
    public async Task CommandHandler_ShouldReturnResult_WhenDIHandlerDoesNotAcceptCancellationToken()
    {
        var command = new CreateUserWithoutCancellation() { Username = "testuser" };

        var services = new ServiceCollection();
        services.AddSingleton(new RegisteredUserIds(new Guid("9502a599-6e0f-44c4-a0b1-3a6bce9ba99f")));
        services.AddSingleton<HandlerLifecycleProbe>();
        services.AddFluentStreams().WithCommand<CreateUserWithoutCancellationHandler>();

        await using var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();
        var probe = serviceProvider.GetRequiredService<HandlerLifecycleProbe>();

        var result = await dispatcher.SendAsync(command);

        var registered = result switch
        {
            UserRegistered value => value,
            RegistrationFailed failure => throw new InvalidOperationException(failure.Message),
        };

        registered.Id.Should().Be(serviceProvider.GetRequiredService<RegisteredUserIds>().NextId);
        probe.CreatedHandlers.Should().Be(1);
        probe.HandledCommands.Should().Be(1);
        probe.LastUsername.Should().Be(command.Username);
    }

    [Fact]
    public async Task CommandHandler_ShouldReturnResult_WhenDIHandlerRequiresCancellationToken()
    {
        var command = new CreateUserWithRequiredCancellation { Username = "testuser" };
        using var cancellationTokenSource = new CancellationTokenSource();

        var services = new ServiceCollection();
        services.AddSingleton(new RegisteredUserIds(new Guid("4ed1b56f-5c33-4b87-9c13-0e88df0d32cc")));
        services.AddSingleton<HandlerLifecycleProbe>();
        services.AddFluentStreams().WithCommand<CreateUserWithRequiredCancellationHandler>();

        await using var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();
        var probe = serviceProvider.GetRequiredService<HandlerLifecycleProbe>();

        var result = await dispatcher.SendAsync(command, cancellationTokenSource.Token);

        var registered = result switch
        {
            UserRegistered value => value,
            RegistrationFailed failure => throw new InvalidOperationException(failure.Message),
        };

        registered.Id.Should().Be(serviceProvider.GetRequiredService<RegisteredUserIds>().NextId);
        probe.CreatedHandlers.Should().Be(1);
        probe.HandledCommands.Should().Be(1);
        probe.LastUsername.Should().Be(command.Username);
        probe.LastTokenCanBeCanceled.Should().BeTrue();
    }

    [Fact]
    public async Task CommandHandler_ShouldReturnResult_WhenDIHandlerReturnsTask()
    {
        var command = new CreateUserWithTask { Username = "testuser" };

        var services = new ServiceCollection();
        services.AddSingleton(new RegisteredUserIds(new Guid("b0e53950-14f8-41dc-a6fd-455fbc5ad1a8")));
        services.AddSingleton<HandlerLifecycleProbe>();
        services.AddFluentStreams().WithCommand<CreateUserWithTaskHandler>();

        await using var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();
        var probe = serviceProvider.GetRequiredService<HandlerLifecycleProbe>();

        var result = await dispatcher.SendAsync(command);

        var registered = result switch
        {
            UserRegistered value => value,
            RegistrationFailed failure => throw new InvalidOperationException(failure.Message),
        };

        registered.Id.Should().Be(serviceProvider.GetRequiredService<RegisteredUserIds>().NextId);
        probe.CreatedHandlers.Should().Be(1);
        probe.HandledCommands.Should().Be(1);
        probe.LastUsername.Should().Be(command.Username);
    }

    public sealed record RegisterUser
    {
        public required string Username { get; init; }

        public required string Password { get; init; }
    }

    public sealed class RegisterUserHandler
    {
        private readonly RegisteredUserIds registeredUserIds;
        private readonly HandlerLifecycleProbe probe;

        public RegisterUserHandler(RegisteredUserIds registeredUserIds, HandlerLifecycleProbe probe)
        {
            this.registeredUserIds = registeredUserIds;
            this.probe = probe;
            probe.RecordHandlerCreated();
        }

        public ValueTask<UserRegistrationResult> HandleAsync(
            RegisterUser command,
            CancellationToken cancellationToken = default
        )
        {
            probe.RecordCommandHandled(command.Username);

            return ValueTask.FromResult<UserRegistrationResult>(
                new UserRegistered { Id = registeredUserIds.NextId }
            );
        }
    }

    public sealed class DuplicateRegisterUserHandler(RegisteredUserIds registeredUserIds)
    {
        public ValueTask<UserRegistrationResult> HandleAsync(
            RegisterUser command,
            CancellationToken cancellationToken = default
        )
        {
            return ValueTask.FromResult<UserRegistrationResult>(
                new UserRegistered { Id = registeredUserIds.NextId }
            );
        }
    }

    public sealed record CreateUserWithoutCancellation
    {
        public required string Username { get; init; }
    }

    public sealed class CreateUserWithoutCancellationHandler
    {
        private readonly RegisteredUserIds registeredUserIds;

        private readonly HandlerLifecycleProbe probe;

        public CreateUserWithoutCancellationHandler(
            RegisteredUserIds registeredUserIds,
            HandlerLifecycleProbe probe
        )
        {
            this.registeredUserIds = registeredUserIds;
            this.probe = probe;
            probe.RecordHandlerCreated();
        }

        public ValueTask<UserRegistrationResult> HandleAsync(CreateUserWithoutCancellation command)
        {
            probe.RecordCommandHandled(command.Username);

            return ValueTask.FromResult<UserRegistrationResult>(
                new UserRegistered { Id = registeredUserIds.NextId }
            );
        }
    }

    public sealed record CreateUserWithRequiredCancellation
    {
        public required string Username { get; init; }
    }

    public sealed class CreateUserWithRequiredCancellationHandler
    {
        private readonly RegisteredUserIds registeredUserIds;

        private readonly HandlerLifecycleProbe probe;

        public CreateUserWithRequiredCancellationHandler(
            RegisteredUserIds registeredUserIds,
            HandlerLifecycleProbe probe
        )
        {
            this.registeredUserIds = registeredUserIds;
            this.probe = probe;
            probe.RecordHandlerCreated();
        }

        public ValueTask<UserRegistrationResult> HandleAsync(
            CreateUserWithRequiredCancellation command,
            CancellationToken cancellationToken
        )
        {
            probe.RecordCommandHandled(command.Username, cancellationToken.CanBeCanceled);

            return ValueTask.FromResult<UserRegistrationResult>(
                new UserRegistered { Id = registeredUserIds.NextId }
            );
        }
    }

    public sealed record CreateUserWithTask
    {
        public required string Username { get; init; }
    }

    public sealed class CreateUserWithTaskHandler
    {
        private readonly RegisteredUserIds registeredUserIds;

        private readonly HandlerLifecycleProbe probe;

        public CreateUserWithTaskHandler(RegisteredUserIds registeredUserIds, HandlerLifecycleProbe probe)
        {
            this.registeredUserIds = registeredUserIds;
            this.probe = probe;
            probe.RecordHandlerCreated();
        }

        public Task<UserRegistrationResult> HandleAsync(CreateUserWithTask command)
        {
            probe.RecordCommandHandled(command.Username);

            return Task.FromResult<UserRegistrationResult>(
                new UserRegistered { Id = registeredUserIds.NextId }
            );
        }
    }

    public sealed record RegisteredUserIds(Guid NextId);

    public sealed class HandlerLifecycleProbe
    {
        public int CreatedHandlers { get; private set; }

        public int HandledCommands { get; private set; }

        public string? LastUsername { get; private set; }

        public bool LastTokenCanBeCanceled { get; private set; }

        public void RecordHandlerCreated()
        {
            CreatedHandlers++;
        }

        public void RecordCommandHandled(string username)
        {
            RecordCommandHandled(username, lastTokenCanBeCanceled: false);
        }

        public void RecordCommandHandled(string username, bool lastTokenCanBeCanceled)
        {
            HandledCommands++;
            LastUsername = username;
            LastTokenCanBeCanceled = lastTokenCanBeCanceled;
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
