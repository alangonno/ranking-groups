using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.Events;

public class DeleteEventRequest
{
    public Guid EventId { get; set; }
}

public class DeleteEventResponse
{
    public bool Success { get; set; }
}

public interface IDeleteEventHandler
{
    Task<DeleteEventResponse> HandleAsync(DeleteEventRequest request, CancellationToken ct);
}

public class DeleteEventHandler : IDeleteEventHandler
{
    private readonly IEventRepository _eventRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly AppDbContext _context;

    public DeleteEventHandler(
        IEventRepository eventRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService,
        AppDbContext context)
    {
        _eventRepository = eventRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
        _context = context;
    }

    public async Task<DeleteEventResponse> HandleAsync(DeleteEventRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var @event = await _eventRepository.GetByIdAsync(request.EventId);
        if (@event == null)
        {
            throw new BusinessRuleException("event_not_found", "Evento não encontrado.");
        }

        var members = await _groupMemberRepository.GetMembersByGroupAsync(@event.GroupId);
        GroupPermissionRules.ValidateUserCanInteract(userId, @event.GroupId, members);

        EventRules.ValidateCanEdit(@event.Status);
        EventRules.ValidateAffectedUserCannotModify(@event.AffectedUserId, userId, @event.Type);

        if (@event.CreatedByUserId != userId)
        {
            throw new BusinessRuleException("not_event_creator", "Apenas o criador do evento pode excluí-lo.");
        }

        _eventRepository.Remove(@event);
        await _context.SaveChangesAsync(ct);

        return new DeleteEventResponse { Success = true };
    }
}
