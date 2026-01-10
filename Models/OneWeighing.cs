using System.ComponentModel.DataAnnotations;

namespace TruckScalesWeb.Models
{
    public class OneWeighing
    {
        [Key]
        public Guid Id { get; set; }
        public bool IsEmpty { get; set; }
        public int Weight { get; set; }
        public DateTime WeightTime { get; set; }
        public Guid OperatorId { get; set; } 
        [Required]
        public User Operator { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsManual { get; set; }
        [Required]
        public Guid WeighingId { get; set; }
        public ICollection<Photo> Photos { get; set; } = new List<Photo>();

    }
}
