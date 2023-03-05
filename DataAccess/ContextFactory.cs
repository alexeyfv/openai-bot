using OpenAiBot.Models;

namespace OpenAiBot.DataAccess;

public interface IContextFactory
{
    Context Create();
}

public class ContextFactory : IContextFactory
{
    private readonly ConnectionInfo dbInfo;

    public ContextFactory(ConnectionInfo dbInfo)
    {
        this.dbInfo = dbInfo ?? throw new ArgumentNullException(nameof(dbInfo));
    }

    public Context Create() => new Context(dbInfo);
}