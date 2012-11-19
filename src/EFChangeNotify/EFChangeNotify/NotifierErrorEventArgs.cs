using System;
using System.Data.SqlClient;

namespace EFChangeNotify
{
    public class NotifierErrorEventArgs : EventArgs
    {
        public string Sql { get; set; }
        public SqlNotificationEventArgs Reason { get; set; }
    }
}
