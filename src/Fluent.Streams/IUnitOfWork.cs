// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams;

/// <summary>
/// Represents a unit of work that encapsulates a set of operations to be performed as a single transaction.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Begins tracking the given entity, in the Added state such that they will be inserted into the database when <see cref="SaveChangesAsync"/> is called.
    /// </summary>
    /// <typeparam name="TStream">The type of the stream.</typeparam>
    /// <param name="newStream">The stream to add.</param>
    void Add<TStream>(TStream newStream)
        where TStream : class;

    /// <summary>
    /// Finds an stream by its ID.
    /// </summary>
    /// <typeparam name="TStream">The type of the stream.</typeparam>
    /// <param name="streamId">The ID of the stream to find.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The stream if found; otherwise, null.</returns>
    /// <remarks>
    /// This method searches for an stream by its ID and returns it if found.
    /// If the stream is not found, it returns null.
    /// </remarks>
    Task<TStream?> LoadAsync<TStream>(Guid streamId, CancellationToken cancellationToken = default)
        where TStream : class;

    /// <summary>
    /// Gets an stream by its ID.
    /// </summary>
    /// <typeparam name="TStream">The type of the stream.</typeparam>
    /// <param name="streamId">The ID of the stream to find.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The stream if found; otherwise, null.</returns>
    /// <remarks>
    /// This method searches for an stream by its ID and returns it if found.
    /// If the stream is not found, it returns null.
    /// </remarks>
    Task<TStream> GetAsync<TStream>(Guid streamId, CancellationToken cancellationToken = default)
        where TStream : class;

    /// <summary>
    /// Saves all changes made within the unit of work asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <remarks>
    /// This method commits all changes made within the unit of work to the underlying data store.
    /// </remarks>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
