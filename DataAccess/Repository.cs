namespace OpenAiBot.DataAccess;

public interface IRepository
{
    bool HasAccess(long userId);
    User UpsertUser(long userId, int tokens);
    RemainingResources Remaining(long userId);
}

public class Repository : IRepository
{
    private readonly IContextFactory factory;

    public Repository(IContextFactory factory)
    {
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public bool HasAccess(long userId)
    {
        var remaining = Remaining(userId);
        if (remaining.Unlimited) return true;
        return remaining.Tokens > 0 && remaining.Requests > 0;
    }

    public RemainingResources Remaining(long userId)
    {
        using var db = factory.Create();

        // Get user-specific rule if exists, otherwise get common rule
        var rule = db.Rules.FirstOrDefault(r => r.UserId == userId) ?? db.Rules.Find(long.MaxValue);
        var user = db.Users.Find(userId);

        // Create user if not exists
        if (user is null) user = UpsertUser(userId);

        // At least one rule must be specified
        if (rule is null)throw new InvalidOperationException("Rule is not specified");

        var tokens = rule.MaxTokenProcessed - user.TokensProcessed;
        var requests =  rule.MaxRequests - user.Requests;

        return new RemainingResources(user.HasUnlimited, requests, tokens);

    }

    public User UpsertUser(long userId, int tokens = 0)
    {
        using var db = factory.Create();

        var user = db.Users.Find(userId);

        // Create if not exist
        if (user is null)
        {
            db.Users.Add(new User()
            {
                Id = userId,
                Requests = 1,
                TokensProcessed = tokens
            });
            db.SaveChanges();

            user = db.Users.Find(userId);

            // If user is still null - throw an exception
            if (user is null) throw new InvalidOperationException("Unable to create a new user");
        }

        user.Requests++;
        user.TokensProcessed += tokens;
        db.SaveChanges();

        return user;
    }
}