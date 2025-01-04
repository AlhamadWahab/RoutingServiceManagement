using DomainLayer.Interfaces;
using InfrastructureLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Repositories
{
    public class MainService<T>(RoutingServiceDb serviceBb) : IService<T>, IDisposable where T : class
    {
        private readonly RoutingServiceDb _serviceDb = serviceBb;

        /// <summary>
        /// Asynchronously retrieves all entities of type <typeparamref name="T"/> from the database.
        /// Utilizes Entity Framework to fetch the data from the specified DbSet.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing an enumerable collection of entities of type <typeparamref name="T"/>.</returns>
        public async Task<IEnumerable<T>> GetAllAsync()
            => await _serviceDb.Set<T>().ToListAsync();

        /// <summary>
        /// Asynchronously retrieves an entity of type <typeparamref name="T"/> by its unique identifier.
        /// Throws an exception if the provided ID is zero or if the entity is not found.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation, containing the entity of type <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided ID is zero.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when an entity with the specified ID is not found.</exception>
        public async Task<T> GetByIdAsync(int id)
        {
            if (id == 0)
            {
                throw new ArgumentNullException(nameof(id));
            }

            T? entity = await _serviceDb.Set<T>().FindAsync(id);
            return entity ?? throw new KeyNotFoundException($"Entity with ID {id} not found.");
        }

        /// <summary>
        /// Asynchronously adds a new entity of type <typeparamref name="T"/> to the database.
        /// Throws an exception if the provided entity is null.
        /// </summary>
        /// <param name="entity">The entity of type <typeparamref name="T"/> to add to the database.</param>
        /// <returns>A task that represents the asynchronous operation, containing the added entity.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided entity is null.</exception>
        public async Task<T> AddAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity), "Entity cannot be null");
            await _serviceDb.Set<T>().AddAsync(entity);
            await _serviceDb.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Asynchronously updates an existing entity of type <typeparamref name="T"/> in the database.
        /// Retrieves the entity by its unique identifier, then sets its values to those of the provided entity.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to update.</param>
        /// <param name="entity">The entity containing updated values.</param>
        /// <returns>A task that represents the asynchronous operation, containing the updated entity.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when an entity with the specified ID is not found.</exception>
        public async Task<T> UpdateAsync(int id, T entity)
        {
            T CurrentEntity = await GetByIdAsync(id);
            _serviceDb.Entry(CurrentEntity).CurrentValues.SetValues(entity);
            await _serviceDb.SaveChangesAsync();
            return CurrentEntity;
        }

        /// <summary>
        /// Asynchronously deletes an entity of type <typeparamref name="T"/> from the database by its unique identifier.
        /// Retrieves the entity using the specified ID, removes it from the database, and saves the changes.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to delete.</param>
        /// <returns>A task that represents the asynchronous operation, containing the deleted entity.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when an entity with the specified ID is not found.</exception>
        public async Task<T> DeleteByIdAsync(int id)
        {
            T entity = await GetByIdAsync(id);
            _serviceDb.Set<T>().Remove(entity);
            await _serviceDb.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Releases the resources used by the current instance of the database context.
        /// This method is called to free up unmanaged resources and perform other cleanup operations.
        /// </summary>
        public void Dispose() => _serviceDb.Dispose();

    }
}
