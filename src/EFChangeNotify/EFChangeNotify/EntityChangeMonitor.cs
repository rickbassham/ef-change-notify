using System;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Runtime.Caching;

namespace EFChangeNotify
{
    public class EntityChangeMonitor<TEntity, TDbContext> : ChangeMonitor,
        IDisposable
        where TDbContext : DbContext, new()
        where TEntity : class
    {
        private DbContext _context;
        private Expression<Func<TEntity, bool>> _query;
        private EntityChangeNotifier<TEntity, TDbContext> _changeNotifier;

        private string _uniqueId;

        public EntityChangeMonitor(Expression<Func<TEntity, bool>> query)
        {
            _context = new TDbContext();
            _query = query;
            _uniqueId = Guid.NewGuid().ToString();
            _changeNotifier = new EntityChangeNotifier<TEntity, TDbContext>(_query);

            _changeNotifier.Error += new EventHandler<NotifierErrorEventArgs>(_changeNotifier_Error);
            _changeNotifier.Changed += new EventHandler<EntityChangeEventArgs<TEntity>>(_changeNotifier_Changed);

            InitializationComplete();
        }

        void _changeNotifier_Error(object sender, NotifierErrorEventArgs e)
        {
            base.OnChanged(null);
        }

        void _changeNotifier_Changed(object sender, EntityChangeEventArgs<TEntity> e)
        {
            base.OnChanged(e.Results);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_changeNotifier != null)
                {
                    _changeNotifier.Dispose();
                    _changeNotifier = null;
                }
            }
        }

        public override string UniqueId
        {
            get { return _uniqueId; }
        }
    }
}
