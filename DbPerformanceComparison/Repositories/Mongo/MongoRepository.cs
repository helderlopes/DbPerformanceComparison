using DbPerformanceComparison.Infrastructure.Mongo;
using DbPerformanceComparison.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbPerformanceComparison.Repositories.Mongo
{
    public class MongoRepository<T> : IRepository<T> where T : class
    {
        public string DatabaseName => "MongoDB";
        private readonly IMongoCollection<T> _collection;

        private Guid GetEntityId(T entity)
        {
            var prop = typeof(T).GetProperty("Id");
            if (prop == null)
                throw new InvalidOperationException("Entity does not have an Id property.");
            return (Guid)prop.GetValue(entity)!;
        }

        public MongoRepository(MongoService mongoService)
        {
            _collection = mongoService.GetCollection<T>();
        }

        public async Task AddAsync(T entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task AddManyAsync(IEnumerable<T> entities)
        {
            await _collection.InsertManyAsync(entities);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var filter = Builders<T>.Filter.Eq("Id", id);
            DeleteResult result = await _collection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _collection.Find(Builders<T>.Filter.Empty).ToListAsync();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            var filter = Builders<T>.Filter.Eq("Id", id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            Guid id = GetEntityId(entity);
            var filter = Builders<T>.Filter.Eq("Id", id);
            ReplaceOneResult result = await _collection.ReplaceOneAsync(filter, entity);
            return result.ModifiedCount > 0;
        }
    }
}
