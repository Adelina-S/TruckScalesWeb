using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TruckScalesWeb.Models
{
    public class Weighing
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Talon { get; set; }
        public WeightTypes WeightType { get; set; }
        public List<OneWeighing> OneWeighings { get; set; } = new List<OneWeighing>();
        public List<Car> Cars { get; set; } = new List<Car>();
        [ForeignKey(nameof(Material))]
        public Guid? MaterialId { get; set; }
        public Material Material { get; set; }  
        public int? Tare { get; set; }
        public int? Netto { get; set; }  
        public int? Brutto { get; set; }
        public DateTime? CloseDate { get; set; }
        [ForeignKey(nameof(Operator))]
        public Guid OperatorId { get; set; }
        [Required]
        public User Operator { get; set; }
        public bool IsDeleted { get;set; }

        [NotMapped]
        public string CarsList => string.Join("\n", Cars.Select(t => t.FullName));
        [NotMapped]
        public string FirstWeightMode => OneWeighings.First().IsEmpty ? "Порожнее" : "Груженоё";
        [NotMapped]
        public string Weight => OneWeighings.First().Weight.ToString("0 кг");
        [NotMapped]
        public string MaterialString => Material?.Name ?? "Не задан";
        [NotMapped]
        public string FirstWeightDate
        {
            get
            {
                OneWeighing first = OneWeighings.First();
                DateTime date = first.WeightTime;
                return Common.GetDateString(date);
            }
        }
        [NotMapped]
        public string TalonShort => Talon.Substring(4);
        [NotMapped]
        public string IsClosed => CloseDate.HasValue ? "Да" : "Нет";
        [NotMapped]
        public string OperatorName => Operator.DisplayName;
        [NotMapped]
        public string WeightDateString
        {
            get
            {
                DateTime first = OneWeighings.First().WeightTime;
                if (CloseDate.HasValue == false) return Common.GetDateString(first);
                DateTime second = CloseDate.Value;
                if (first.Date == second.Date)
                    return Common.GetDateString(second);
                else if (OneWeighings.First().IsEmpty)
                    return $"П: {Common.GetDateString(first)} \nГ: {Common.GetDateString(second)}";
                else
                    return $"П: {Common.GetDateString(second)} \nГ: {Common.GetDateString(first)}";
            }
        }
        [NotMapped]
        public string WeightDateStringLong
        {
            get
            {
                DateTime first = OneWeighings.First().WeightTime;
                if (CloseDate.HasValue == false) return first.ToString("dd.MM.yyyy HH:mm");
                DateTime second = CloseDate.Value;
                if (OneWeighings.First().IsEmpty)
                    return $"П: {first.ToString("dd.MM.yyyy HH:mm")} \nГ: {second.ToString("dd.MM.yyyy HH:mm")}";
                else
                    return $"П: {second.ToString("dd.MM.yyyy HH:mm")} \nГ: {first.ToString("dd.MM.yyyy HH:mm")}";
            }
        }
    }
}
