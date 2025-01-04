using DomainLayer.EntityModels;
using DomainLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Routing_Service.Controllers.Routing
{
    [Route("api/[controller]")]
    [ApiController]
    public class RouterController(ICsvService csvService, NodeEdge graph, IRepository repository) : ControllerBase
    {
        private readonly ICsvService _csvService = csvService;
        private readonly NodeEdge _graph = graph;
        private readonly IRepository _repository = repository;

        /// <summary>
        /// UPLOAD NODES FILE (Csv, txt)
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("upload-nodes")]
        [Authorize(Policy = "ManagerPolicy")]
        public async Task<IActionResult> UploadNodes(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            IEnumerable<Node> nodes = _csvService.ReadNodesCSV(file.OpenReadStream());
            foreach (Node node in nodes)
            {
                await _repository.NodesService.AddAsync(node);
                await _repository.CommitAsync();
            }

            return Ok(nodes);
        }

        /// <summary>
        /// UPLOAD EDGES FILE (Csv, txt)
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("upload-edges")]
        [Authorize(Policy = "ManagerPolicy")]
        public async Task<IActionResult> UploadEdges(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            IEnumerable<Edge> edges = await _csvService.ReadEdgesCSV(file);
            foreach (Edge edge in edges)
            {
                await _repository.EdgesService.AddAsync(edge);
                await _repository.CommitAsync();
            }
            return Ok(edges);
        }
        /// <summary>
        /// CALCULATE THE DISTANCE BETWEEN TOW NODES 
        /// </summary>
        /// <param name="startNodeId"></param>
        /// <param name="endNodeId"></param>
        /// <returns></returns>
        [HttpGet("CalculateDistantce")]
        [Authorize(Roles ="User")]
        public async Task<IActionResult> CalculateDistantce(int startNodeId, int endNodeId)
        {
            Node startNode = await _repository.NodesService.GetByIdAsync(startNodeId);
            Node endNode = await _repository.NodesService.GetByIdAsync(endNodeId);
            var path = FindShortestPath(startNode, endNode);
            return Ok(path);
        }
        /// <summary>
        /// GET LIST OF ALL NODES IN THE SHORTEST PATH (using IDs)
        /// </summary>
        /// <param name="startNodeId"></param>
        /// <param name="endNodeId"></param>
        /// <returns></returns>
        [HttpGet("shortest-path using IDs of Cities")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetShortestPath(int startNodeId, int endNodeId)
        {
            var path = await GetShortestPathAsync(startNodeId, endNodeId);
            if (path == null || !path.Any())
            {
                return NotFound("No path found between the specified nodes.");
            }

            return Ok(path);
        }
        /// <summary>
        /// GET LIST OF ALL NODES IN THE SHORTEST PATH (using Names)
        /// </summary>
        /// <param name="startCityName"></param>
        /// <param name="endCityName"></param>
        /// <returns></returns>
        [HttpGet("shortest-path using Names of Cities")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetShortestPath(string startCityName, string endCityName)
        {
            try
            {
                var path = await GetShortestPathByCityNamesAsync(startCityName, endCityName);
                if (path == null || !path.Any())
                {
                    return NotFound("No path found between the specified cities.");
                }

                return Ok(path);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }




        /*/  -------------------------------------- Privat Methods -------------------------------------  /*/


        /// <summary>
        /// CALCULATE THE DISTANCE BETWEEN TOW NODES
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        /// <returns>The calculated distance of type double.</returns>
        private double FindShortestPath(Node startNode, Node endNode)
        {
            double minDistance = double.MaxValue;
            return FindShortestPathRecursive(startNode, endNode, new List<Node>(), 0, ref minDistance);
        }

        /// <summary>
        /// Recursively finds the shortest path between the current node and the end node using Depth-First Search (DFS).
        /// Updates the shortest distance if a valid path is found.
        /// </summary>
        private double FindShortestPathRecursive(Node currentNode, Node endNode, List<Node> visitedNodes, double currentDistance, ref double theShortestDistance)
        {
            foreach (Node node in _repository.NodesService.GetAllAsync().GetAwaiter().GetResult())
            {
                _graph.Nodes.Add(node);
            }

            if (currentNode.Id == endNode.Id)
            {
                theShortestDistance = Math.Min(theShortestDistance, currentDistance);
                return theShortestDistance;
            }

            visitedNodes.Add(currentNode);

            var edges = _repository.EdgesService.GetAllAsync().GetAwaiter().GetResult();


            foreach (var edge in edges.Where(e => e.startNode.Id == currentNode.Id).ToList())
            {
                var nextNode = edge.endNode;

                /// Avoid cycles by checking if the node has already been visited
                if (!visitedNodes.Contains(nextNode))
                {
                    FindShortestPathRecursive(nextNode, endNode, new List<Node>(visitedNodes), currentDistance + GetEdgeDistance(edge), ref theShortestDistance);
                }
            }

            return theShortestDistance;
        }

        /// <summary>
        /// Calculates the distance between two nodes connected by an edge.
        /// Utilizes the latitude and longitude of the start and end nodes to compute the distance.
        /// </summary>
        /// <param name="edge">The edge connecting the two nodes.</param>
        /// <returns>The calculated distance between the start and end nodes of the edge.</returns>
        private double GetEdgeDistance(Edge edge)
        {
            return CalculateDistance(edge.startNode.Latitude, edge.startNode.Longitude, edge.endNode.Latitude, edge.endNode.Longitude);
        }

        /// <summary>
        /// Calculates the great-circle distance between two geographical points specified by latitude and longitude.
        /// Uses the Haversine formula to compute the distance in kilometers.
        /// </summary>
        /// <param name="lat1">Latitude of the first point in decimal degrees.</param>
        /// <param name="lon1">Longitude of the first point in decimal degrees.</param>
        /// <param name="lat2">Latitude of the second point in decimal degrees.</param>
        /// <param name="lon2">Longitude of the second point in decimal degrees.</param>
        /// <returns>The distance between the two points in kilometers.</returns>
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in kilometers
            double dLat = DegreesToRadians(lat2 - lat1);
            double dLon = DegreesToRadians(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        /// <summary>
        /// Asynchronously finds the shortest path between two nodes in a graph using Dijkstra's algorithm.
        /// Retrieves all nodes and edges from the repository, constructs the graph, and calculates the shortest path.
        /// </summary>
        /// <param name="startNodeId">The ID of the starting node.</param>
        /// <param name="endNodeId">The ID of the ending node.</param>
        /// <returns>A task that represents the asynchronous operation, containing a list of nodes representing the shortest path, or null if no path is found.</returns>
        private async Task<List<Node>> GetShortestPathAsync(int startNodeId, int endNodeId)
        {
            var nodes = await _repository.NodesService.GetAllAsync();
            var edges = await _repository.EdgesService.GetAllAsync();

            var graph = new Dictionary<Node, List<Edge>>();

            foreach (var edge in edges)
            {
                if (!graph.ContainsKey(edge.startNode))
                {
                    graph[edge.startNode] = new List<Edge>();
                }
                graph[edge.startNode].Add(edge);
            }

            /// Initialize distances and previous nodes
            var distances = new Dictionary<Node, double>();
            var previous = new Dictionary<Node, Node>();
            var priorityQueue = new List<Node>();

            foreach (var node in nodes)
            {
                distances[node] = double.MaxValue;
                previous[node] = null;
                priorityQueue.Add(node);
            }

            distances[nodes.FirstOrDefault(n => n.Id == startNodeId)] = 0;

            while (priorityQueue.Count > 0)
            {
                /// Get the node with the smallest distance
                var currentNode = priorityQueue.OrderBy(n => distances[n]).First();
                priorityQueue.Remove(currentNode);

                /// If we reached the end node, we can reconstruct the path
                if (currentNode.Id == endNodeId)
                {
                    return ReconstructPath(previous, currentNode);
                }

                if (graph.ContainsKey(currentNode))
                {
                    foreach (var edge in graph[currentNode])
                    {
                        var neighbor = edge.endNode;
                        var alt = distances[currentNode] + 1;

                        if (alt < distances[neighbor])
                        {
                            distances[neighbor] = alt;
                            previous[neighbor] = currentNode;
                        }
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Reconstructs the path from the start node to the end node using the previous node mapping.
        /// Traverses backward from the end node to the start node, building the path list.
        /// </summary>
        /// <param name="previous">A dictionary mapping each node to its previous node in the path.</param>
        /// <param name="endNode">The end node from which to reconstruct the path.</param>
        /// <returns>A list of nodes representing the path from the start node to the end node in the correct order.</returns>
        private List<Node> ReconstructPath(Dictionary<Node, Node> previous, Node endNode)
        {
            var path = new List<Node>();
            var currentNode = endNode;

            while (currentNode != null)
            {
                path.Add(currentNode);
                currentNode = previous[currentNode];
            }

            path.Reverse();
            return path;
        }


        /// <summary>
        /// Asynchronously finds the shortest path between two cities specified by their names using Dijkstra's algorithm.
        /// Retrieves all nodes and edges from the repository, identifies the start and end nodes by city name,
        /// constructs the graph, and calculates the shortest path.
        /// </summary>
        /// <param name="startCityName">The name of the starting city.</param>
        /// <param name="endCityName">The name of the ending city.</param>
        /// <returns>A task that represents the asynchronous operation, containing a list of nodes representing the shortest path,
        /// or null if no path is found. Throws an ArgumentException if one or both city names are invalid or do not exist.</returns>
        private async Task<List<Node>> GetShortestPathByCityNamesAsync(string startCityName, string endCityName)
        {
            var nodes = await _repository.NodesService.GetAllAsync();
            var edges = await _repository.EdgesService.GetAllAsync();

            /// Find the start and end nodes by city name
            var startNode = nodes.FirstOrDefault(n => n.CityName.Equals(startCityName, StringComparison.OrdinalIgnoreCase));
            var endNode = nodes.FirstOrDefault(n => n.CityName.Equals(endCityName, StringComparison.OrdinalIgnoreCase));

            if (startNode == null || endNode == null)
            {
                throw new ArgumentException("One or both city names are invalid or not exists.");
            }

            /// Create a dictionary to hold the graph
            Dictionary<Node, List<Edge>> graph = new Dictionary<Node, List<Edge>>();

            foreach (var edge in edges)
            {
                if (!graph.ContainsKey(edge.startNode))
                {
                    graph[edge.startNode] = new List<Edge>();
                }
                graph[edge.startNode].Add(edge);
            }

            /// Initialize distances and previous nodes
            var distances = new Dictionary<Node, double>();
            var previous = new Dictionary<Node, Node>();
            var priorityQueue = new List<Node>();

            foreach (var node in nodes)
            {
                distances[node] = double.MaxValue;
                previous[node] = null;
                priorityQueue.Add(node);
            }

            distances[startNode] = 0;

            while (priorityQueue.Count > 0)
            {
                /// Get the node with the smallest distance
                var currentNode = priorityQueue.OrderBy(n => distances[n]).First();
                priorityQueue.Remove(currentNode);

                /// If we reached the end node, we can reconstruct the path
                if (currentNode.Id == endNode.Id)
                {
                    return ReconstructPath(previous, currentNode);
                }

                if (graph.ContainsKey(currentNode))
                {
                    foreach (var edge in graph[currentNode])
                    {
                        var neighbor = edge.endNode;
                        var alt = distances[currentNode] + 1;

                        if (alt < distances[neighbor])
                        {
                            distances[neighbor] = alt;
                            previous[neighbor] = currentNode;
                        }
                    }
                }
            }
            return null;
        }
    }
}
