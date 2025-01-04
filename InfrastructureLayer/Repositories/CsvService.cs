using CsvHelper;
using DomainLayer.EntityModels;
using DomainLayer.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayer.Repositories
{
    public class CsvService(IRepository repository) : ICsvService
    {
        private readonly IRepository _repository = repository;

        /// <summary>
        /// Reads nodes from a CSV file and returns them as an enumerable collection of Node objects.
        /// </summary>
        /// <param name="file">The input stream representing the CSV file containing node data.</param>
        /// <returns>An IEnumerable<Node> containing the records parsed from the CSV file.</returns>
        public IEnumerable<Node> ReadNodesCSV(Stream file)
        {
            var reader = new StreamReader(file);
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<Node>().ToList();
            return records;
        }

        /// <summary>
        /// Asynchronously reads edges from a CSV file and returns them as an enumerable collection of Edge objects.
        /// </summary>
        /// <param name="file">The IFormFile representing the CSV file containing edge data.</param>
        /// <returns>A Task that represents the asynchronous operation, with a result of IEnumerable<Edge> containing the edges parsed from the CSV file.</returns>
        /// <remarks>
        /// This method reads the provided CSV file, skipping the header line, and processes each subsequent line to create Edge objects.
        /// Each edge is constructed using the first column as its Id and the second and third columns as references to start and end nodes, respectively.
        /// 
        /// The method retrieves all existing nodes from the repository to match against the city names provided in the CSV file. 
        /// If a line does not contain at least three values, it is skipped to avoid errors. 
        /// 
        /// It is assumed that the CSV file is properly formatted and that the Node class contains a property called CityName 
        /// that can be matched with the values in the second and third columns of the CSV. 
        /// 
        /// Note: This method may throw exceptions if the Id cannot be parsed to an integer or if the file is not in the expected format.
        /// </remarks>
        public async Task<IEnumerable<Edge>> ReadEdgesCSV(IFormFile file)

        {
            using var reader = new StreamReader(file.OpenReadStream());
            _ = await reader.ReadLineAsync() ?? ""; // Skip header line
            List<Edge> edges = new List<Edge>();
            IEnumerable<Node> nodes = await _repository.NodesService.GetAllAsync();

            while (!reader.EndOfStream)
            {
                string line = await reader.ReadLineAsync() ?? "";
                string[] values = line.Split(',');

                if (values.Length < 3)
                {
                    // Handle the case where the line does not have enough columns
                    continue;
                }

                Edge edge = new Edge
                {
                    Id = int.Parse(values[0])
                };
                foreach (Node node in nodes)
                {
                    if (values[1] == node.CityName)
                    {
                        edge.startNode = node;
                    }
                }

                foreach (Node node in nodes)
                {
                    if (values[2] == node.CityName)
                    {
                        edge.endNode = node;
                    }
                }
                edges.Add(edge);

            }
            return edges;
        }
    }
}
