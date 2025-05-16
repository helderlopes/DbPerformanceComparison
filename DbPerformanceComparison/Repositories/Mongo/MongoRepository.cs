using DbPerformanceComparison.Infrastructure.Mongo;
using DbPerformanceComparison.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbPerformanceComparison.Repositories.Mongo
{
    public class MongoRepository<T> : IRepository<T, string> where T : class
    {
        private readonly IMongoCollection<T> _collection;

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

        public async Task<bool> DeleteAsync(string id)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
            DeleteResult result = await _collection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _collection.Find(Builders<T>.Filter.Empty).ToListAsync();
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateAsync(T entity, string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
            ReplaceOneResult result = await _collection.ReplaceOneAsync(filter, entity);
            return result.ModifiedCount > 0;
        }
    }
}
