using System.Text.Json.Serialization;

namespace LatinoNetOnline.MCP.Server.Models;

// Modelo base para respuestas de API que siguen el patrón result
public class ApiResponse<T>
{
    [JsonPropertyName("result")]
    public T Result { get; set; } = default!;

    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

// Modelo para la respuesta de próximos proposals
public class ProposalsResponse : ApiResponse<List<ProposalWithSpeakers>>
{
}

// Modelo para la respuesta de búsqueda de speakers
public class SpeakersSearchResponse : ApiResponse<List<Speaker>>
{
}

// Modelo que combina proposal y speakers
public class ProposalWithSpeakers
{
    [JsonPropertyName("proposal")]
    public Proposal Proposal { get; set; } = new();

    [JsonPropertyName("speakers")]
    public List<Speaker> Speakers { get; set; } = new();
}

// Modelo para Proposal
public class Proposal
{
    [JsonPropertyName("proposalId")]
    public string ProposalId { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("eventDate")]
    public DateTime EventDate { get; set; }

    [JsonPropertyName("creationTime")]
    public DateTime CreationTime { get; set; }

    [JsonPropertyName("audienceAnswer")]
    public string AudienceAnswer { get; set; } = string.Empty;

    [JsonPropertyName("knowledgeAnswer")]
    public string KnowledgeAnswer { get; set; } = string.Empty;

    [JsonPropertyName("useCaseAnswer")]
    public string UseCaseAnswer { get; set; } = string.Empty;

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("webinarNumber")]
    public int WebinarNumber { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("meetup")]
    public string? Meetup { get; set; }

    [JsonPropertyName("streamyard")]
    public string? Streamyard { get; set; }

    [JsonPropertyName("liveStreaming")]
    public string? LiveStreaming { get; set; }

    [JsonPropertyName("flyer")]
    public string? Flyer { get; set; }

    [JsonPropertyName("views")]
    public int? Views { get; set; }

    [JsonPropertyName("liveAttends")]
    public int? LiveAttends { get; set; }

    //// Propiedad computed para el status como string
    //public string StatusText => Status switch
    //{
    //    0 => "Inactive",
    //    1 => "Active",
    //    2 => "Completed",
    //    3 => "Cancelled",
    //    _ => "Unknown"
    //};
}

// Modelo para Speaker
public class Speaker
{
    [JsonPropertyName("speakerId")]
    public string SpeakerId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("twitter")]
    public string Twitter { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string? Image { get; set; }
 
}
