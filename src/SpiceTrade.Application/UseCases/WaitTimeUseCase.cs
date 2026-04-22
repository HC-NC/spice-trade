namespace SpiceTrade.Application.UseCases;

public sealed class WaitTimeInput
{
    public required int Days { get; init; }
}

public sealed class WaitTimeUseCase : UseCase<WaitTimeInput, UseCaseResult<WaitTimeOutput>>
{
    public override UseCaseResult<WaitTimeOutput> Execute(WaitTimeInput input)
    {
        if (input.Days <= 0)
            return new UseCaseResult<WaitTimeOutput> { Success = false, Message = "Количество дней должно быть положительным" };

        return new UseCaseResult<WaitTimeOutput>
        {
            Success = true,
            Message = $"Прошло {input.Days} дней.",
            Data = new WaitTimeOutput
            {
                DaysPassed = input.Days
            }
        };
    }
}

public sealed class WaitTimeOutput
{
    public required int DaysPassed { get; init; }
}