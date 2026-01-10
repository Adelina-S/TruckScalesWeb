using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TruckScalesWeb.Models
{
    public class Car
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string GosNumber { get; set; }
        [NotMapped]
        public string GosNumberFull => $"{GosNumber}({Country.ShortName})";
        public string Model { get; set; }
        [Required]
        public Country Country { get; set; }
        [NotMapped]
        public string CountryName { get => Country.FullName; }
        [Required]
        public CarType CarType { get; set; }
        [NotMapped]
        public string CarTypeName { get => CarType.Name; }
        public bool IsActive { get; set; }  
        public DateTime? LastUsedTime { get; set; }
        [NotMapped]
        public string LastUsedTimeString
        {
            get
            {
                if (LastUsedTime is null) return "Не использовалось";
                else
                {
                    var date = DateOnly.FromDateTime(LastUsedTime.Value);
                    if (date == DateOnly.FromDateTime(DateTime.Now)) return LastUsedTime.Value.ToString("HH:mm");
                    else if (date == DateOnly.FromDateTime(DateTime.Now + TimeSpan.FromDays(1))) return "Вчера";
                    else return (LastUsedTime.Value.ToString("dd.MM.yyyy"));
                }
                   
            }
        }
        [NotMapped]
        public int Position { get; set; }
        [NotMapped]
        public string Description { get; set; }
        [NotMapped]
        public string FullName => $"{CarType.Name} {GosNumberFull}{(string.IsNullOrEmpty(Model) ? "" : " "+Model)}";
    }
}
