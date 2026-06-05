using backend.src.Common;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Repositories;
using backend.src.Services;
using Microsoft.EntityFrameworkCore;

namespace backend.src.Handlers.Events;

public class RequestEventRemovalRequest
{
    public Guid EventId { get; set; }
}

public class RequestEventRemovalResponse
{
    public Guid EventId { get; set; }
    public bool IsPendingRemoval { get; set; }
    public int RemoveCount { get; set; }
    public int KeepCount { get; set; }
    public int QuorumRequired { get; set; }
    public bool RemovedImmediately { get; set; }
}

public interface IRequestEventRemovalHandler
{
    Task<RequestEventRemovalResponse> HandleAsync(RequestEventRemovalRequest request, CancellationToken ct);
}

public class RequestEventRemovalHandler : IRequestEventRemovalHandler
{
    private readonly IEventRepository _eventRepository;
    private readonly IEventApprovalRepository _eventApprovalRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly AppDbContext _context;

    public RequestEventRemovalHandler(
        IEventRepository eventRepository,
        IEventApprovalRepository eventApprovalRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService,
        IAuditLogRepository auditLogRepository,
        INotificationRepository notificationRepository,
        AppDbContext context)
    {
        _eventRepository = eventRepository;
        _eventApprovalRepository = eventApprovalRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
        _auditLogRepository = auditLogRepository;
        _notificationRepository = notificationRepository;
        _context = context;
    }

    public async Task<RequestEventRemovalResponse> HandleAsync(RequestEventRemovalRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var @event = await _eventRepository.GetByIdAsync(request.EventId);
        if (@event == null)
        {
            throw new BusinessRuleException("event_not_found", "Evento não encontrado.");
        }

        var members = await _groupMemberRepository.GetMembersByGroupAsync(@event.GroupId);
        EventRemovalRules.ValidateCanInitiateRemoval(@event, userId, members);

        var totalMembers = members.Count();
        var quorum = EventRemovalRules.CalculateQuorum(totalMembers);

        // Bypass: afetado removendo evento positivo sobre si → remove imediatamente
        if (EventRemovalRules.IsBypassRemoval(@event, userId))
        {
            await RemoveEventImmediatelyAsync(@event, userId);

            return new RequestEventRemovalResponse
            {
                EventId = @event.Id,
                IsPendingRemoval = false,
                RemoveCount = 1,
                KeepCount = 0,
                QuorumRequired = quorum,
                RemovedImmediately = true
            };
        }

        // Abre votação de remoção com prazo de 48h
        @event.IsPendingRemoval = true;
        @event.RemovalVoteDeadline = DateTime.UtcNow.AddHours(48);
        _eventRepository.Update(@event);

        // Criador auto-vota Keep
        _context.EventApprovals.Add(new EventApproval
        {
            EventId = @event.Id,
            UserId = @event.CreatedByUserId,
            VoteType = EventVoteType.Keep
        });

        // Iniciador vota Remove (se não for o criador)
        if (userId != @event.CreatedByUserId)
        {
            _context.EventApprovals.Add(new EventApproval
            {
                EventId = @event.Id,
                UserId = userId,
                VoteType = EventVoteType.Remove
            });
        }

        await _context.SaveChangesAsync(ct);

        var existingApprovals = await _eventApprovalRepository.GetByEventAsync(request.EventId);
        var removeCount = existingApprovals.Count(a => a.VoteType == EventVoteType.Remove);
        var keepCount = existingApprovals.Count(a => a.VoteType == EventVoteType.Keep);

        // Verifica se quorum já foi atingido imediatamente
        if (removeCount >= quorum && removeCount > keepCount)
        {
            await RemoveEventImmediatelyAsync(@event, userId);

            return new RequestEventRemovalResponse
            {
                EventId = @event.Id,
                IsPendingRemoval = false,
                RemoveCount = removeCount,
                KeepCount = keepCount,
                QuorumRequired = quorum,
                RemovedImmediately = true
            };
        }

        if (keepCount >= quorum && keepCount > removeCount)
        {
            @event.IsPendingRemoval = false;
            @event.RemovalVoteDeadline = null;
            _eventRepository.Update(@event);

            await _context.SaveChangesAsync(ct);

            var cancelledLog = AuditLogBuilder.EventRemovalCancelled(@event, userId);
            _auditLogRepository.Add(cancelledLog);
            await _context.SaveChangesAsync(ct);

            var cancelledNotifications = NotificationBuilder.BuildNotifications(cancelledLog, members, @event, null);
            _notificationRepository.AddRange(cancelledNotifications);
            await _context.SaveChangesAsync(ct);

            return new RequestEventRemovalResponse
            {
                EventId = @event.Id,
                IsPendingRemoval = false,
                RemoveCount = removeCount,
                KeepCount = keepCount,
                QuorumRequired = quorum,
                RemovedImmediately = false
            };
        }

        // Quórum não atingido ainda → votação continua aberta
        var initiatedLog = AuditLogBuilder.EventRemovalInitiated(@event, userId);
        _auditLogRepository.Add(initiatedLog);
        await _context.SaveChangesAsync(ct);

        var initiatedNotifications = NotificationBuilder.BuildNotifications(initiatedLog, members, @event, null);
        _notificationRepository.AddRange(initiatedNotifications);
        await _context.SaveChangesAsync(ct);

        return new RequestEventRemovalResponse
        {
            EventId = @event.Id,
            IsPendingRemoval = true,
            RemoveCount = removeCount,
            KeepCount = keepCount,
            QuorumRequired = quorum,
            RemovedImmediately = false
        };
    }

    private async Task RemoveEventImmediatelyAsync(Event @event, Guid performedByUserId)
    {
        // Reverte score se evento estava aprovado
        if (@event.Status == EventStatus.Approved)
        {
            var member = await _groupMemberRepository.GetByGroupAndUserAsync(@event.GroupId, @event.AffectedUserId);
            if (member != null)
            {
                var revertPoints = @event.Type == EventType.Negative ? -@event.Points : @event.Points;
                member.CurrentScore -= revertPoints;
                _groupMemberRepository.Update(member);
            }
        }

        _eventRepository.Remove(@event);
        await _context.SaveChangesAsync();

        var revertedPoints = @event.Status == EventStatus.Approved
            ? (@event.Type == EventType.Negative ? -@event.Points : @event.Points)
            : 0;
        var removedLog = AuditLogBuilder.EventRemovedByVote(@event, performedByUserId, revertedPoints);
        _auditLogRepository.Add(removedLog);
        await _context.SaveChangesAsync();
    }
}
