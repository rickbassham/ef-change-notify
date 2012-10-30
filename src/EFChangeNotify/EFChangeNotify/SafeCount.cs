using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EFChangeNotify
{
    public class SafeCount
    {
        private int _counter;
        public int Counter { get { return _counter; } }

        public void Increment()
        {
            Interlocked.Increment(ref _counter);
        }

        public void Decrement()
        {
            Interlocked.Decrement(ref _counter);
        }
    }
}
