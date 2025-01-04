using System.ComponentModel.DataAnnotations;

namespace DomainLayer.EntityModels
{
    public class Edge
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public Node startNode { get; set; }
        [Required]
        public Node endNode { get; set; }
    }
}
