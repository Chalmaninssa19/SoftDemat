using System.Net.Sockets;
using MailKit.Net.Smtp;
using MailKit.Security;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Exceptions;

namespace SoftDemat.Infrastructure.Mail;

internal static class SmtpDeliveryFailure
{
    public static bool IsDeliveryFailure(Exception exception)
        => exception is SmtpCommandException
            or SmtpProtocolException
            or MailKit.Security.AuthenticationException
            or SslHandshakeException
            or NotSupportedException
            or SocketException
            or IOException
            or TimeoutException;

    public static DomainException ToDomainException(Exception exception, SmtpSetting setting, string from)
        => new(Describe(exception, setting, from), exception);

    private static string Describe(Exception exception, SmtpSetting setting, string from) => exception switch
    {
        SmtpCommandException command when IsSenderRejection(command)
            => $"Le serveur SMTP {setting.Host} refuse l'expéditeur {from} pour le compte {Account(setting)}. "
                + $"L'adresse d'expédition doit appartenir à ce compte. Réponse du serveur : {command.Message.Trim()}",
        SmtpCommandException command
            => $"Le serveur SMTP {setting.Host} a refusé le message. Réponse du serveur : {command.Message.Trim()}",
        MailKit.Security.AuthenticationException
            => $"Le serveur SMTP {setting.Host} refuse l'identifiant ou le mot de passe du compte {Account(setting)}.",
        SslHandshakeException
            => $"Connexion chiffrée impossible avec {setting.Host}:{setting.Port}. "
                + "Vérifiez le port et l'option STARTTLS.",
        NotSupportedException
            => $"Le serveur SMTP {setting.Host} ne prend pas en charge cette demande : {exception.Message} "
                + "Vérifiez le port et l'option STARTTLS.",
        _ => $"Le serveur SMTP {setting.Host}:{setting.Port} est injoignable ou a interrompu l'échange."
    };

    private static bool IsSenderRejection(SmtpCommandException exception)
        => exception.ErrorCode == SmtpErrorCode.SenderNotAccepted
            || exception.Message.Contains("sender", StringComparison.OrdinalIgnoreCase);

    private static string Account(SmtpSetting setting)
        => string.IsNullOrWhiteSpace(setting.UserName) ? "(sans authentification)" : setting.UserName;
}
