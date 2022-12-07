#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["PageFetcher.Tests/PageFetcher.Tests.csproj", "PageFetcher.Tests/"]
COPY ["PageFetcher/PageFetcher.csproj", "PageFetcher/"]
RUN dotnet restore "PageFetcher.Tests/PageFetcher.Tests.csproj"
COPY . .
WORKDIR "/src/PageFetcher.Tests"

ENTRYPOINT ["dotnet", "test"]