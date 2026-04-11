FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "AuthService.dll"] 
# Add this before dotnet restore
RUN dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org || true
RUN dotnet restore --disable-parallel