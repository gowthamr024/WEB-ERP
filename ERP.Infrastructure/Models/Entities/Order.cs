namespace ERP.Infrastructure.Models.Entities
{
    public class OrderMaster
    {
        public int OrderId { get; set; }
        public required string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public int CustomerId { get; set; }
        public string DestinationCountry { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = "Pending";
    }
}
