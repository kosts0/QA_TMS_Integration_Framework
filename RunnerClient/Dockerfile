FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app
COPY *.csproj ./
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_ENVIRONMENT=Prod
ENTRYPOINT ["dotnet", "RunnerClient.dll"]