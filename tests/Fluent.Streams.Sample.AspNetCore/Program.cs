// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

using Fluent.Streams.Sample;
using Fluent.Streams.Sample.AspNetCore;
using Fluent.Streams.Sample.AspNetCore.ApiModel;
using Fluent.Streams.Sample.Commands;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddFluentStreams().WithCommand<CreateNewCommandHandler>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
app.MapOpenApi();

var ordersApi = app.MapGroup("/orders");
ordersApi
    .MapPost(
        "/",
        async ValueTask<Ok<CreateOrderResponse>> (
            CreateOrderRequest request,
            ICommandDispatcher commandDispatcher,
            CancellationToken cancellationToken
        ) =>
        {
            var order = await commandDispatcher.SendAsync(
                new CreateNew { Items = request.Items },
                cancellationToken
            );

            return TypedResults.Ok(
                new CreateOrderResponse
                {
                    OrderId = order.Id,
                    Items = order.Items,
                    CreatedAt = order.CreatedAt,
                }
            );
        }
    )
    .WithName("CreateOrder");

await app.RunAsync();
