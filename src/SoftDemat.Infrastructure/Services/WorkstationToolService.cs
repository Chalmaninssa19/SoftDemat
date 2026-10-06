using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;
using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Interfaces;

namespace SoftDemat.Infrastructure.Services;

public sealed class WorkstationToolService : IWorkstationToolService
{
    private readonly IArchiveFolderPicker _folders;
    private readonly ILocalMailbox _mailbox;

    public WorkstationToolService(IArchiveFolderPicker folders, ILocalMailbox mailbox)
    {
        _folders = folders;
        _mailbox = mailbox;
    }

    public Task<ArchiveFolderPickResponse> PickArchiveFolderAsync(bool onSenderMachine, CancellationToken cancellationToken = default)
    {
        if (!onSenderMachine)
            throw new DomainException("Le choix du dossier d'archivage s'ouvre sur le poste où SoftDemat est lancé.");

        return Task.FromResult(new ArchiveFolderPickResponse(_folders.Pick() ?? string.Empty));
    }

    public Task<OutlookSenderResponse> ReadOutlookSenderAsync(bool onSenderMachine, CancellationToken cancellationToken = default)
    {
        if (!onSenderMachine)
            throw new DomainException("Outlook se lit sur le poste de la personne qui envoie, là où SoftDemat est lancé.");

        return Task.FromResult(new OutlookSenderResponse(_mailbox.ReadDefaultAddress()));
    }
}
