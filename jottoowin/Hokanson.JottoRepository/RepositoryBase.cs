using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hokanson.JottoRepository
{
    public abstract class RepositoryBase<T> : IRepository<T>
    {
        protected static readonly ConcurrentDictionary<string, T> Objects = new ConcurrentDictionary<string, T>();  // Id -> object

        // sub-classes must implement
        public abstract Task<T> AddAsync(T obj);
        public abstract Task<T> UpdateAsync(string id, T obj);

        public virtual Task<IEnumerable<T>> GetAllAsync()
        {
            return Task.FromResult(Objects.Values.AsEnumerable());
        }

        public virtual Task<IEnumerable<T>> GetAllAsync(Func<T, bool> predicate)
        {
            return Task.FromResult(Objects.Values.Where(predicate));
        }

        public virtual Task<T> GetAsync(string id)
        {
            T obj;
            Objects.TryGetValue(id, out obj);

            return Task.FromResult(obj);
        }

        public virtual Task<T> GetAsync(Func<T, bool> predicate)
        {
            return Task.FromResult(Objects.Values.FirstOrDefault(predicate));
        }

        public virtual Task SaveChangesAsync()
        {
            return Task.FromResult(0);
        }
    }
}
