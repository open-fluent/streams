// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams;

internal interface ICommandRegistration;

internal interface ICommandRegistration<TCommand> : ICommandRegistration
    where TCommand : notnull
{
    ValueTask HandleAsync(TCommand command, CancellationToken cancellationToken);
}

internal interface ICommandRegistration<TCommand, TResult> : ICommandRegistration
    where TCommand : notnull
{
    ValueTask<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
}
