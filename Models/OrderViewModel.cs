namespace manyasligida.Models
{
    public class OrderViewModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Notes { get; set; }
    }
} 