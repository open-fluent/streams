# Fluent.Streams.DependencyInjection

Markerless CQRS and event sourcing primitives for .NET.

Fluent.Streams.DependencyInjection provides append-only stream-oriented building blocks and source-generated command dispatching while keeping your domain command types framework-free.

```csharp
using Fluent.Streams;

public sealed record RegisterUser(string Username, string Password);

public sealed class RegisterUserHandler
{
    public ValueTask<UserRegistrationResult> HandleAsync(
        RegisterUser command,
        CancellationToken cancellationToken = default)
    {
        // Append events and return a domain-specific result.
        return ValueTask.FromResult<UserRegistrationResult>(new UserRegistered(command.Username));
    }
}

ICommandDispatcher dispatcher = new EventSourcingBuilder()
    .WithCommand<RegisterUserHandler>()
    .Build();

UserRegistrationResult result = await dispatcher.SendAsync(new RegisterUser("leszek", "secret"));
```

Command handlers are ordinary classes with `HandleAsync(TCommand, CancellationToken)` methods. Use `ValueTask` for commands without a result and `ValueTask<TResult>` for commands that return a union, result type, or custom response.

## License

Fluent.Streams is free and open source software licensed under the **MIT License**. You can use it in private and commercial projects.  
Keep in mind that you must include a copy of the license in your project.
