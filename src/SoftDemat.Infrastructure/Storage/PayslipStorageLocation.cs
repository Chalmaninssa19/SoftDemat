using Microsoft.Extensions.Options;
using SoftDemat.Infrastructure.Options;

namespace SoftDemat.Infrastructure.Storage;

public sealed class PayslipStorageLocation
{
    public PayslipStorageLocation(IOptions<PayslipStorageOptions> options)
    {
        Root = ResolveRoot(options.Value.RootPath);
    }

    public string Root { get; }

    public static string ResolveRoot(string? configuredRoot)
        => string.IsNullOrWhiteSpace(configuredRoot)
            ? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "App_Data", "payslip-uploads"))
            : Path.GetFullPath(configuredRoot);
}
