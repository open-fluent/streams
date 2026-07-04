// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams;

/// <summary>
/// Executes a hook for the event occurred.
/// Implement this interface to define what actually happens when the hook runs
/// (e.g. an HTTP call to an external system, sending an email, etc.).
/// </summary>
public interface IHook;
