using backend.src.Entities.Base;

namespace backend.src.Entities;

public class Comment : Entity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid? EventId { get; set; }
    public Event? Event { get; set; }

    public Guid? SharedEventId { get; set; }
    public SharedEvent? SharedEvent { get; set; }

    public Guid? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }

    public string Content { get; set; } = string.Empty;

    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
