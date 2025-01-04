using DomainLayer.EntityModels;
using Microsoft.AspNetCore.Http;

namespace DomainLayer.Interfaces
{
    public interface ICsvService
    {
        /// <summary>
        /// Reads nodes from a CSV file and returns them as an enumerable collection of Node objects.
        /// </summary>
        /// <param name="file">The input stream representing the CSV file containing node data.</param>
        /// <returns>An IEnumerable<Node> containing the records parsed from the CSV file.</returns>
        public IEnumerable<Node> ReadNodesCSV(Stream file);

        /// <summary>
        /// Asynchronously reads edges from a CSV file and returns them as an enumerable collection of Edge objects.
        /// </summary>
        /// <param name="file">The IFormFile representing the CSV file containing edge data.</param>
        /// <returns>A Task that represents the asynchronous operation, with a result of IEnumerable<Edge> containing the edges parsed from the CSV file.</returns>
        public Task<IEnumerable<Edge>> ReadEdgesCSV(IFormFile file);
    }
}
