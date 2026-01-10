using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TruckScalesWeb.Models
{
    public class Material
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        public bool CanBeSingle { get; set; }
        public bool CanBeTransit { get; set; }
        public bool IsActive { get; set; }
        public int Sorter { get; set; }
    }
}
