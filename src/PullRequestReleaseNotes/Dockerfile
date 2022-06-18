# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:6.0-jammy AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.csproj .
RUN dotnet restore -r linux-x64 /p:PublishReadyToRun=true

# copy and publish app and libraries
COPY . .
RUN dotnet publish -c release -o /app -r linux-x64 --self-contained true --no-restore /p:PublishReadyToRun=true /p:PublishSingleFile=true /p:IncludeSymbolsInSingleFile=false /p:PublishTrimmed=true

# final stage/image
FROM mcr.microsoft.com/dotnet/runtime-deps:6.0-jammy
WORKDIR /app
COPY --from=build /app .

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        libgit2-dev \
    && rm -rf /var/lib/apt/lists/*
RUN ln -s /usr/lib/x86_64-linux-gnu/libgit2.so /usr/lib/x86_64-linux-gnu/libgit2-106a5f2.so

ENTRYPOINT ["./PullRequestReleaseNotes"]
