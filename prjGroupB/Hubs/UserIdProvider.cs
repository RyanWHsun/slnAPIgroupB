using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace prjGroupB.Hubs
{
    public class UserIdProvider: IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            try
            {
                return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            catch (Exception ex) {
                Console.WriteLine($"signalR記錄連線者資訊失敗: {ex.Message}");
                return "0";
            }

        }
    }
}
