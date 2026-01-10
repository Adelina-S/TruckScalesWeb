using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TruckScalesWeb.Models
{
    public class User
    {
        public Guid Id { get; set; }
        [Required]
        public string Account { get; set; } = string.Empty; // Windows login (DOMAIN\UserName)
        [Required]
        public string DisplayName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // Для резервного входа
        public string HasPassword { get => string.IsNullOrEmpty(PasswordHash) ? "Не задан" : "Задан"; }
        public bool IsActive { get; set; } = true;
        public string IsActiveString { get => IsActive ? "Да" : "Нет"; }
        public DateTime LastLogin { get; set; }
        public string LastLoginString { get => LastLogin.ToString("dd.MM.yyyy HH:mm"); }
        public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
        public string RolesString { get => string.Join("; ", Roles.Select(t => t.Name)); }
        public bool IsAdministrator => Roles.Any(t => t.Name == "Администратор");
    }
}
