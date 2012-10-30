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

        public event EventHandler<EntityChangeEventArgs<TEntity>> Changed;
        public event EventHandler<NotifierErrorEventArgs> Error;

        public EntityChangeNotifier(Expression<Func<TEntity, bool>> query)
        {
            _context = new TDbContext();
            _query = query;

            SafeCountDictionary.Increment(_context.Database.Connection.ConnectionString, x => { SqlDependency.Start(x); });

            RegisterNotification();
        }

        private void RegisterNotification()
        {
            _context = new TDbContext();

            string sql = GetSql();

            using (SqlConnection connection = new SqlConnection(_context.Database.Connection.ConnectionString))
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
            return _context.Set<TEntity>().Where(_query) as DbQuery<TEntity>;
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

        private void _sqlDependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Subscribe || e.Info == SqlNotificationInfo.Error)
            {
                OnError(
                    new NotifierErrorEventArgs
                    {
                        Reason = e,
                        Sql = GetSql()
                    }
                );
            }
            else
            {
                var args =
                    new EntityChangeEventArgs<TEntity>
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

        public void Dispose()
        {
            SafeCountDictionary.Decrement(_context.Database.Connection.ConnectionString, x => { SqlDependency.Stop(x); });
        }
    }
}
