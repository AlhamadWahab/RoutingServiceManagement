using DomainLayer.EntityModels;
using DomainLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Routing_Service.Controllers.Entities
{
    [Route("api/[controller]")]
    [ApiController]
    public class NodeController(IRepository repository) : ControllerBase
    {
        private readonly IRepository _repository = repository;

        /// <summary>
        /// this method to get all nodes, that in the Database stored
        /// </summary>
        /// <returns>List of nodes of type Node</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllNodes()
        {
            IEnumerable<Node> nodes = await _repository.NodesService.GetAllAsync();
            return Ok(nodes);
        }

        /// <summary>
        /// get a special node with an id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>object of type Node</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetNodeById(int id)
        {
            if (id == 0) { return NotFound(new { Message = $"Entity with ID {id} not found." }); }
            Node node = await _repository.NodesService.GetByIdAsync(id);
            return Ok(node);
        }

        /// <summary>
        /// Create a new node that passes through the body.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>

        [HttpPost]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> CreateNewNode([FromBody] Node node)
        {
            if (node == null) { return BadRequest(); }
            await _repository.NodesService.AddAsync(node);
            await _repository.CommitAsync();
            return Ok(node);
        }

        /// <summary>
        /// Edit a node, values passes through the body.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="node"></param>
        /// <returns></returns>

        [HttpPut("{id}")]
        [Authorize(Policy = "ManagerPolicy")]
        public async Task<IActionResult> UpdateExistsNode(int id, [FromBody] Node node)
        {
            if (id == 0 || node == null)
            {
                return NotFound();
            }

            Node oldNode = await _repository.NodesService.GetByIdAsync(id);
            //if (node == null)
            //{
            //    return NotFound();
            //}

            oldNode.CityName = node.CityName;
            oldNode.Longitude = node.Longitude;
            oldNode.Latitude = node.Latitude;

            await _repository.CommitAsync();
            return Ok(node);
        }

        /// <summary>
        /// delete a Node object from Database.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpDelete("{id}")]
        [Authorize(Policy = "ManagerPolicy")]
        public async Task<IActionResult> DeleteExistsNode(int id)
        {
            await _repository.NodesService.DeleteByIdAsync(id);
            return NoContent();
        }
    }
}
