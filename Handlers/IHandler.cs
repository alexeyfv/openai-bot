namespace ChatGptBot.Handlers;

public interface IHandler<TRequest>
{
    Task HandleAsync(TRequest r);
}

public interface IHandler<TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest r);
}
