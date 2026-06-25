using System;

namespace PaymentApi.Models
{
    public class Order
    {
        public long Id { get; set; }
        public string SourceId { get; set; } = string.Empty;
        public long AmountCents { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? SquarePaymentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? RawResponse { get; set; }
    }
}
