using backend.src.Handlers.Jobs;
using Quartz;

namespace backend.src.Jobs;

public class NightlyCleanupJob : IJob
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public NightlyCleanupJob(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var voteHandler = scope.ServiceProvider.GetRequiredService<IResolveExpiredVotesJobHandler>();
        var sharedEventHandler = scope.ServiceProvider.GetRequiredService<ICloseExpiredSharedEventsJobHandler>();

        var cutoff = DateTime.UtcNow.Date;
        var ct = context.CancellationToken;

        var voteResult = await voteHandler.ProcessAsync(cutoff, ct);
        var sharedEventResult = await sharedEventHandler.ProcessAsync(cutoff, ct);
    }
}
