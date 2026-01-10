using System.ComponentModel.DataAnnotations;

namespace TruckScalesWeb.Models
{
    public class CarType
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public bool CanBePrimary { get; set; }
        public bool CanBeSecondary { get; set; }
        public int Sorter { get; set; }
    }
}
