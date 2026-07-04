# Fluent.Streams

[Created in Poland by Artur Sawicki, Leszek Pomianowski](https://lepo.co/) and [open-source community](https://github.com/open-fluent/streams/graphs/contributors).  
Fluent.Streams is a markerless CQRS and event sourcing library for .NET built around append-only streams, plain domain commands, source-generated registration, and domain models that stay free from framework contracts.

[![NuGet](https://img.shields.io/nuget/v/Fluent.Streams.svg)](https://www.nuget.org/packages/Fluent.Streams) [![NuGet Downloads](https://img.shields.io/nuget/dt/Fluent.Streams.svg)](https://www.nuget.org/packages/Fluent.Streams) [![GitHub license](https://img.shields.io/github/license/open-fluent/streams)](https://github.com/open-fluent/streams/blob/main/LICENSE)

Your domain should describe the business. Not the framework.

`public sealed record RegisterUser(string Username, string Password);`

No `ICommand`.  
No `ICommand<T>`.  
No required base class.  
No framework reference in your domain model.

Fluent.Streams lets your command and event types remain ordinary .NET types. The infrastructure is connected later, at the application boundary, where source-generated code can wire handlers without forcing marker interfaces into the core domain.

## The library your domain does not need

Fluent.Streams is built so the innermost domain model can live in its own project without referencing Fluent.Streams.  
That means your domain can contain records, entities, value objects, events, commands, and business rules without knowing how dispatching, registration, or persistence is implemented. The application layer decides how those pieces are connected.

Built for event-sourced systems

## Markerless by design

Most CQRS frameworks start by asking your domain to implement their interfaces.

Fluent.Streams takes the opposite direction.  
Your command handlers are ordinary classes with ordinary HandleAsync methods:

```csharp
using Fluent.Streams;

public sealed record RegisterUser(string Username, string Password);

public sealed class RegisterUserHandler
{
  public ValueTask<UserRegistrationResult> HandleAsync(
  RegisterUser command,
  CancellationToken cancellationToken = default)
  {
    // Validate business rules.
    // Append domain events.
    // Return a domain-specific result.
      return ValueTask.FromResult<UserRegistrationResult>(
        new UserRegistered(command.Username));
    }
}

ICommandDispatcher dispatcher = new EventSourcingBuilder()
  .WithCommand<RegisterUserHandler>() // registration + source-generator signal
  .Build();

UserRegistrationResult result = await dispatcher.SendAsync(
  new RegisterUser("leszek", "correct-horse-battery-staple"));
```

Handlers can return `ValueTask`, or `Task` for commands without a result or `ValueTask<TResult>` / `Task<TResult>` when the command returns a union, result type, or custom domain response.

The goal is not to hide event sourcing behind vague abstractions, but to make the essential parts explicit, small, and pleasant to use.

## Modern .NET. Real-world .NET

Fluent.Streams targets modern .NET while still supporting applications that cannot simply abandon .NET Framework.

Microsoft may move on. Production systems often cannot.

Fluent.Streams is built for teams maintaining real systems across generations of .NET, from current runtimes to long-lived framework applications that still matter.

## Designed and crafted by people

Fluent.Streams is created in Poland by people who build software for systems where correctness, maintainability, and architectural clarity matter.

[Artur Sawicki](https://www.linkedin.com/in/artur-sawicki73)  
is a solution designer and software architect with more than 20 years of deep experience in Domain-Driven Design, enterprise architecture, and complex business systems.

[Leszek Pomianowski](https://www.linkedin.com/in/pomian)  
is a quality assurance architect building solutions for critical infrastructure in banking and energy. His open-source work has been adopted by Microsoft, and his experience is backed by libraries with millions of installations.

## Installation

```
dotnet add package Fluent.Streams
```

NuGet: <https://www.nuget.org/packages/Fluent.Streams>

## License

Fluent.Streams is free and open source software licensed under the MIT License. You can use it in private and commercial projects.

Keep in mind that you must include a copy of the license in your project.
