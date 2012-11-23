using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Caching;

namespace EFChangeNotify
{
    public class EntityCache<TEntity, TDbContext>
        : IDisposable
        where TDbContext : DbContext, new()
        where TEntity : class
    {
        private DbContext _context;
        private Expression<Func<TEntity, bool>> _query;

        private string _cacheKey = Guid.NewGuid().ToString();

        public EntityCache(Expression<Func<TEntity, bool>> query)
        {
            _context = new TDbContext();
            _query = query;
        }

        private IEnumerable<TEntity> GetCurrent()
        {
            var query = _context.Set<TEntity>().Where(_query);

            return query;
        }

        private IEnumerable<TEntity> GetResults()
        {
            List<TEntity> value = MemoryCache.Default[_cacheKey] as List<TEntity>;

            if (value == null)
            {
                value = GetCurrent().ToList();

                var changeMonitor = new EntityChangeMonitor<TEntity, TDbContext>(_query);

                CacheItemPolicy policy = new CacheItemPolicy();

                policy.ChangeMonitors.Add(changeMonitor);

                MemoryCache.Default.Add(_cacheKey, value, policy);

                Console.WriteLine("From Database...");
            }
            else
            {
                Console.WriteLine("From Cache...");
            }

            return value;
        }

        public IEnumerable<TEntity> Results
        {
            get
            {
                return GetResults();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_context != null)
                {
                    _context.Dispose();
                    _context = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
