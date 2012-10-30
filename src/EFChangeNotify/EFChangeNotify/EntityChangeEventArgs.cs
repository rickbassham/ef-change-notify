using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFChangeNotify
{
    public class EntityChangeEventArgs<T> : EventArgs
    {
        public IEnumerable<T> Results { get; set; }
        public bool ContinueListening { get; set; }
    }
}
