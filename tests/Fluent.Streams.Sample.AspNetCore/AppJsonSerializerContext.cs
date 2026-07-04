// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

using System.Text.Json.Serialization;
using Fluent.Streams.Sample;
using Fluent.Streams.Sample.ValueObjects;

namespace Fluent.Streams.Sample.AspNetCore;

/// <summary>
/// Provides source-generated JSON metadata for the native AOT ASP.NET Core sample.
/// </summary>
[JsonSerializable(typeof(CreateOrderRequest))]
[JsonSerializable(typeof(Order))]
[JsonSerializable(typeof(OrderedItem))]
[JsonSerializable(typeof(OrderedItem[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext;
