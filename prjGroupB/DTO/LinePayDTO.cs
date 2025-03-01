public class LinePayRequestDto
{
    public decimal amount { get; set; } // 總金額
    public string currency { get; set; } = "TWD"; // 幣別
    public string orderId { get; set; } // 訂單編號
    public List<LinePayPackageDto> packages { get; set; } = new(); // 商品包裝清單
    public LinePayRedirectUrlsDto redirectUrls { get; set; } // 付款完成後跳轉的 URL
}

public class LinePayPackageDto
{
    public string id { get; set; } // 套組 ID
    public decimal amount { get; set; } // 套組內總價
    public string name { get; set; } // 套組名稱
    public List<LinePayProductDto> products { get; set; } = new(); // 商品列表
}

public class LinePayRedirectUrlsDto
{
    public string confirmUrl { get; set; } // 付款成功回傳 URL
    public string cancelUrl { get; set; } // 付款取消回傳 URL
}

public class LinePayProductDto
{
    public string name { get; set; } // 商品名稱
    public string imageUrl { get; set; } // 商品圖片 URL
    public int quantity { get; set; } // 購買數量
    public decimal price { get; set; } // 單價
}
public class ConfirmPaymentDto
{
    public string transactionId { get; set; }
    public decimal amount { get; set; }
}




