using Microsoft.AspNetCore.SignalR;
public class OrderHub:Hub
{
    public async Task NotifyOrderUpdated(int orderId)
    {
        await Clients.All.SendAsync("OrderUpdated", orderId);
    }
}
