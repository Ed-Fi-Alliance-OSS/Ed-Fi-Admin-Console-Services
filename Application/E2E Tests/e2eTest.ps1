# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

function Read-EnvVariables {
    $envFilePath = "$PSScriptRoot/.env"
    $envFileContent = Get-Content -Path $envFilePath

    foreach ($line in $envFileContent) {
        if ($line -match "^\s*([^#][^=]+)=(.*)$") {
            $name = $matches[1].Trim()
            $value = $matches[2].Trim()
            [System.Environment]::SetEnvironmentVariable($name, $value)
        }
    }
}

function Register-AdminApiClient {
    $headers = @{
        "Content-Type" = "application/x-www-form-urlencoded"
        "tenant" = $env:DEFAULTTENANT
    }

    $body = @{
        "ClientId" = $clientId
        "ClientSecret" = $clientSecret
        "DisplayName" = $clientId
    }

    try {
        $response = Invoke-RestMethod -SkipCertificateCheck -Uri $RegisterUrl -Method Post -Headers $headers -Body $body -StatusCodeVariable statusCode
        
        $output = [PSCustomObject]@{
            Body        = $response
            StatusCode  = $statusCode
        }

        return $output
    }
    catch {
        Write-Error "Failed to send request to $RegisterUrl. Error: $_" -ErrorAction Stop
    }
}

function Get-Token {
    $headers = @{
        "Content-Type"  = "application/x-www-form-urlencoded"
        "tenant" = "$env:DEFAULTTENANT"
    }

    $body = @{
        "client_id" = $clientId
        "client_secret" = $clientSecret
        "grant_type" = "client_credentials"
        "scope" = "edfi_admin_api/full_access"
    }

    try {
        $response = Invoke-RestMethod -SkipCertificateCheck -Uri $TokenUrl -Method Post -Headers $headers -Body $body -StatusCodeVariable statusCode
        
        $output = [PSCustomObject]@{
            Body        = $response
            StatusCode  = $statusCode
        }

        return $output
    }
    catch {
        Write-Error "Failed to send request to $TokenUrl. Error: $_" -ErrorAction Stop
    }
}

function Create-Instance {
    param (
        [Parameter(Mandatory = $true)]
        [string]$access_token,

        [Parameter(Mandatory = $true)]
        [string]$filePath
    )

    $headers = @{
        "Authorization" = "Bearer $access_token"
        "Content-Type"  = "application/json"
        "tenant" = $env:DEFAULTTENANT
    }

    try {
        $response = Invoke-RestMethod -SkipCertificateCheck -Uri $adminConsoleInstancesUrl -Method Post -Headers $headers -InFile $filePath -StatusCodeVariable statusCode

        $output = [PSCustomObject]@{
            Body        = $response
            StatusCode  = $statusCode
        }

        return $output
    }
    catch {
        Write-Error "Failed to send request to $adminConsoleInstancesUrl. Error: $_" -ErrorAction Stop
    }
}

function Get-HealthCheck {
    param (
        [Parameter(Mandatory = $true)]
        [string]$access_token
    )

    $headers = @{
        "Authorization" = "Bearer $access_token"
        "Content-Type"  = "application/json"
        "tenant" = $env:DEFAULTTENANT
    }
    
    try {
        $response = Invoke-RestMethod -SkipCertificateCheck -Uri $AdminConsoleHealthCheckUrl -Method Get -Headers $headers -StatusCodeVariable statusCode
        
        $output = [PSCustomObject]@{
            Body        = $response
            StatusCode  = $statusCode
        }

        return $output
    }
    catch {
        Write-Error "Failed to send request to $AdminConsoleHealthCheckUrl. Error: $_" -ErrorAction Stop
    }
}

# Pre-Step
Read-EnvVariables

# Global Variables
$adminConsoleInstancesUrl = "$env:ADMIN_API/adminconsole/instances"
$AdminConsoleHealthCheckUrl = "$env:ADMIN_API/adminconsole/healthcheck"
$RegisterUrl = "$env:ADMIN_API/connect/register"
$TokenUrl = "$env:ADMIN_API/connect/token"
$clientId = "client-" + $(New-Guid)
$clientSecret = $env:CLIENT_SECRET
$AdminConsoleHealthCheckWorkerProcessPath = $env:ADMIN_CONSOLE_HEALTHCHECK_WORKER_PROCESS_PATH

if ($env:MULTITENANCY -eq "true")
{
    # 1. Register client on Admin Api
    Write-Host "Register client..."
    $response = Register-AdminApiClient
    if ($response.StatusCode -ne 200) {
        Write-Error "Not able to register user on Admin Api." -ErrorAction Stop
    }

    # 2. Get Token from Admin Api
    Write-Host "Get token..."
    $response = Get-Token
    if ($response.StatusCode -ne 200) {
        Write-Error "Not able to get token on Admin Api." -ErrorAction Stop
    }

    $access_token = $response.Body.access_token

    # 3.1 Create Instance
    Write-Host "Create Instance..."
    $response = Create-Instance -access_token $access_token -filePath "./instance.json"
    if ($response.StatusCode -ne 201) {
        Write-Error "Not able to create instance on Admin Api - Console" -ErrorAction Stop
    }

    # 4. Call worker

    Set-Location $AdminConsoleHealthCheckWorkerProcessPath

    $clientIdArg = "--ClientId=$clientId"
    $clientSecretArg = "--ClientSecret=$clientSecret"
    $multitenancyArg = "--IsMultiTenant=true"
    $tenantArg = "--Tenant=$env:DEFAULTTENANT"
    $dotnetApp = "EdFi.AdminConsole.HealthCheckService.dll"

    Write-Host "Call Ed-Fi-Admin-Console-Health-Check-Worker-Process..."
    dotnet $dotnetApp $clientIdArg $clientSecretArg $multitenancyArg $tenantArg

    Set-Location -Path $PSScriptRoot

    # 5. Get HealthCheck
    Write-Host "Get HealthCheck..."
    $response = Get-HealthCheck -access_token $access_token
    if ($response.StatusCode -ne 200) {
        Write-Error "Not able to get get healthcheck on Admin Api." -ErrorAction Stop
    }

    # Check if the response is an array
    Write-Host "Check response..."
    if ($response.Body -is [System.Collections.IEnumerable]) {
        # Iterate through each item in the array
        foreach ($healthcheckItem in $response.Body) {
            if ($healthcheckItem.document.healthy -ne $True) {
                Write-Error "Instance: ${healthcheckItem.document.instanceId} is not healthy" -ErrorAction Stop
            }
            else {
                Write-Host "Instance: ${healthcheckItem.document.instanceId} is healthy"
            }
        }
    } else {
        Write-Error "HealthCheck response is not an array." -ErrorAction Stop
    }
}
else {
    Write-Error "Single tenant not supported yet." -ErrorAction Stop
}

Write-Host "HealthCheck Data returned. Process completed."
