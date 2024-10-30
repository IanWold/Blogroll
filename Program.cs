using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using Metalsharp;
using Metalsharp.LiquidTemplates;

var config = JsonSerializer.Deserialize<Config>(await File.ReadAllTextAsync("Config/config.json"));
var feeds = new List<SyndicationFeed>();

using (var client = new HttpClient())
{
    var downloadTasks = config.Feeds.Select(async url =>
    {
        try
        {
            Console.WriteLine($"Downloading {url}");

            using var stream = await client.GetStreamAsync(url);

            using var xmlReader = XmlReader.Create(stream);
            var feed = SyndicationFeed.Load(xmlReader);

            if (feed != null)
            {
                lock (feeds)
                {
                    feeds.Add(feed);
                }
            }
            else
            {
                Console.WriteLine($"Failed to parse feed from {url}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to download {url}: {ex.Message}");
        }
    });

    await Task.WhenAll(downloadTasks);
}

Console.WriteLine("Finished downloading rss feeds");

var blogs = feeds.Select(f => {
    var url = f.Links.First(l => !l.Uri.ToString().Contains("feed")).Uri.ToString();
    return new Blog(
        url,
        f.Title.Text,
        f.Description?.Text ?? string.Empty,
        f.Items
            .Where(i => i.Links.Count != 0)
            .Select(i => new Post(i.Links.First().Uri.ToString(), i.Title.Text, i.PublishDate.UtcDateTime.ToString("dd MMMM yyyy"), f.Title.Text, url))
    );
});

var posts =
    blogs
    .SelectMany(b => b.Posts)
    .OrderBy(p => p.Date)
    .Take(100);

new MetalsharpProject()
.AddOutput(new MetalsharpFile(string.Empty, "index.html", new Dictionary<string, object>()
{
    ["template"] = "blogroll",
    ["blogs"] = blogs,
    ["title"] = "Hello, I'm <a href=\"https://ian.wold.guru\">Ian</a>! This is my blogroll.",
    ["subtitle"] = "This site is generated and hosted on <a href=\"https://github.com/IanWold/Blogroll\">GitHub</a>, updating each morning."
}))
.AddOutput(new MetalsharpFile(string.Empty, "feed.html", new Dictionary<string, object>()
{
    ["template"] = "feed",
    ["posts"] = posts,
    ["title"] = "Posts Feed",
    ["subtitle"] = "Here's the latest 100 posts from the blogs on my roll."
}))
.UseLiquidTemplates("Templates")
.AddOutput("Static", @".\")
.Build(new BuildOptions()
{
	OutputDirectory = "output",
	ClearOutputDirectory = true
});

record Config(string[] Feeds);
record Post(string Url, string Title, string Date, string Author, string AuthorUrl);
record Blog(string Url, string Name, string Description, IEnumerable<Post> Posts);