// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams;

/// <summary>
/// Handles a single command.
/// </summary>
/// <remarks>
/// Each command type is expected to have a single registered handler. Implementations
/// are responsible for creating a command execution context, propagating the
/// correlation identifier, and invoking the handler registered for the specified
/// command type.
/// </remarks>
public interface ICommandHandler;
