FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
RUN apt-get update -yq \
    && apt-get install curl gnupg -yq \
    && curl -sL https://deb.nodesource.com/setup_16.x | bash \
    && apt-get install nodejs -yq
RUN npm install react-scripts@4.0.3 -g --silent
WORKDIR /src

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
RUN dotnet restore "CastIt.Server/CastIt.Server.csproj"

COPY ["CastIt.Domain/", "/src/CastIt.Domain"]
COPY ["CastIt.Shared/", "/src/CastIt.Shared"]
COPY ["CastIt.GoogleCast.Shared/", "/src/CastIt.GoogleCast.Shared"]
COPY ["CastIt.FFmpeg/", "/src/CastIt.FFmpeg"]
COPY ["CastIt.GoogleCast/", "/src/CastIt.GoogleCast"]
COPY ["CastIt.GoogleCast.LocalFile/", "/src/CastIt.GoogleCast.LocalFile"]
COPY ["CastIt.GoogleCast.Generator/", "/src/CastIt.GoogleCast.Generator"]
COPY ["CastIt.Server.Shared/", "/src/CastIt.Server.Shared"]
COPY ["CastIt.GoogleCast.Youtube/", "/src/CastIt.GoogleCast.Youtube"]
COPY ["CastIt.Youtube/", "/src/CastIt.Youtube"]
COPY ["CastIt.Server/", "/src/CastIt.Server"]

WORKDIR "/src/CastIt.Server"
RUN dotnet build "CastIt.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CastIt.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CastIt.Server.dll"]