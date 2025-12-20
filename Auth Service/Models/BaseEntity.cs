namespace Auth.Models
{
    public class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { set; get; } = DateTime.Now;
        public DateTime? UpdatedAt { set; get; } = DateTime.Now;
        public bool IsDeleted { set; get; } = false;

    }
}
