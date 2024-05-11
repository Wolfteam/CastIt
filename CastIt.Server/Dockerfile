FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update -yq && apt-get install ffmpeg -yq
USER app
WORKDIR /app
EXPOSE 8080

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
ARG BUILD_CONFIGURATION=Release
RUN apt-get update -yq \
    && apt-get install curl gnupg -yq \
    && curl -sL https://deb.nodesource.com/setup_20.x | bash \
    && apt-get install nodejs -yq
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["CastIt.Domain/CastIt.Domain.csproj", "CastIt.Domain/"]
COPY ["CastIt.Shared/CastIt.Shared.csproj", "CastIt.Shared/"]
COPY ["CastIt.GoogleCast.Shared/CastIt.GoogleCast.Shared.csproj", "CastIt.GoogleCast.Shared/"]
COPY ["CastIt.FFmpeg/CastIt.FFmpeg.csproj", "CastIt.FFmpeg/"]
COPY ["CastIt.GoogleCast/CastIt.GoogleCast.csproj", "CastIt.GoogleCast/"]
COPY ["CastIt.GoogleCast.LocalFile/CastIt.GoogleCast.LocalFile.csproj", "CastIt.GoogleCast.LocalFile/"]
COPY ["CastIt.GoogleCast.Generator/CastIt.GoogleCast.Generator.csproj", "CastIt.GoogleCast.Generator/"]
COPY ["CastIt.Server.Shared/CastIt.Server.Shared.csproj", "CastIt.Server.Shared/"]
COPY ["CastIt.GoogleCast.Youtube/CastIt.GoogleCast.Youtube.csproj", "CastIt.GoogleCast.Youtube/"]
COPY ["CastIt.Youtube/CastIt.Youtube.csproj", "CastIt.Youtube/"]
COPY ["CastIt.Server/CastIt.Server.csproj", "CastIt.Server/"]
RUN dotnet restore -a $TARGETARCH "CastIt.Server/CastIt.Server.csproj"

COPY ["CastIt.Domain/", "CastIt.Domain/"]
COPY ["CastIt.Shared/", "CastIt.Shared/"]
COPY ["CastIt.GoogleCast.Shared/", "CastIt.GoogleCast.Shared/"]
COPY ["CastIt.FFmpeg/", "CastIt.FFmpeg/"]
COPY ["CastIt.GoogleCast/", "CastIt.GoogleCast/"]
COPY ["CastIt.GoogleCast.LocalFile/", "CastIt.GoogleCast.LocalFile/"]
COPY ["CastIt.GoogleCast.Generator/", "CastIt.GoogleCast.Generator/"]
COPY ["CastIt.Server.Shared/", "CastIt.Server.Shared/"]
COPY ["CastIt.GoogleCast.Youtube/", "CastIt.GoogleCast.Youtube/"]
COPY ["CastIt.Youtube/", "CastIt.Youtube/"]
COPY ["CastIt.Server/", "CastIt.Server/"]

WORKDIR "/src/CastIt.Server"
RUN dotnet build --no-restore -c $BUILD_CONFIGURATION -a $TARGETARCH

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish --no-restore --no-build -c $BUILD_CONFIGURATION -a $TARGETARCH -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CastIt.Server.dll"]