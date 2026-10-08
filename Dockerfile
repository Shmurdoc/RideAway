FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY RideAway.sln ./
COPY RideAway.API/RideAway.API.csproj RideAway.API/
COPY RideAway.Application/RideAway.Application.csproj RideAway.Application/
COPY RideAway.Domain/RideAway.Domain.csproj RideAway.Domain/
COPY RideAway.Infrastructure/RideAway.Infrastructure.csproj RideAway.Infrastructure/
COPY RideAway.Tests/RideAway.Tests.csproj RideAway.Tests/

RUN dotnet restore RideAway.sln

COPY RideAway.API/ RideAway.API/
COPY RideAway.Application/ RideAway.Application/
COPY RideAway.Domain/ RideAway.Domain/
COPY RideAway.Infrastructure/ RideAway.Infrastructure/
COPY RideAway.Tests/ RideAway.Tests/

RUN dotnet build RideAway.sln -c Release --no-restore

RUN dotnet publish RideAway.API/RideAway.API.csproj -c Release --no-build -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Run unprivileged: the app never needs root, and a container escape should not start
# from a root-owned process.
RUN adduser --system --uid 1001 --group AppUser \
    && chown -R AppUser:AppUser /app
USER AppUser

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 8080

ENTRYPOINT ["dotnet", "RideAway.API.dll"]
