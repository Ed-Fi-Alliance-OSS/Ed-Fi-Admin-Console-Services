// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Infrastructure.DTO;
using EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Infrastructure.Services.OdsApi;

namespace EdFi.Ods.AdminApi.AdminConsole.HealthCheckService.Features;

public interface IOdsApiCaller
{
    Task ExecuteAsync(IEnumerable<AdminApiInstance> instances);
}

public class OdsApiCaller : IOdsApiCaller
{
    private IOdsApiClient _odsApiClient;
    private IOdsApiEndpoints _odsApiEndpoints;

    public OdsApiCaller(IOdsApiClient odsApiClient, IOdsApiEndpoints odsApiEndpoints)
    {
        _odsApiClient = odsApiClient;
        _odsApiEndpoints = odsApiEndpoints;
    }

    public async Task ExecuteAsync(IEnumerable<AdminApiInstance> instances)
    {
        var urls = _odsApiEndpoints;

        //var urls = new List<string>
        //{
        //    $"https://api.ed-fi.org:443/v7.1/api/data/v3/ed-fi/studentSchoolAssociations?{Constants.OdsApiQueryParams}",
        //    $"https://api.ed-fi.org:443/v7.1/api/data/v3/ed-fi/studentSectionAssociations?{Constants.OdsApiQueryParams}",
        //    $"https://api.ed-fi.org:443/v7.1/api/data/v3/ed-fi/studentSchoolAttendanceEvents?{Constants.OdsApiQueryParams}",
        //    $"https://api.ed-fi.org:443/v7.1/api/data/v3/ed-fi/courseTranscripts?{Constants.OdsApiQueryParams}",
        //    $"https://api.ed-fi.org:443/v7.1/api/data/v3/ed-fi/sections?{Constants.OdsApiQueryParams}"
        //};

        var tasks = new List<Task>();

        foreach (var url in urls)
        {
            tasks.Add(Task.Run(() => SendRequestAsync(url)));
        }

        await Task.WhenAll(tasks);
        Console.WriteLine("All requests completed.");

    }

    static async Task SendRequestAsync(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            var response = await client.GetAsync(url);
            Console.WriteLine($"Response from {url}: {response.StatusCode}");
        }
    }
}
