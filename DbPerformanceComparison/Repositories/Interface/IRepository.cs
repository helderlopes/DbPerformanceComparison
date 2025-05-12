using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbPerformanceComparison.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<int> AddAsync(T entity);
        Task AddManyAsync(IEnumerable<T> entities);
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<bool> UpdateAsync(T entity);
        Task<bool> DeleteAsync(int id);
    }
}
