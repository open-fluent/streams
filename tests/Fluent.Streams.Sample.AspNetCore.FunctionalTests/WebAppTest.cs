// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Artur Sawicki, Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Streams.Sample.AspNetCore.FunctionalTests;

public abstract class WebAppTest : WebApplicationFactory<Program>
{
    private HttpClient? httpClient = null;

    protected HttpClient Client
    {
        get
        {
            httpClient ??= CreateClient();

            return httpClient;
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Tests");
        base.ConfigureWebHost(builder);
        builder.UseEnvironment("Tests");

        builder.ConfigureServices(services => { });
    }

    /// <inheritdoc />
    public override ValueTask DisposeAsync()
    {
        httpClient?.Dispose();

        return base.DisposeAsync();
    }
}
