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
        public string Id { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<PaymentProduct> Products { get; set; } = new List<PaymentProduct>();
    }

    public class PaymentProduct
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
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