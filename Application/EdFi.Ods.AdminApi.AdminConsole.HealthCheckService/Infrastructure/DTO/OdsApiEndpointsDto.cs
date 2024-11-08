// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.Extensions.Options;
using System.Collections;

namespace EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Infrastructure.DTO;

public interface IOdsApiEndpoints : IEnumerable<string>
{
    
}

public  class OdsApiEndpointsDto : IOdsApiEndpoints
{
    private List<string> endpoints;
    private OdsApiSettings _odsApiOptions;

    public OdsApiEndpointsDto(IOptions<OdsApiSettings> odsApiOptions)
    {
        _odsApiOptions = odsApiOptions.Value;

        endpoints = new List<string>();
        endpoints.AddRange(_odsApiOptions.Endpoints);
    }

    public IEnumerator<string> GetEnumerator()
    {
        return endpoints.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}
