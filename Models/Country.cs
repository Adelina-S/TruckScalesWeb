using System.ComponentModel.DataAnnotations;

namespace TruckScalesWeb.Models
{
    public class Country
    {
        [Key]
        public int Id { get; set; }
        [Required]  
        public string ShortName { get; set; }
        [Required]
        public string FullName { get; set; }
        public int Sorter { get; set; }
    }
}
