// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams.DependencyInjection.UnitTests;

public sealed class CommandHandlerWithResultsTests
{
    [Fact]
    public async Task CommandHandler_Should_Return_Union_Result_When_Handler_Is_Created_By_DI()
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
    public void AddFluentStreams_Should_Throw_When_Command_Handler_Is_Registered_Twice()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new RegisteredUserIds(Guid.NewGuid()));
        services.AddSingleton<HandlerLifecycleProbe>();
        EventSourcingServiceBuilder builder = services.AddFluentStreams()
            .WithCommand<RegisterUserHandler>();

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

    public sealed record RegisteredUserIds(Guid NextId);

    public sealed class HandlerLifecycleProbe
    {
        public int CreatedHandlers { get; private set; }

        public int HandledCommands { get; private set; }

        public string? LastUsername { get; private set; }

        public void RecordHandlerCreated()
        {
            CreatedHandlers++;
        }

        public void RecordCommandHandled(string username)
        {
            HandledCommands++;
            LastUsername = username;
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
