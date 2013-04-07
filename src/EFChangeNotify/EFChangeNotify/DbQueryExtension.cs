using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace EFChangeNotify
{
    public static class DbQueryExtension
    {
        public static ObjectQuery<T> ToObjectQuery<T>(this DbQuery<T> query)
        {
            var internalQuery = query.GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(field => field.Name == "_internalQuery")
                .Select(field => field.GetValue(query))
                .First();

            var objectQuery = internalQuery.GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(field => field.Name == "_objectQuery")
                .Select(field => field.GetValue(internalQuery))
                .Cast<ObjectQuery<T>>()
                .First();

            return objectQuery;
        }

        public static SqlCommand ToSqlCommand<T>(this DbQuery<T> query)
        {
            var command = new SqlCommand();
            command.CommandText = query.ToString();

            var objectQuery = query.ToObjectQuery();

            foreach (var param in objectQuery.Parameters)
            {
                command.Parameters.AddWithValue(param.Name, param.Value);
            }

            return command;
        }

        public static string ToTraceString<T>(this DbQuery<T> query)
        {
            var objectQuery = query.ToObjectQuery();

            return objectQuery.ToTraceStringWithParameters();
        }

        public static string ToTraceStringWithParameters<T>(this ObjectQuery<T> query)
        {
            var traceString = query.ToTraceString() + "\n";

            foreach (var parameter in query.Parameters)
            {
                traceString += parameter.Name + " [" + parameter.ParameterType.FullName + "] = " + parameter.Value + "\n";
            }

            return traceString;
        }
    }
}
