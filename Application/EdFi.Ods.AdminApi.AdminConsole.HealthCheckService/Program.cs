using Microsoft.Extensions.Hosting;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateDefaultApp(args).Build();

        Console.WriteLine("starting");
        await host.RunAsync();
        Console.WriteLine("stopped");


        var urls = new List<string>
        {
            "https://api.ed-fi.org:443/v7.1/api/data/v3/ed-fi/studentSchoolAssociations?offset=0&limit=0&totalCount=true",
            "https://api.ed-fi.org:443/v7.1/api/data/v3/ed-fi/studentSectionAssociations?offset=0&limit=0&totalCount=true",
            "https://api.ed-fi.org:443/v7.1/api/data/v3/ed-fi/studentSchoolAttendanceEvents?offset=0&limit=0&totalCount=true",
            "https://api.ed-fi.org:443/v7.1/api/data/v3/ed-fi/courseTranscripts?offset=0&limit=0&totalCount=true",
            "https://api.ed-fi.org:443/v7.1/api/data/v3/ed-fi/sections?offset=0&limit=0&totalCount=true"
        };

        var tasks = new List<Task>();

        foreach (var url in urls)
        {
            tasks.Add(Task.Run(() => SendRequestAsync(url)));
        }

        await Task.WhenAll(tasks);
        Console.WriteLine("All requests completed.");
    }

    private static IHostBuilder CreateDefaultApp(string[] args)
    {
        var builder = Host.CreateDefaultBuilder();
        
        return builder;
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