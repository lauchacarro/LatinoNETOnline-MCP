using ModelContextProtocol;
using ModelContextProtocol.Server;
using ProtectedMcpServer.Models;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace ProtectedMcpServer.Tools;

[McpServerToolType]
public sealed class WebinarTools
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WebinarTools(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    private string? GetJwtToken()
    {
        var context = _httpContextAccessor.HttpContext;
        var authorizationHeader = context?.Request.Headers["Authorization"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(authorizationHeader))
            return null;

        const string bearerPrefix = "Bearer ";
        if (authorizationHeader.StartsWith(bearerPrefix))
        {
            return authorizationHeader.Substring(bearerPrefix.Length);
        }

        return null;
    }

    [McpServerTool, Description("Get all upcoming webinars from the LatinoNet platform.")]
    public async Task<string> GetUpcomingWebinars()
    {
        var jwtToken = GetJwtToken();
        if (string.IsNullOrEmpty(jwtToken))
        {
            throw new McpException("Authentication token is required to access webinar data.");
        }

        var client = _httpClientFactory.CreateClient("WebinarApi");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

        try
        {
            var response = await client.GetFromJsonAsync<ProposalsResponse>("/api/v1/webinars-module/Proposals");
            
            if (response == null)
            {
                return "No webinar data available.";
            }

            if (!response.IsSuccess)
            {
                return $"API Error: {response.Error ?? "Unknown error"}";
            }

            if (response.Result == null || !response.Result.Any())
            {
                return "No upcoming webinars found.";
            }

            return string.Join("\n" + new string('-', 50) + "\n", response.Result.Select(item =>
            {
                var proposal = item.Proposal;
                var speakers = item.Speakers;

                var speakersInfo = speakers?.Any() == true
                    ? string.Join(", ", speakers.Select(s => 
                        $"{s.Name} {s.LastName} {(!string.IsNullOrEmpty(s.Email) ? $" ({s.Email})" : "")}{(!string.IsNullOrEmpty(s.Twitter) ? $" - @{s.Twitter}" : "")}"))
                    : "Speaker not specified";

                var links = new List<string>();
                if (!string.IsNullOrEmpty(proposal.Meetup))
                    links.Add($"🔗 Meetup: {proposal.Meetup}");
                if (!string.IsNullOrEmpty(proposal.LiveStreaming))
                    links.Add($"📺 Live: {proposal.LiveStreaming}");

                var linksSection = links.Any() ? $"\n{string.Join("\n", links)}" : "";

                return $"""
                    📅 Webinar #{proposal.WebinarNumber}: {proposal.Title}
                    👤 Speaker(s): {speakersInfo}
                    📆 Date: {proposal.EventDate:dd-MMMM-yyyy}
                    📝 Description: {(proposal.Description.Length > 300 ? proposal.Description.Substring(0, 300) + "..." : proposal.Description)}{linksSection}
                    """;
            }));
        }
        catch (HttpRequestException ex)
        {
            throw new McpException($"Failed to fetch webinar data: {ex.Message}");
        }
        catch (JsonException ex)
        {
            throw new McpException($"Failed to parse webinar response: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new McpException($"An error occurred while fetching webinars: {ex.Message}");
        }
    }

    //[McpServerTool, Description("Search for speakers in the LatinoNet database.")]
    public async Task<string> SearchSpeakers(
        [Description("Search term to find speakers by name, expertise, or other criteria.")] string search,
        [Description("Maximum number of speakers to return (default: 5, max: 50).")] int take = 5)
    {
        var jwtToken = GetJwtToken();
        if (string.IsNullOrEmpty(jwtToken))
        {
            throw new McpException("Authentication token is required to search speakers.");
        }

        if (string.IsNullOrWhiteSpace(search))
        {
            throw new McpException("Search term cannot be empty.");
        }

        if (take <= 0 || take > 50)
        {
            take = 5; // Default to 5 if invalid
        }

        var client = _httpClientFactory.CreateClient("WebinarApi");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

        try
        {
            var encodedSearch = Uri.EscapeDataString(search);
            var requestUrl = $"/api/v1/webinars-module/Speakers/Search?search={encodedSearch}&take={take}";
            
            var response = await client.GetFromJsonAsync<SpeakersSearchResponse>(requestUrl);
            
            if (response == null)
            {
                return "No speaker data available.";
            }

            if (!response.IsSuccess)
            {
                return $"API Error: {response.Error ?? "Unknown error"}";
            }

            if (response.Result == null || !response.Result.Any())
            {
                return $"No speakers found matching '{search}'.";
            }

            var speakers = response.Result;
            var totalFound = speakers.Count;
            var resultHeader = $"Found {totalFound} speaker{(totalFound != 1 ? "s" : "")} matching '{search}':\n" + new string('=', 60) + "\n";

            var speakersInfo = string.Join("\n" + new string('-', 40) + "\n", speakers.Select(speaker =>
            {

                return $"""
                    👤 Name: {speaker.Name} {speaker.LastName}
                    📧 Email: {speaker.Email}
                    🏢 X: {speaker.Twitter}
                    📷 Picture: {speaker.Twitter}
                    💻 Bio: {(speaker.Description.Length > 200 ? speaker.Description.Substring(0, 200) + "..." : speaker.Description)}
                    """;
            }));

            return resultHeader + speakersInfo;
        }
        catch (HttpRequestException ex)
        {
            throw new McpException($"Failed to search speakers: {ex.Message}");
        }
        catch (JsonException ex)
        {
            throw new McpException($"Failed to parse speaker search response: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new McpException($"An error occurred while searching speakers: {ex.Message}");
        }
    }
}
