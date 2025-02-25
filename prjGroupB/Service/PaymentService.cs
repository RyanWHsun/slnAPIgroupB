//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Dapper;
//using Microsoft.Data.SqlClient;
//using prjGroupB.DTO;

//public class PaymentService
//{
//    private readonly string _connectionString;

//    public PaymentService(string connectionString)
//    {
//        _connectionString = connectionString;
//    }

//    public PaymentRequestDTO CreatePaymentRequest(int orderId)
//    {
//        using (var connection = new SqlConnection(_connectionString))
//        {
//            // 查詢訂單資料
//            var orderQuery = @"
//            SELECT FOrderId, FOrderDate, FPaymentMethod, FShipAddress
//            FROM tOrders
//            WHERE FOrderId = @OrderId";

//            var order = connection.QueryFirstOrDefault(orderQuery, new { OrderId = orderId });
//            if (order == null)
//                throw new Exception("Order not found");

//            // 查詢訂單商品資料
//            var orderDetailsQuery = @"
//            SELECT FItemId, FItemType, FOrderQty, FUnitPrice
//            FROM tOrdersDetails
//            WHERE FOrderId = @OrderId";

//            var orderDetails = connection.Query(orderDetailsQuery, new { OrderId = orderId }).ToList();
//            if (!orderDetails.Any())
//                throw new Exception("Order details not found");

//            // 計算總金額
//            var totalAmount = orderDetails.Sum(od => (decimal)od.FOrderQty * od.FUnitPrice);

//            // 創建付款請求資料
//            var packages = new List<PaymentPackage>();

//            packages.Add(new PaymentPackage
//            {
//                Id = "PKG001",
//                Name = "購物車結帳",
//                Amount = totalAmount,
//                Products = orderDetails.Select(od => new PaymentProduct
//                {
//                    Id = od.FItemId.ToString(),
//                    Name = od.FItemType, // 商品名稱從 FItemType（商品類型）取得
//                    Quantity = od.FOrderQty,
//                    Price = od.FUnitPrice,
//                    ImageUrl = "https://example.com/product-image.jpg" // 假設的商品圖片 URL
//                }).ToList()
//            });

//            return new PaymentRequestDTO
//            {
//                TotalAmount = totalAmount,
//                OrderId = order.FOrderId,  // ✅ 改回 `int` 型別
//                Packages = packages,
//                ConfirmUrl = "https://yourfrontend.com/payment-success",
//                CancelUrl = "https://yourfrontend.com/payment-failed"
//            };
//        }
//    }
//}