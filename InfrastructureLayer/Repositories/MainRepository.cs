using DomainLayer.EntityModels;
using DomainLayer.Interfaces;
using InfrastructureLayer.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayer.Repositories
{
    public class MainRepository : IRepository
    {
        private readonly RoutingServiceDb _serviceDb;
        private readonly ILogger logger;

        public MainRepository(RoutingServiceDb serviceDb)
        {
            _serviceDb = serviceDb;
            NodesService = new MainService<Node>(_serviceDb);
            EdgesService = new MainService<Edge>(_serviceDb);
            NodeEdgesService = new MainService<NodeEdge>(_serviceDb);
        }

        /// <summary>
        /// Gets or sets the service responsible for managing <see cref="Node"/> entities.
        /// </summary>
        /// <value>
        /// An instance of <see cref="IService{Node}"/> that provides operations for <see cref="Node"/> entities.
        /// </value>
        public IService<Node> NodesService { get; /*private*/ set; }

        /// <summary>
        /// Gets or sets the service responsible for managing <see cref="Edge"/> entities.
        /// </summary>
        /// <value>
        /// An instance of <see cref="IService{Edge}"/> that provides operations for <see cref="Edge"/> entities.
        /// </value>
        public IService<Edge> EdgesService { get; /*private*/ set; }

        /// <summary>
        /// Gets or sets the service responsible for managing <see cref="NodeEdge"/> entities.
        /// </summary>
        /// <value>
        /// An instance of <see cref="IService{NodeEdge}"/> that provides operations for <see cref="NodeEdge"/> entities.
        /// </value>
        public IService<NodeEdge> NodeEdgesService { get; /*private*/ set; }

        /// <summary>
        /// Asynchronously commits all changes made in the current context to the database.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CommitAsync() => await _serviceDb.SaveChangesAsync();

        public void Dispose() => _serviceDb.Dispose();
    }
}
