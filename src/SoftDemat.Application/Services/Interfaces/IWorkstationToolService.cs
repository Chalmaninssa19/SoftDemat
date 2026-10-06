using SoftDemat.Application.DTOs;

namespace SoftDemat.Application.Services.Interfaces;

public interface IWorkstationToolService
{
    Task<ArchiveFolderPickResponse> PickArchiveFolderAsync(bool onSenderMachine, CancellationToken cancellationToken = default);
    Task<OutlookSenderResponse> ReadOutlookSenderAsync(bool onSenderMachine, CancellationToken cancellationToken = default);
}
