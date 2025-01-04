using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainLayer.EntityModels
{
    public class NodeEdge
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey(nameof(Id))]
        public int NodeId { get; set; }
        public Node Node { get; set; }
        [ForeignKey(nameof(Id))]
        public int EdgeId { get; set; }
        public Edge Edge { get; set; }
        public ICollection<Node> Nodes { get; set; } = new List<Node>();
        public ICollection<Edge> Edges { get; set; } = new List<Edge>();
    }
}
