using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hokanson.JottoRepository
{
    public interface IRepository<T>
    {
        Task<T> AddAsync(T obj);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsync(Func<T,bool> predicate);
        Task<T> GetAsync(string id);
        Task<T> GetAsync(Func<T, bool> predicate);
        Task<T> UpdateAsync(string id, T obj);
        Task SaveChangesAsync();
    }
}
