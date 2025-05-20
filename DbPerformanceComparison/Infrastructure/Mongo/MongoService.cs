using MongoDB.Bson;
using MongoDB.Driver;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbPerformanceComparison.Infrastructure.Mongo
{
    public class MongoService
    {
        private readonly IMongoDatabase _database;

        public MongoService(string connectionString, string databaseName, bool dropCollections = false)
        {
            MongoClient client = new(connectionString);
            _database = client.GetDatabase(databaseName);
            if (dropCollections)
            {
                client.DropDatabase(databaseName);
            }
        }

        public IMongoCollection<T> GetCollection<T>()
        {
            return _database.GetCollection<T>(typeof(T).Name);
        }
    }
}
