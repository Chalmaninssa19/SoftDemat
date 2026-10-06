using SoftDemat.Application.DTOs;
using SoftDemat.Domain.Exceptions;

namespace SoftDemat.Api.Uploads;

public static class PayslipUploadBinder
{
    public static IReadOnlyList<PayslipUploadItem> Bind(IReadOnlyList<IFormFile>? files, IReadOnlyList<string>? paths)
    {
        var uploaded = files ?? [];
        var names = paths ?? [];
        if (uploaded.Count != names.Count)
            throw new DomainException("Le téléversement est incomplet.");

        return uploaded
            .Select((file, index) => new PayslipUploadItem(names[index], file.OpenReadStream(), file.Length))
            .ToList();
    }
}
