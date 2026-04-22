namespace SpiceTrade.Application.UseCases;

public abstract class UseCase<TInput, TOutput>
{
    public abstract TOutput Execute(TInput input);
}

public abstract class UseCase<TInput>
{
    public abstract void Execute(TInput input);
}

public abstract class UseCase
{
    public abstract void Execute();
}

public class UseCaseResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = "";
}

public class UseCaseResult<T> : UseCaseResult
{
    public T? Data { get; init; }
}