FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["tests/Fluent.Streams.AspNetCore.Sample/Fluent.Streams.AspNetCore.Sample.csproj", "tests/Fluent.Streams.AspNetCore.Sample/"]
RUN dotnet restore "tests/Fluent.Streams.AspNetCore.Sample/Fluent.Streams.AspNetCore.Sample.csproj"
COPY . .
WORKDIR "/src/tests/Fluent.Streams.AspNetCore.Sample"
RUN dotnet build "./Fluent.Streams.AspNetCore.Sample.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Fluent.Streams.AspNetCore.Sample.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fluent.Streams.AspNetCore.Sample.dll"]
