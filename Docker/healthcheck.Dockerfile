# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

# Image based on .NET SDK to compile and publish the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY ../Application/EdFi.AdminConsole.HealthCheckService.sln .
COPY ../Application/EdFi.AdminConsole.HealthCheckService/EdFi.AdminConsole.HealthCheckService.csproj ./EdFi.AdminConsole.HealthCheckService/

# Restore dependencies
RUN dotnet restore

# Copy source code and compile the application
COPY ../Application/EdFi.AdminConsole.HealthCheckService/. ./EdFi.AdminConsole.HealthCheckService/
WORKDIR /source/EdFi.AdminConsole.HealthCheckService
RUN dotnet publish -c Release -o /app

# .NET Runtime image to execute the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Execute the app
ENTRYPOINT ["dotnet", "EdFi.AdminConsole.HealthCheckService.dll"]
