using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestConsole.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
