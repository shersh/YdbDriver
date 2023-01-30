FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["examples/ShortenLinks/ShortenLinks.csproj", "ShortenLinks/"]
RUN dotnet restore "examples/ShortenLinks/ShortenLinks.csproj"
COPY . .
WORKDIR "/src/ShortenLinks"
RUN dotnet build "ShortenLinks.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShortenLinks.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShortenLinks.dll"]
