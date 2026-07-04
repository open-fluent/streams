// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams.SourceGenerators;

[Generator(LanguageNames.CSharp)]
public sealed class CommandHandlerGenerator : IIncrementalGenerator
{
    private static readonly SymbolDisplayFormat TypeFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
            | SymbolDisplayMiscellaneousOptions.UseSpecialTypes
    );

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<CommandHandlerInfo?> registrations = context
            .SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => IsWithCommandInvocation(node),
                static (syntaxContext, cancellationToken) =>
                    TryCreateHandlerInfo(syntaxContext, cancellationToken)
            )
            .Where(static registration => registration is not null);

        context.RegisterSourceOutput(
            registrations.Collect(),
            static (productionContext, registrations) => Execute(productionContext, registrations)
        );
    }

    private static bool IsWithCommandInvocation(SyntaxNode node)
    {
        return node
            is InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax
                {
                    Name: GenericNameSyntax
                    {
                        Identifier.ValueText: "WithCommand",
                        TypeArgumentList.Arguments.Count: 1,
                    },
                },
            };
    }

    private static CommandHandlerInfo? TryCreateHandlerInfo(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken
    )
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return null;
        }

        if (
            memberAccess.Name is not GenericNameSyntax genericName
            || genericName.TypeArgumentList.Arguments.Count != 1
        )
        {
            return null;
        }

        if (genericName.Identifier.ValueText != "WithCommand")
        {
            return null;
        }

        ITypeSymbol? receiverType = context
            .SemanticModel.GetTypeInfo(memberAccess.Expression, cancellationToken)
            .Type;

        BuilderKind builderKind;
        string? receiverTypeName = receiverType?.ToDisplayString(TypeFormat);
        if (receiverTypeName is null or "global::Fluent.Streams.EventSourcingBuilder")
        {
            builderKind = BuilderKind.Core;
        }
        else if (receiverTypeName == "global::Fluent.Streams.DependencyInjection.EventSourcingServiceBuilder")
        {
            builderKind = BuilderKind.DependencyInjection;
        }
        else
        {
            return null;
        }

        TypeSyntax handlerTypeSyntax = genericName.TypeArgumentList.Arguments[0];
        if (
            context.SemanticModel.GetTypeInfo(handlerTypeSyntax, cancellationToken).Type
            is not INamedTypeSymbol handlerType
        )
        {
            return null;
        }

        return TryCreateHandlerInfo(handlerType, builderKind);
    }

    private static CommandHandlerInfo? TryCreateHandlerInfo(
        INamedTypeSymbol handlerType,
        BuilderKind builderKind
    )
    {
        foreach (ISymbol member in handlerType.GetMembers("HandleAsync"))
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }

            if (method.DeclaredAccessibility != Accessibility.Public || method.IsStatic)
            {
                continue;
            }

            if (method.Parameters.Length is < 1 or > 2)
            {
                continue;
            }

            if (method.Parameters.Length == 2 && !IsCancellationToken(method.Parameters[1].Type))
            {
                continue;
            }

            if (!TryGetValueTaskResult(method.ReturnType, out ITypeSymbol? resultType, out bool hasResult))
            {
                continue;
            }

            ITypeSymbol commandType = method.Parameters[0].Type;
            string handlerTypeName = handlerType.ToDisplayString(TypeFormat);
            string commandTypeName = commandType.ToDisplayString(TypeFormat);
            string? resultTypeName = resultType?.ToDisplayString(TypeFormat);
            string uniqueName = SanitizeIdentifier(
                $"{handlerTypeName}_{commandTypeName}_{(hasResult ? resultTypeName : "Void")}".Replace(
                    "global::",
                    string.Empty
                )
            );

            return new CommandHandlerInfo(
                handlerTypeName,
                commandTypeName,
                resultTypeName,
                hasResult,
                method.Parameters.Length == 2,
                uniqueName,
                builderKind
            );
        }

        return null;
    }

    private static bool IsCancellationToken(ITypeSymbol type)
    {
        return type.ToDisplayString(TypeFormat) == "global::System.Threading.CancellationToken";
    }

    private static bool TryGetValueTaskResult(
        ITypeSymbol type,
        out ITypeSymbol? resultType,
        out bool hasResult
    )
    {
        resultType = null;
        hasResult = false;

        if (type is not INamedTypeSymbol namedType)
        {
            return false;
        }

        if (
            namedType.Name != "ValueTask"
            || namedType.ContainingNamespace.ToDisplayString() != "System.Threading.Tasks"
        )
        {
            return false;
        }

        if (namedType.TypeArguments.Length == 0)
        {
            return true;
        }

        if (namedType.TypeArguments.Length == 1)
        {
            resultType = namedType.TypeArguments[0];
            hasResult = true;
            return true;
        }

        return false;
    }

    private static void Execute(
        SourceProductionContext context,
        ImmutableArray<CommandHandlerInfo?> nullableRegistrations
    )
    {
        CommandHandlerInfo[] registrations = nullableRegistrations
            .Where(static registration => registration is not null)
            .Select(static registration => registration!)
            .Distinct(CommandHandlerInfoComparer.Instance)
            .OrderBy(static registration => registration.HandlerType, StringComparer.Ordinal)
            .ThenBy(static registration => registration.CommandType, StringComparer.Ordinal)
            .ToArray();

        if (registrations.Length == 0)
        {
            return;
        }

        context.AddSource(
            "FluentStreams.CommandRegistrations.g.cs",
            SourceText.From(GenerateCommandRegistrations(registrations), Encoding.UTF8)
        );
    }

    private static string GenerateCommandRegistrations(IReadOnlyList<CommandHandlerInfo> registrations)
    {
        var source = new StringBuilder();
        source.AppendLine("// <auto-generated />");
        source.AppendLine("#nullable enable");
        source.AppendLine();
        source.AppendLine("namespace Fluent.Streams");
        source.AppendLine("{");
        CommandHandlerInfo[] coreRegistrations = registrations
            .Where(static registration => registration.BuilderKind == BuilderKind.Core)
            .ToArray();
        if (coreRegistrations.Length > 0)
        {
            source.AppendLine("    public static class EventSourcingBuilderGeneratedExtensions");
            source.AppendLine("    {");
            source.AppendLine(
                "        public static global::Fluent.Streams.EventSourcingBuilder WithCommand<THandler>(this global::Fluent.Streams.EventSourcingBuilder builder)"
            );
            source.AppendLine("            where THandler : class, new()");
            source.AppendLine("        {");

            foreach (CommandHandlerInfo registration in coreRegistrations)
            {
                AppendRegistration(source, registration);
            }

            source.AppendLine("            return builder;");
            source.AppendLine("        }");
            source.AppendLine("    }");
            source.AppendLine();
        }

        AppendDispatcherExtensions(source, registrations);
        source.AppendLine("}");

        CommandHandlerInfo[] dependencyInjectionRegistrations = registrations
            .Where(static registration => registration.BuilderKind == BuilderKind.DependencyInjection)
            .ToArray();
        if (dependencyInjectionRegistrations.Length > 0)
        {
            source.AppendLine();
            source.AppendLine("namespace Fluent.Streams.DependencyInjection");
            source.AppendLine("{");
            source.AppendLine("    public static class EventSourcingServiceBuilderGeneratedExtensions");
            source.AppendLine("    {");
            source.AppendLine(
                "        public static global::Fluent.Streams.DependencyInjection.EventSourcingServiceBuilder WithCommand<THandler>(this global::Fluent.Streams.DependencyInjection.EventSourcingServiceBuilder builder)"
            );
            source.AppendLine("            where THandler : class");
            source.AppendLine("        {");

            foreach (CommandHandlerInfo registration in dependencyInjectionRegistrations)
            {
                AppendRegistration(source, registration);
            }

            source.AppendLine("            return builder;");
            source.AppendLine("        }");
            source.AppendLine("    }");
            source.AppendLine("}");
        }

        return source.ToString();
    }

    private static void AppendRegistration(StringBuilder source, CommandHandlerInfo registration)
    {
        source.AppendLine($"            if (typeof(THandler) == typeof({registration.HandlerType}))");
        source.AppendLine("            {");

        if (registration.HasResult)
        {
            source.AppendLine(
                $"                return builder.RegisterCommand<{registration.CommandType}, {registration.ResultType}, {registration.HandlerType}>("
            );
        }
        else
        {
            source.AppendLine(
                $"                return builder.RegisterCommand<{registration.CommandType}, {registration.HandlerType}>("
            );
        }

        string invocation = registration.AcceptsCancellationToken
            ? "handler.HandleAsync(command, cancellationToken)"
            : "handler.HandleAsync(command)";

        source.AppendLine(
            $"                    static (handler, command, cancellationToken) => {invocation}"
        );
        source.AppendLine("                );");
        source.AppendLine("            }");
        source.AppendLine();
    }

    private static void AppendDispatcherExtensions(
        StringBuilder source,
        IReadOnlyList<CommandHandlerInfo> registrations
    )
    {
        source.AppendLine("    public static class CommandDispatcherGeneratedExtensions");
        source.AppendLine("    {");

        foreach (
            CommandHandlerInfo registration in registrations.Distinct(CommandDispatchInfoComparer.Instance)
        )
        {
            if (registration.HasResult)
            {
                source.AppendLine(
                    $"        public static global::System.Threading.Tasks.ValueTask<{registration.ResultType}> SendAsync(this global::Fluent.Streams.ICommandDispatcher dispatcher, {registration.CommandType} command, global::System.Threading.CancellationToken cancellationToken = default)"
                );
                source.AppendLine("        {");
                source.AppendLine(
                    $"            return dispatcher.DispatchAsync<{registration.CommandType}, {registration.ResultType}>(command, cancellationToken);"
                );
                source.AppendLine("        }");
            }
            else
            {
                source.AppendLine(
                    $"        public static global::System.Threading.Tasks.ValueTask SendAsync(this global::Fluent.Streams.ICommandDispatcher dispatcher, {registration.CommandType} command, global::System.Threading.CancellationToken cancellationToken = default)"
                );
                source.AppendLine("        {");
                source.AppendLine(
                    $"            return dispatcher.DispatchAsync<{registration.CommandType}>(command, cancellationToken);"
                );
                source.AppendLine("        }");
            }

            source.AppendLine();
        }

        source.AppendLine("    }");
    }

    private static string SanitizeIdentifier(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (char character in value)
        {
            builder.Append(char.IsLetterOrDigit(character) ? character : '_');
        }

        return builder.ToString();
    }

    private sealed class CommandHandlerInfoComparer : IEqualityComparer<CommandHandlerInfo>
    {
        public static readonly CommandHandlerInfoComparer Instance = new();

        public bool Equals(CommandHandlerInfo? x, CommandHandlerInfo? y)
        {
            return x is not null
                && y is not null
                && x.HandlerType == y.HandlerType
                && x.CommandType == y.CommandType
                && x.ResultType == y.ResultType
                && x.HasResult == y.HasResult
                && x.BuilderKind == y.BuilderKind;
        }

        public int GetHashCode(CommandHandlerInfo obj)
        {
            unchecked
            {
                int hash = obj.HandlerType.GetHashCode();
                hash = (hash * 397) ^ obj.CommandType.GetHashCode();
                hash = (hash * 397) ^ (obj.ResultType?.GetHashCode() ?? 0);
                hash = (hash * 397) ^ obj.HasResult.GetHashCode();
                hash = (hash * 397) ^ obj.BuilderKind.GetHashCode();
                return hash;
            }
        }
    }

    private sealed class CommandDispatchInfoComparer : IEqualityComparer<CommandHandlerInfo>
    {
        public static readonly CommandDispatchInfoComparer Instance = new();

        public bool Equals(CommandHandlerInfo? x, CommandHandlerInfo? y)
        {
            return x is not null
                && y is not null
                && x.CommandType == y.CommandType
                && x.ResultType == y.ResultType
                && x.HasResult == y.HasResult;
        }

        public int GetHashCode(CommandHandlerInfo obj)
        {
            unchecked
            {
                int hash = obj.CommandType.GetHashCode();
                hash = (hash * 397) ^ (obj.ResultType?.GetHashCode() ?? 0);
                hash = (hash * 397) ^ obj.HasResult.GetHashCode();
                return hash;
            }
        }
    }

    private enum BuilderKind
    {
        Core,
        DependencyInjection,
    }

    private sealed record CommandHandlerInfo(
        string HandlerType,
        string CommandType,
        string? ResultType,
        bool HasResult,
        bool AcceptsCancellationToken,
        string UniqueName,
        BuilderKind BuilderKind
    );
}
