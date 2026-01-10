using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TruckScalesWeb.Models
{
    public class Photo
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public Guid OneWeighingId { get; set; }
        public OneWeighing OneWeighing { get; set; }
        public bool IsNormal { get; set; }
        public PhotoContent? PhotoContent { get; set; }
    }

    public class PhotoContent
    {
        [Key]
        public Guid PhotoId { get; set; } 
        [Required]
        public byte[] ByteArray { get; set; }
        [ForeignKey("PhotoId")]
        public Photo Photo { get; set; }
    }
}
