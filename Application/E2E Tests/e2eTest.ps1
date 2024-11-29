# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

# Global Variables
$adminApi = "https://localhost:7214"
$adminConsoleInstancesUrl = $adminApi + "/adminconsole/instances"
$AdminConsoleHealthCheckUrl = $adminApi + "/adminconsole/healthcheck"
$RegisterUrl = $adminApi + "/connect/register"
$TokenUrl = $adminApi + "/connect/token"
$clientId = "uu75"
$clientSecret = "Gapuser1234*Gapuser1234*Gapuser1234*Gapuser1234*"
$AdminConsoleHealthCheckWorkerProcessPath = "./../EdFi.AdminConsole.HealthCheckService/bin/Debug/net8.0/"
$AdminConsoleHealthCheckWorkerProcessExe = "EdFi.AdminConsole.HealthCheckService.exe"

function Register-AdminApiClient {
    $headers = @{
        "Content-Type" = "application/x-www-form-urlencoded"
    }

    $body = @{
        "ClientId" = $clientId
        "ClientSecret" = $clientSecret
        "DisplayName" = $clientId
    }

    try {
        $response = Invoke-RestMethod -Uri $RegisterUrl -Method Post -Headers $headers -Body $body -StatusCodeVariable statusCode
        
        $output = [PSCustomObject]@{
            Body        = $response
            StatusCode  = $statusCode
        }

        return $output
    }
    catch {
        Write-Error "Failed to send request to $RegisterUrl. Error: $_"
    }

    # return Send-RestRequest -url $RegisterUrl -headers $headers -method "POST" -body $body
}

function Get-Token {
    $headers = @{
        "Content-Type"  = "application/x-www-form-urlencoded"
    }

    $body = @{
        "client_id" = $clientId
        "client_secret" = $clientSecret
        "grant_type" = "client_credentials"
    }

    try {
        $response = Invoke-RestMethod -Uri $TokenUrl -Method Post -Headers $headers -Body $body -StatusCodeVariable statusCode
        
        $output = [PSCustomObject]@{
            Body        = $response
            StatusCode  = $statusCode
        }

        return $output
    }
    catch {
        Write-Error "Failed to send request to $TokenUrl. Error: $_"
    }
}

function Create-Instance {
    $headers = @{
        "Authorization" = "Bearer $Token"
        "Content-Type"  = "application/json"
    }

    $body = @{
        "docId" = 1
        "instanceId" = 1
        "edOrgId" = 1
        "tenantId" = 1
        "document" = '{\"instanceId\": \"1\",\"tenantId\": \"1\",\"instanceName\": \"instance 1\",\"clientId\": \"RvcohKz9zHI4\",\"clientSecret\": \"E1iEFusaNf81xzCxwHfbolkC\",\"baseUrl\": \"https://api.ed-fi.org/v7.1/api\",\"resourcesUrl\":\"/data/v3/ed-fi/\", \"authenticationUrl\":\"/oauth/token/\"}'
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri $adminConsoleInstancesUrl -Method Post -Headers $headers -Body $body -StatusCodeVariable statusCode
        
        $output = [PSCustomObject]@{
            Body        = $response
            StatusCode  = $statusCode
        }

        return $output
    }
    catch {
        Write-Error "Failed to send request to $adminConsoleInstancesUrl. Error: $_"
    }
}

function Get-HealthCheck {
    $headers = @{
        "Authorization" = "Bearer $Token"
        "Content-Type"  = "application/json"
    }

    
    try {
        Invoke-RestMethod -Uri $AdminConsoleHealthCheckUrl -Method Get -Headers $headers -StatusCodeVariable statusCode
        
        $output = [PSCustomObject]@{
            Body        = $response
            StatusCode  = $statusCode
        }

        return $output
    }
    catch {
        Write-Error "Failed to send request to $AdminConsoleHealthCheckUrl. Error: $_"
    }
}

# 1. Register client on Admin Api
$response = Register-AdminApiClient
if ($response.StatusCode -ne 200) {
    Write-Host "Not able to register user on Admin Api."
    exit 1
}

# 2. Get Token from Admin Api
$response = Get-Token
if ($response.StatusCode -ne 200) {
    Write-Host "Not able to get token on Admin Api."
    exit 1
}

$parsedResponse = $response.Body | ConvertFrom-Json
$access_token = $parsedResponse.access_token

# 3. Create Instance
$response = Create-Instance
if ($response.StatusCode -ne 201) {
    Write-Host "Not able to create instance on Admin Api - Console"
    exit 1
}

# 4. Call worker
$exe = $AdminConsoleHealthCheckWorkerProcessPath + $AdminConsoleHealthCheckWorkerProcessExe
&$exe --ClientId=$clientId --ClientSecret=$clientSecret

# 5. Get HealthCheck
$response = Get-HealthCheck
if ($response.StatusCode -ne 200) {
    Write-Host "Not able to get get healthcheck on Admin Api."
    exit 1
}

Write-Host "HealthCheck Data returned from Admin Api:"
Write-Host $response.Body
