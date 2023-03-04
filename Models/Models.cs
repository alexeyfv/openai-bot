namespace OpenAiBot.Models;

public record OpenAiRequest(IEnumerable<ChatMessage> Messages);

public record OpenAiResponse(string Answer);

public record ChatMessage(Role Role, string Text);

public enum Role
{
    User,
    Assistant
}
