using System.Net;

namespace SoftDemat.Api.Security;

public static class LocalRequest
{
    public static bool IsLoopback(HttpContext context)
    {
        var address = context.Connection.RemoteIpAddress;
        if (address is null)
            return true;
        if (address.IsIPv4MappedToIPv6)
            address = address.MapToIPv4();
        return IPAddress.IsLoopback(address);
    }
}
