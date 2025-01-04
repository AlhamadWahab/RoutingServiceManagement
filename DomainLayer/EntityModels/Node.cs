using System.ComponentModel.DataAnnotations;

namespace DomainLayer.EntityModels
{
    public class Node
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string CityName { get; set; }
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
    }
}
