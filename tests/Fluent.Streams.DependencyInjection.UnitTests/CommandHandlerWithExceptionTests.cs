// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams.DependencyInjection.UnitTests;

public sealed class CommandHandlerWithExceptionTests
{
    [Fact]
    public async Task CommandHandler_ShouldThrowException_WhenHandlerIsCreatedByDi()
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

    [Fact]
    public async Task CommandHandler_ShouldDispatchCommand_WhenDIHandlerDoesNotAcceptCancellationToken()
    {
        var command = new AuditUserLogin { Username = "testuser" };

        var services = new ServiceCollection();
        services.AddSingleton<AuditProbe>();
        services.AddFluentStreams().WithCommand<AuditUserLoginHandler>();

        await using var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();
        var probe = serviceProvider.GetRequiredService<AuditProbe>();

        await dispatcher.SendAsync(command);

        probe.HandledCommands.Should().Be(1);
        probe.LastUsername.Should().Be(command.Username);
    }

    [Fact]
    public async Task CommandHandler_ShouldDispatchCommand_WhenDIHandlerRequiresCancellationToken()
    {
        var command = new AuditRequiredCancellationUserLogin { Username = "testuser" };
        using var cancellationTokenSource = new CancellationTokenSource();

        var services = new ServiceCollection();
        services.AddSingleton<AuditProbe>();
        services.AddFluentStreams().WithCommand<AuditRequiredCancellationUserLoginHandler>();

        await using var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();
        var probe = serviceProvider.GetRequiredService<AuditProbe>();

        await dispatcher.SendAsync(command, cancellationTokenSource.Token);

        probe.HandledCommands.Should().Be(1);
        probe.LastUsername.Should().Be(command.Username);
        probe.LastTokenCanBeCanceled.Should().BeTrue();
    }

    [Fact]
    public async Task CommandHandler_ShouldDispatchCommand_WhenDIHandlerReturnsTask()
    {
        var command = new AuditTaskUserLogin { Username = "testuser" };

        var services = new ServiceCollection();
        services.AddSingleton<AuditProbe>();
        services.AddFluentStreams().WithCommand<AuditTaskUserLoginHandler>();

        await using var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();
        var probe = serviceProvider.GetRequiredService<AuditProbe>();

        await dispatcher.SendAsync(command);

        probe.HandledCommands.Should().Be(1);
        probe.LastUsername.Should().Be(command.Username);
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

    public sealed record AuditUserLogin
    {
        public required string Username { get; init; }
    }

    public sealed class AuditUserLoginHandler(AuditProbe probe)
    {
        public ValueTask HandleAsync(AuditUserLogin command)
        {
            probe.Record(command.Username);
            return ValueTask.CompletedTask;
        }
    }

    public sealed record AuditRequiredCancellationUserLogin
    {
        public required string Username { get; init; }
    }

    public sealed class AuditRequiredCancellationUserLoginHandler(AuditProbe probe)
    {
        public ValueTask HandleAsync(
            AuditRequiredCancellationUserLogin command,
            CancellationToken cancellationToken
        )
        {
            probe.Record(command.Username, cancellationToken.CanBeCanceled);
            return ValueTask.CompletedTask;
        }
    }

    public sealed record AuditTaskUserLogin
    {
        public required string Username { get; init; }
    }

    public sealed class AuditTaskUserLoginHandler(AuditProbe probe)
    {
        public Task HandleAsync(AuditTaskUserLogin command)
        {
            probe.Record(command.Username);
            return Task.CompletedTask;
        }
    }

    public sealed class AuditProbe
    {
        public int HandledCommands { get; private set; }

        public string? LastUsername { get; private set; }

        public bool LastTokenCanBeCanceled { get; private set; }

        public void Record(string username)
        {
            Record(username, lastTokenCanBeCanceled: false);
        }

        public void Record(string username, bool lastTokenCanBeCanceled)
        {
            HandledCommands++;
            LastUsername = username;
            LastTokenCanBeCanceled = lastTokenCanBeCanceled;
        }
    }
}
