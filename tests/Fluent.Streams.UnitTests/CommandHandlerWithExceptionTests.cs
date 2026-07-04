// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams.UnitTests;

public sealed class CommandHandlerWithExceptionTests
{
    [Fact]
    public async Task CommandHandler_ShouldThrowException()
    {
        var command = new RegisterUser { Username = "testuser", Password = "password123" };
        var dispatcher = new EventSourcingBuilder().WithCommand<RegisterUserHandler>().Build();

        var action = async () => await dispatcher.SendAsync(command);

        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*cannot be registered*");
    }

    [Fact]
    public async Task CommandHandler_ShouldDispatchCommand_WhenHandlerDoesNotAcceptCancellationToken()
    {
        AuditUserLoginHandler.HandledCommands = 0;
        var command = new AuditUserLogin { Username = "testuser" };
        var dispatcher = new EventSourcingBuilder().WithCommand<AuditUserLoginHandler>().Build();

        await dispatcher.SendAsync(command);

        AuditUserLoginHandler.HandledCommands.Should().Be(1);
    }

    [Fact]
    public async Task CommandHandler_ShouldDispatchCommand_WhenHandlerRequiresCancellationToken()
    {
        AuditRequiredCancellationUserLoginHandler.HandledCommands = 0;
        AuditRequiredCancellationUserLoginHandler.LastTokenCanBeCanceled = false;
        var command = new AuditRequiredCancellationUserLogin { Username = "testuser" };
        using var cancellationTokenSource = new CancellationTokenSource();
        var dispatcher = new EventSourcingBuilder()
            .WithCommand<AuditRequiredCancellationUserLoginHandler>()
            .Build();

        await dispatcher.SendAsync(command, cancellationTokenSource.Token);

        AuditRequiredCancellationUserLoginHandler.HandledCommands.Should().Be(1);
        AuditRequiredCancellationUserLoginHandler.LastTokenCanBeCanceled.Should().BeTrue();
    }

    [Fact]
    public async Task CommandHandler_ShouldDispatchCommand_WhenHandlerReturnsTask()
    {
        AuditTaskUserLoginHandler.HandledCommands = 0;
        AuditTaskUserLoginHandler.LastUsername = null;
        var command = new AuditTaskUserLogin { Username = "testuser" };
        var dispatcher = new EventSourcingBuilder().WithCommand<AuditTaskUserLoginHandler>().Build();

        await dispatcher.SendAsync(command);

        AuditTaskUserLoginHandler.HandledCommands.Should().Be(1);
        AuditTaskUserLoginHandler.LastUsername.Should().Be(command.Username);
    }

    public sealed record RegisterUser
    {
        public required string Username { get; init; }

        public required string Password { get; init; }
    }

    public sealed class RegisterUserHandler
    {
        public ValueTask HandleAsync(RegisterUser command, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException($"User '{command.Username}' cannot be registered.");
        }
    }

    public sealed record AuditUserLogin
    {
        public required string Username { get; init; }
    }

    public sealed class AuditUserLoginHandler
    {
        public static int HandledCommands { get; set; }

        public ValueTask HandleAsync(AuditUserLogin command)
        {
            HandledCommands++;
            return ValueTask.CompletedTask;
        }
    }

    public sealed record AuditRequiredCancellationUserLogin
    {
        public required string Username { get; init; }
    }

    public sealed class AuditRequiredCancellationUserLoginHandler
    {
        public static int HandledCommands { get; set; }

        public static bool LastTokenCanBeCanceled { get; set; }

        public ValueTask HandleAsync(
            AuditRequiredCancellationUserLogin command,
            CancellationToken cancellationToken
        )
        {
            HandledCommands++;
            LastTokenCanBeCanceled = cancellationToken.CanBeCanceled;
            return ValueTask.CompletedTask;
        }
    }

    public sealed record AuditTaskUserLogin
    {
        public required string Username { get; init; }
    }

    public sealed class AuditTaskUserLoginHandler
    {
        public static int HandledCommands { get; set; }

        public static string? LastUsername { get; set; }

        public Task HandleAsync(AuditTaskUserLogin command)
        {
            HandledCommands++;
            LastUsername = command.Username;
            return Task.CompletedTask;
        }
    }
}
