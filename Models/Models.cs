namespace ChatGptBot.Models;

public record ChatGptRequest(IEnumerable<ChatMessage> Messages);

public record ChatGptResponse(string Answer);

public record ChatMessage(Role Role, string Text);

public enum Role
{
    User,
    Assistant
}
