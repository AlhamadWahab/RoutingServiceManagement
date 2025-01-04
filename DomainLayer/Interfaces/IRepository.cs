using DomainLayer.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Interfaces
{
    public interface IRepository : IDisposable
    {
        public IService<Node> NodesService { get; }
        public IService<Edge> EdgesService { get; }
        public IService<NodeEdge> NodeEdgesService { get; }
        public Task CommitAsync();
    }
}
