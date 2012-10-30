using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TestConsole.Models;

using EFChangeNotify;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var notifer = new EntityChangeNotifier<Product, StoreDbContext>(p => p.Name == "Lamp"))
            {
                notifer.Error += (sender, e) =>
                {
                    Console.WriteLine("[{0}, {1}, {2}]:\n{3}", e.Reason.Info, e.Reason.Source, e.Reason.Type, e.Sql);
                };

                notifer.Changed += (sender, e) =>
                {
                    Console.WriteLine(e.Results.Count());
                    foreach (var p in e.Results)
                    {
                        Console.WriteLine("  {0}", p.Name);
                    }
                };

                Console.WriteLine("Press any key to stop...");
                Console.ReadKey(true);
            }
        }
    }
}
