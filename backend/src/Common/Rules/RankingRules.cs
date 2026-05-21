using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Common.Exceptions;

namespace backend.src.Common.Rules;

public static class RankingRules
{
    public static int CalculateScoreFromEvents(IEnumerable<Event> events, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = events.Where(e => e.Status == EventStatus.Approved);

        if (fromDate.HasValue)
        {
            query = query.Where(e => e.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.CreatedAt <= toDate.Value);
        }

        return query.Sum(e => e.Type == EventType.Negative ? -e.Points : e.Points);
    }

    public static void ValidateEventApprovedBeforeScoring(EventStatus status)
    {
        if (status != EventStatus.Approved)
        {
            throw new BusinessRuleException(
                "event_not_approved",
                "A pontuação só é aplicada após a aprovação do evento."
            );
        }
    }
}
