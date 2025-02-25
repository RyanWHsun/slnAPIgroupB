using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace prjGroupB.DTO
{
    public class PaymentRequestDTO
    {
        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public List<PaymentPackage> Packages { get; set; } = new List<PaymentPackage>();

        [Required]
        public string ConfirmUrl { get; set; } = string.Empty;

        [Required]
        public string CancelUrl { get; set; } = string.Empty;
    }

    public class PaymentPackage
    {
        public string id { get; set; } = string.Empty;
        public decimal amount { get; set; }
        public string name { get; set; } = string.Empty;
        public List<PaymentProduct> products { get; set; } = new List<PaymentProduct>();
    }

    public class PaymentProduct
    {
        public string id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string imageUrl { get; set; } = string.Empty;
        public int quantity { get; set; }
        public decimal price { get; set; }
    }

    /// <summary>
    /// LinePay 付款確認的 DTO
    /// </summary>
    public class ConfirmPaymentDto
    {
        [Required]
        public string TransactionId { get; set; } = string.Empty;  // ✅ 確保非 null

        [Required]
        public decimal Amount { get; set; }
    }
}