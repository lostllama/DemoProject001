#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["PageFetcher/PageFetcher.csproj", "PageFetcher/"]
RUN dotnet restore "PageFetcher/PageFetcher.csproj"
COPY . .
WORKDIR "/src/PageFetcher"
RUN dotnet build "PageFetcher.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PageFetcher.csproj" -c Release --runtime linux-x64 -p:PublishSingleFile=true -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

#ENTRYPOINT ["dotnet", "PageFetcher.dll"]
ENTRYPOINT ["/bin/bash"]