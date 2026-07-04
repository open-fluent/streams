// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams;

public interface ICommandContext<out TCommand>
    where TCommand : notnull
{
    /// <summary>
    /// Gets the correlation identifier that binds logs, telemetry, and processing
    /// steps to the same logical operation.
    /// </summary>
    Guid CorrelationId { get; }

    /// <summary>Gets the timestamp when this command was executed by the dispatcher.</summary>
    DateTimeOffset ExecutedAt { get; init; }

    /// <summary>
    /// Gets the command instance executed within this context.
    /// </summary>
    TCommand Command { get; }
}
