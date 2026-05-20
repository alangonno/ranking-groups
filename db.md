# Entities

# Base Entity

```csharp
namespace Api.Entities.Base;

public abstract class Entity
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
```

---

# Enums

## GroupRole

```csharp
namespace Api.Common.Enums;

public enum GroupRole
{
    Owner = 1,
    Admin = 2,
    Member = 3
}
```

---

## EventStatus

```csharp
namespace Api.Common.Enums;

public enum EventStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4
}
```

---

## EventType

```csharp
namespace Api.Common.Enums;

public enum EventType
{
    Positive = 1,
    Negative = 2
}
```

---

## EventVoteType

```csharp
namespace Api.Common.Enums;

public enum EventVoteType
{
    Approve = 1,
    Reject = 2
}
```

---

# User

```csharp
using Api.Entities.Base;

namespace Api.Entities;

public class User : Entity
{
    public string Name { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public ICollection<Group> OwnedGroups { get; set; }
        = new List<Group>();

    public ICollection<GroupMember> GroupMembers { get; set; }
        = new List<GroupMember>();

    public ICollection<Event> CreatedEvents { get; set; }
        = new List<Event>();

    public ICollection<Event> AffectedEvents { get; set; }
        = new List<Event>();

    public ICollection<EventApproval> EventApprovals { get; set; }
        = new List<EventApproval>();

    public ICollection<SharedEvent> CreatedSharedEvents { get; set; }
    = new List<SharedEvent>();

    public ICollection<SharedEventParticipant> SharedEventParticipations { get; set; }
    = new List<SharedEventParticipant>();
}
```

---

# Group

```csharp
using Api.Entities.Base;

namespace Api.Entities;

public class Group : Entity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string InviteCode { get; set; } = string.Empty;

    public Guid CreatedByUserId { get; set; }

    public User CreatedByUser { get; set; } = null!;

    public ICollection<GroupMember> Members { get; set; }
        = new List<GroupMember>();

    public ICollection<Event> Events { get; set; }
        = new List<Event>();

    public ICollection<SharedEvent> SharedEvents { get; set; }
    = new List<SharedEvent>();
}
```

---

# GroupMember

```csharp
using Api.Common.Enums;
using Api.Entities.Base;

namespace Api.Entities;

public class GroupMember : Entity
{
    public Guid GroupId { get; set; }

    public Group Group { get; set; } = null!;

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public GroupRole Role { get; set; }

    public int CurrentScore { get; set; }
}
```

---

# Event

```csharp
using Api.Common.Enums;
using Api.Entities.Base;

namespace Api.Entities;

public class Event : Entity
{
    public Guid GroupId { get; set; }

    public Group Group { get; set; } = null!;

    public Guid CreatedByUserId { get; set; }

    public User CreatedByUser { get; set; } = null!;

    public Guid AffectedUserId { get; set; }

    public User AffectedUser { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Points { get; set; }

    public EventType Type { get; set; }

    public EventStatus Status { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime? RejectedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public ICollection<EventApproval> Approvals { get; set; }
        = new List<EventApproval>();
}
```

---

# EventApproval

```csharp
using Api.Common.Enums;
using Api.Entities.Base;

namespace Api.Entities;

public class EventApproval : Entity
{
    public Guid EventId { get; set; }

    public Event Event { get; set; } = null!;

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public EventVoteType VoteType { get; set; }
}
```

---

# AuditLog

```csharp
using Api.Entities.Base;

namespace Api.Entities;

public class AuditLog : Entity
{
    public string Action { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public Guid PerformedByUserId { get; set; }

    public User PerformedByUser { get; set; } = null!;

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }
}
```

---

# RefreshToken

```csharp
using Api.Entities.Base;

namespace Api.Entities;

public class RefreshToken : Entity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }
}
```

---

# Notification

```csharp
using Api.Entities.Base;

namespace Api.Entities;

public class Notification : Entity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsRead { get; set; }
}
```

---

# Relationship Summary

## User

Relacionamentos:

- owned groups
- group memberships
- created events
- affected events
- approvals
- notifications
- refresh tokens

---

## Group

Relacionamentos:

- members
- events

---

## GroupMember

Relacionamento N:N entre:

- users
- groups

Também contém:

- role
- current_score

---

## Event

Relacionamentos:

- group
- creator
- affected user
- approvals

---

## EventApproval

Representa votos de aprovação/rejeição.

---

# Regras Importantes

## Event

- pontos negativos iniciam Pending
- pontos positivos podem iniciar Approved
- eventos aprovados não devem ser editados

---

## EventApproval

- unique(event_id, user_id)
- affected user não vota
- criador não aprova sozinho

---

## GroupMember

- unique(group_id, user_id)

---

# Índices Recomendados

## Users

```text
email
username
```

---

## Groups

```text
invite_code
```

---

## GroupMembers

```text
group_id
user_id
(group_id, user_id) unique
```

---

## Events

```text
group_id
affected_user_id
created_by_user_id
status
created_at
```

---

## EventApprovals

```text
event_id
user_id
(event_id, user_id) unique
```


kakskask

# Nova Entidade

## SharedEvent

```csharp
using Api.Entities.Base;

namespace Api.Entities;

public class SharedEvent : Entity
{
    public Guid GroupId { get; set; }

    public Group Group { get; set; } = null!;

    public Guid CreatedByUserId { get; set; }

    public User CreatedByUser { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Points { get; set; }

    public bool IsClosed { get; set; }

    public ICollection<SharedEventParticipant> Participants { get; set; }
        = new List<SharedEventParticipant>();
}
```

---

# Nova Entidade

## SharedEventParticipant

```csharp
using Api.Entities.Base;

namespace Api.Entities;

public class SharedEventParticipant : Entity
{
    public Guid SharedEventId { get; set; }

    public SharedEvent SharedEvent { get; set; } = null!;

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;
}
```

---

# Índices Obrigatórios

## SharedEvents

```text
group_id
created_by_user_id
created_at
is_closed
```

---

## SharedEventParticipants

```text
shared_event_id
user_id
(shared_event_id, user_id) unique
```

---

# Regras de Banco

## SharedEvent

- apenas eventos positivos
- pontos obrigatoriamente maiores que zero
- pertence a apenas um grupo

---

# Regras de Integridade

## SharedEventParticipant

- usuário não participa duas vezes
- apenas membros do grupo participam
- não permitir participação em evento fechado



