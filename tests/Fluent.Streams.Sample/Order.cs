// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

using Fluent.Streams.Sample.Events;
using Fluent.Streams.Sample.ValueObjects;

namespace Fluent.Streams.Sample;

public sealed partial class Order
{
    private Order() { }

    public Guid BaskedId { get; private set; }

    public OrderedItem[] Items { get; private set; } = [];

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ConfirmedAt { get; private set; }

    public DateTimeOffset? SentAt { get; private set; }

    public void MarkAsConfirmed(DateTimeOffset confirmedAt)
    {
        Apply(new Confirmed { ConfirmedAt = confirmedAt });
    }

    public void MarkAsSent(DateTimeOffset sentAt)
    {
        Apply(new Sent { SentAt = sentAt });
    }

    public static Order Create(Guid id, DateTimeOffset createdAt, OrderedItem[] items)
    {
        var basket = new Order();

        basket.Apply(
            new Created
            {
                Id = id,
                Items = items,
                CreatedAt = createdAt,
            }
        );

        return basket;
    }

    private void Apply(object @event)
    {
        switch (@event)
        {
            case Created e:
                BaskedId = e.Id;
                Items = e.Items;
                CreatedAt = e.CreatedAt;
                break;

            case Confirmed e:
                ConfirmedAt = e.ConfirmedAt;
                break;

            case Sent e:
                SentAt = e.SentAt;
                break;

            default:
                throw new InvalidOperationException($"Unknown event type: {@event.GetType().Name}");
        }
    }
}
