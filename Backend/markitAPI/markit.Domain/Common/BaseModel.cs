namespace markit.Domain.Common
{
    public abstract class BaseModel
    {
        public int Id { get; set; }

        // Auditoría
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public bool Enable { get; set; }
    }
}
