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

        // sub-classes should implement if they don't want callers to get the exception
        public virtual Task<T> AddAsync(T obj) => throw new NotImplementedException();
        public virtual Task<T> UpdateAsync(string id, T obj) => throw new NotImplementedException();

        public virtual Task<IEnumerable<T>> GetAllAsync() => Task.FromResult<IEnumerable<T>>(Objects.Values);
        public virtual Task<IEnumerable<T>> GetAllAsync(Func<T, bool> predicate) => Task.FromResult(Objects.Values.Where(predicate));
        public virtual Task<T> GetAsync(Func<T, bool> predicate) => Task.FromResult(Objects.Values.FirstOrDefault(predicate));
        public virtual Task SaveChangesAsync() => Task.FromResult(0);   // essentially a no-op

        public virtual Task<T> GetAsync(string id)
        {
            Objects.TryGetValue(id, out T obj);

            return Task.FromResult(obj);
        }
    }
}
