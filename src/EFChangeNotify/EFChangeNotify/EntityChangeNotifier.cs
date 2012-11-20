using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace EFChangeNotify
{
    public class EntityChangeNotifier<TEntity, TDbContext>
        : IDisposable
        where TDbContext : DbContext, new()
        where TEntity : class
    {
        private DbContext _context;
        private Expression<Func<TEntity, bool>> _query;
        private string _connectionString;

        public event EventHandler<EntityChangeEventArgs<TEntity>> Changed;
        public event EventHandler<NotifierErrorEventArgs> Error;

        public EntityChangeNotifier(Expression<Func<TEntity, bool>> query)
        {
            _context = new TDbContext();
            _query = query;
            _connectionString = _context.Database.Connection.ConnectionString;

            SafeCountDictionary.Increment(_connectionString, x => { SqlDependency.Start(x); });

            RegisterNotification();
        }

        private void RegisterNotification()
        {
            _context = new TDbContext();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = GetCommand())
                {
                    command.Connection = connection;
                    connection.Open();

                    var sqlDependency = new SqlDependency(command);
                    sqlDependency.OnChange += new OnChangeEventHandler(_sqlDependency_OnChange);

                    // NOTE: You have to execute the command, or the notification will never fire.
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                    }
                }
            }
        }

        private string GetSql()
        {
            var q = GetCurrent();

            return q.ToTraceString();
        }

        private SqlCommand GetCommand()
        {
            var q = GetCurrent();

            return q.ToSqlCommand();
        }

        private DbQuery<TEntity> GetCurrent()
        {
            var query = _context.Set<TEntity>().Where(_query) as DbQuery<TEntity>;

            return query;
        }

        private void _sqlDependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            if (_context == null)
                return;

            if (e.Type == SqlNotificationType.Subscribe || e.Info == SqlNotificationInfo.Error)
            {
                var args = new NotifierErrorEventArgs
                {
                    Reason = e,
                    Sql = GetCurrent().ToString()
                };

                OnError(args);
            }
            else
            {
                var args = new EntityChangeEventArgs<TEntity>
                {
                    Results = GetCurrent(),
                    ContinueListening = true
                };

                OnChanged(args);

                if (args.ContinueListening)
                {
                    RegisterNotification();
                }
            }
        }

        protected virtual void OnChanged(EntityChangeEventArgs<TEntity> e)
        {
            if (Changed != null)
            {
                Changed(this, e);
            }
        }

        protected virtual void OnError(NotifierErrorEventArgs e)
        {
            if (Error != null)
            {
                Error(this, e);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                SafeCountDictionary.Decrement(_connectionString, x => { SqlDependency.Stop(x); });

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
