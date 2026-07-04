# Fluent.Streams

[Created in Poland by Artur Sawicki, Leszek Pomianowski](https://lepo.co/) and [open-source community](https://github.com/open-fluent/streams/graphs/contributors).  
Markerless CQRS and event sourcing building blocks for .NET: append-only streams, domain commands, projections, and source-generated command dispatching without forcing your domain model to reference framework marker interfaces.

[![NuGet](https://img.shields.io/nuget/v/Fluent.Streams.svg)](https://www.nuget.org/packages/Fluent.Streams) [![NuGet Downloads](https://img.shields.io/nuget/dt/Fluent.Streams.svg)](https://www.nuget.org/packages/Fluent.Streams) [![GitHub license](https://img.shields.io/github/license/open-fluent/streams)](https://github.com/open-fluent/streams/blob/main/LICENSE)

## Getting started

```powershell
dotnet add package Fluent.Streams
```

<https://www.nuget.org/packages/Fluent.Streams>

```csharp
using Fluent.Streams;

public sealed record RegisterUser
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}

public sealed class RegisterUserHandler
{
    public ValueTask HandleAsync(RegisterUser command, CancellationToken cancellationToken = default)
    {
        // Append domain events to your stream here.
        return ValueTask.CompletedTask;
    }
}

ICommandDispatcher dispatcher = new EventSourcingBuilder()
    .WithCommand<RegisterUserHandler>() // generated at compile time
    .Build();

await dispatcher.SendAsync(new RegisterUser
{
    Username = "leszek",
    Password = "correct-horse-battery-staple",
});
```

Handlers can return native `ValueTask` for exception-based flows or `ValueTask<TResult>` when the command produces an outcome. `TResult` may be a C# union, a result type, or any application-specific response.

Fluent.Streams keeps command types plain: no `ICommand`, no `ICommand<T>`, and no required `CommandContext<T>` in your domain handlers. The explicit `EventSourcingBuilder.WithCommand<THandler>()` call acts as both runtime registration and a source-generator signal.

## License

Fluent.Streams is free and open source software licensed under the **MIT License**. You can use it in private and commercial projects.  
Keep in mind that you must include a copy of the license in your project.
