
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY VideoStreaming.csproj ./
RUN dotnet restore "VideoStreaming.csproj"


COPY . ./

RUN dotnet publish "VideoStreaming.csproj" -c Release -o /app/publish /p:UseAppHost=false



FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

USER root


RUN apt-get update \
    && apt-get install -y --no-install-recommends ffmpeg \
    && rm -rf /var/lib/apt/lists/*


ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80


COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "VideoStreaming.dll"]
