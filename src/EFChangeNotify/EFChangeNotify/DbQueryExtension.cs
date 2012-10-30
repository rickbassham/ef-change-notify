using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.Infrastructure;
using System.Reflection;
using System.Data.Objects;
using System.Data.SqlClient;

namespace EFChangeNotify
{
    public static class DbQueryExtension
    {
        /// <summary>
        /// Returns the SqlCommand that represents the given DbQuery.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static SqlCommand ToSqlCommand<T>(this DbQuery<T> query)
        {
            SqlCommand command = new SqlCommand();

            command.CommandText = query.ToString();

            var internalQueryField = query.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(f => f.Name.Equals("_internalQuery")).FirstOrDefault();

            var internalQuery = internalQueryField.GetValue(query);

            var objectQueryField = internalQuery.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(f => f.Name.Equals("_objectQuery")).FirstOrDefault();

            var objectQuery = objectQueryField.GetValue(internalQuery) as ObjectQuery<T>;

            foreach (var param in objectQuery.Parameters)
            {
                command.Parameters.AddWithValue(param.Name, param.Value);
            }

            return command;
        }

        // The two methods below were found at: 
        // http://social.msdn.microsoft.com/Forums/en-US/adodotnetentityframework/thread/91c7fb6d-d1b8-4a7f-aec9-16336dbd619b/
        public static string ToTraceString<T>(this DbQuery<T> query)
        {
            var internalQueryField = query.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(f => f.Name.Equals("_internalQuery")).FirstOrDefault();

            var internalQuery = internalQueryField.GetValue(query);

            var objectQueryField = internalQuery.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(f => f.Name.Equals("_objectQuery")).FirstOrDefault();

            var objectQuery = objectQueryField.GetValue(internalQuery) as ObjectQuery<T>;

            return objectQuery.ToTraceStringWithParameters();
        }

        public static string ToTraceStringWithParameters<T>(this ObjectQuery<T> query)
        {
            string traceString = query.ToTraceString() + "\n";

            foreach (var parameter in query.Parameters)
            {
                traceString += parameter.Name + " [" + parameter.ParameterType.FullName + "] = " + parameter.Value + "\n";
            }

            return traceString;
        }
    }
}
