ARG base=3.1-alpine

# copy and publish app
FROM mcr.microsoft.com/dotnet/core/sdk:${base} AS build
WORKDIR /build
COPY . .
RUN dotnet publish -c Release -o /build/out

# start app
FROM mcr.microsoft.com/dotnet/core/runtime:${base} AS runtime
RUN apk add --no-cache tzdata
WORKDIR /app
COPY --from=build /build/out .
WORKDIR /appdata
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENTRYPOINT ["dotnet", "/app/simple-station.dll"]
