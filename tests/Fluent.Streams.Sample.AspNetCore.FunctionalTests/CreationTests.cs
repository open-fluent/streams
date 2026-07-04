// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

using Fluent.Streams.Sample.ValueObjects;

namespace Fluent.Streams.Sample.AspNetCore.FunctionalTests;

public sealed class CreationTests : WebAppTest
{
    [Fact]
    public async Task CreateOrder_ShouldSucceed()
    {
        await Client
            .Post(
                "/orders",
                new { items = (OrderedItem[])[new OrderedItem { ProductId = Guid.NewGuid(), Quantity = 1 }] },
                TestContext.Current.CancellationToken
            )
            .Should()
            .Succeed();
    }

    [Fact]
    public async Task CreateOrder_ShouldReturnResponse()
    {
        await Client
            .Post(
                "/orders",
                new { items = (OrderedItem[])[new OrderedItem { ProductId = Guid.NewGuid(), Quantity = 1 }] },
                TestContext.Current.CancellationToken
            )
            .Should()
            .Satisfy<Order>(s =>
            {
                s.Items.Should().HaveCount(1);
            });
    }
}
