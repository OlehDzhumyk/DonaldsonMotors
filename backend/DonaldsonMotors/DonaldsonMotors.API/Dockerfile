# Build context: backend/DonaldsonMotors (the folder with the .sln)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["DonaldsonMotors.API/DonaldsonMotors.API.csproj", "DonaldsonMotors.API/"]
RUN dotnet restore "DonaldsonMotors.API/DonaldsonMotors.API.csproj"
COPY DonaldsonMotors.API/ DonaldsonMotors.API/
RUN dotnet publish "DonaldsonMotors.API/DonaldsonMotors.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "DonaldsonMotors.API.dll"]
