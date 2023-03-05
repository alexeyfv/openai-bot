namespace OpenAiBot.Models;

public record OpenAiRequest(ChatMessage[] Messages);

public record OpenAiResponse(string Answer, int Tokens);

public record ChatMessage(long ChatId, long UserId, string Text, Role Role = Role.User);

public record struct CacheKey(long ChatId, long UserID);

public record ChatCommand(long ChatId, long UserId, string Text);

public record BotInfo(string Name);

public record OpenAiInfo(string Token);

public record ConnectionInfo(string DataSource);

public enum Role
{
    User,
    Assistant
}
