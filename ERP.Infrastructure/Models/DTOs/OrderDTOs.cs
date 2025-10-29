using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Infrastructure.Models.DTOs
{
    public class Order
    {
        public int OrderID { get; set; }
        public string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public int CustomerID { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }
}
