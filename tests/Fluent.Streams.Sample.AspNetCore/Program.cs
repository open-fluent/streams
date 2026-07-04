// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

using Fluent.Streams;
using Fluent.Streams.DependencyInjection;
using Fluent.Streams.Sample;
using Fluent.Streams.Sample.AspNetCore;
using Fluent.Streams.Sample.Commands;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddFluentStreams().WithCommand<CreateNewBasketCommandHandler>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
app.MapOpenApi();

var ordersApi = app.MapGroup("/orders");
ordersApi
    .MapPost(
        "/",
        async ValueTask<Ok<Order>> (
            CreateOrderRequest request,
            ICommandDispatcher commandDispatcher,
            CancellationToken cancellationToken
        ) =>
        {
            var order = await commandDispatcher.SendAsync(
                new CreateNewBasket { Items = request.Items },
                cancellationToken
            );

            return TypedResults.Ok(order);
        }
    )
    .WithName("CreateOrder");

await app.RunAsync();
