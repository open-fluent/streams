// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams.UnitTests;

public sealed class CommandHandlerWithExceptionTests
{
    [Fact]
    public async Task CommandHandler_Should_Throw_Exception()
    {
        RegisterUser command = new() { Username = "testuser", Password = "password123" };
        ICommandDispatcher dispatcher = new EventSourcingBuilder().WithCommand<RegisterUserHandler>().Build();

        Func<Task> action = async () => await dispatcher.SendAsync(command);

        await action.Should().ThrowAsync<InvalidOperationException>();
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
}
