using System.Collections;
using System.Runtime.InteropServices;
using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Interfaces;

namespace SoftDemat.Infrastructure.Mail;

public sealed class OutlookMailbox : ILocalMailbox
{
    public string ReadDefaultAddress()
        => StaRunner.Run(ReadAddress);

    public string Send(OutgoingMail mail)
        => StaRunner.Run(() => Deliver(mail));

    private static string ReadAddress()
    {
        var application = Create();
        try
        {
            var address = FirstAddress(application);
            return address ?? throw new DomainException("Aucun compte de messagerie Outlook n'a été trouvé sur ce poste.");
        }
        finally
        {
            Marshal.ReleaseComObject(application);
        }
    }

    private static string Deliver(OutgoingMail mail)
    {
        var application = Create();
        try
        {
            dynamic account = FirstAccount(application)
                ?? throw new DomainException("Aucun compte de messagerie Outlook n'a été trouvé sur ce poste.");
            var from = AddressOf(account)!;
            dynamic item = application.CreateItem(0);
            item.SendUsingAccount = account;
            item.To = mail.To;
            if (!string.IsNullOrWhiteSpace(mail.Bcc))
                item.BCC = mail.Bcc;
            item.Subject = mail.Subject;
            item.HTMLBody = mail.HtmlBody;
            item.Attachments.Add(mail.AttachmentPath, 1, 1, mail.AttachmentName);
            item.Send();
            return from;
        }
        finally
        {
            Marshal.ReleaseComObject(application);
        }
    }

    private static string? FirstAddress(dynamic application)
    {
        dynamic account = FirstAccount(application);
        return account is null ? null : AddressOf(account);
    }

    private static dynamic? FirstAccount(dynamic application)
    {
        dynamic session = application.Session;
        foreach (dynamic account in (IEnumerable)session.Accounts)
        {
            if (!string.IsNullOrWhiteSpace(AddressOf(account)))
                return account;
        }

        return null;
    }

    private static string? AddressOf(dynamic account)
    {
        try
        {
            return ((string?)account.SmtpAddress)?.Trim();
        }
        catch (COMException)
        {
            return null;
        }
    }

    private static dynamic Create()
    {
        var type = Type.GetTypeFromProgID("Outlook.Application");
        if (type is null)
            throw new DomainException("Outlook n'est pas installé sur ce poste.");

        return Activator.CreateInstance(type)
            ?? throw new DomainException("Outlook n'a pas pu être ouvert sur ce poste.");
    }
}
