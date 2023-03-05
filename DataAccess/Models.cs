using System.ComponentModel.DataAnnotations;

namespace OpenAiBot.DataAccess;

public record User
{
    /// <summary>
    /// User unique Id
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// The number of tokens processed
    /// </summary>
    [ConcurrencyCheck]
    public int TokensProcessed { get; set; }

    /// <summary>
    /// The number of requests by the user
    /// </summary>
    public int Requests { get; set; }

    /// <summary>
    /// User has unlimited access 
    /// </summary>
    public bool HasUnlimited { get; set; }
}

public record Rule
{
    /// <summary>
    /// Rule id
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Limit by max number of processed tokens
    /// </summary>
    public int MaxTokenProcessed { get; init; }

    /// <summary>
    /// Limit by max number of requests
    /// </summary>
    public int MaxRequests { get; init; }

    /// <summary>
    /// If specified, the rule will be applied to the specific user. Otherwise for all users.
    /// </summary>
    public long? UserId { get; init; }

    /// <summary>
    /// Unrestricted mode 
    /// </summary>
    public bool Unrestricted { get; init; }
}

public record RemainingResources(bool Unlimited, int Requests, int Tokens);