// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams;

internal sealed class ResultCommandRegistration<TCommand, TResult, THandler>(
    Func<THandler, TCommand, CancellationToken, ValueTask<TResult>> handler
) : ICommandRegistration<TCommand, TResult>
    where TCommand : notnull
    where THandler : class, new()
{
    public ValueTask<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        return handler(new THandler(), command, cancellationToken);
    }
}
