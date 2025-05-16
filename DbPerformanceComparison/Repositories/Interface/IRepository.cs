using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbPerformanceComparison.Repositories.Interfaces
{
    public interface IRepository<T, TKey>
    {
        Task AddAsync(T entity);
        Task AddManyAsync(IEnumerable<T> entities);
        Task<T?> GetByIdAsync(TKey id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<bool> UpdateAsync(T entity, TKey id);
        Task<bool> DeleteAsync(TKey id);
    }
}
