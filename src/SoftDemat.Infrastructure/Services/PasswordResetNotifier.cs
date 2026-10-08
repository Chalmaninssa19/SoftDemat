using System.Net.Sockets;
using System.Security.Authentication;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SoftDemat.Application.Services.Interfaces;
using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Interfaces;

namespace SoftDemat.Infrastructure.Services;

public sealed class PasswordResetNotifier : IPasswordResetNotifier
{
    private const string ResetPath = "/auth/reinitialiser";
    private readonly IMailSender _mailSender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PasswordResetNotifier> _logger;

    public PasswordResetNotifier(
        IMailSender mailSender,
        IConfiguration configuration,
        ILogger<PasswordResetNotifier> logger)
    {
        _mailSender = mailSender;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendResetLinkAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        var resetUrl = CreateResetUrl(token);
        try
        {
            await _mailSender.SendAsync(new OutgoingMail(
                email,
                null,
                "Réinitialisation de votre mot de passe SOFT-DEMAT",
                CreateHtmlBody(resetUrl),
                string.Empty,
                string.Empty), cancellationToken);
        }
        catch (Exception exception) when (IsDeliveryError(exception))
        {
            _logger.LogError(exception, "L'envoi du lien de réinitialisation a échoué.");
        }
    }

    private Uri CreateResetUrl(string token)
    {
        var frontendUrl = _configuration["PasswordReset:FrontendBaseUrl"];
        if (string.IsNullOrWhiteSpace(frontendUrl))
            frontendUrl = _configuration.GetSection("Cors:Origins").Get<string[]>()?.FirstOrDefault();
        if (!Uri.TryCreate(frontendUrl, UriKind.Absolute, out var baseUri)
            || (baseUri.Scheme != Uri.UriSchemeHttps && baseUri.Scheme != Uri.UriSchemeHttp))
            throw new DomainException("L'adresse publique de l'application n'est pas configurée.");

        var builder = new UriBuilder(baseUri)
        {
            Path = ResetPath,
            Query = $"token={Uri.EscapeDataString(token)}"
        };
        return builder.Uri;
    }

    private static string CreateHtmlBody(Uri resetUrl)
        => $"""
            <p>Bonjour,</p>
            <p>Une demande de réinitialisation de votre mot de passe SOFT-DEMAT a été effectuée.</p>
            <p><a href="{System.Net.WebUtility.HtmlEncode(resetUrl.AbsoluteUri)}">Choisir un nouveau mot de passe</a></p>
            <p>Ce lien est valable pendant une heure et ne peut être utilisé qu'une seule fois.</p>
            <p>Si vous n'êtes pas à l'origine de cette demande, vous pouvez ignorer ce message.</p>
            <p>Cordialement,<br>SOFT-DEMAT</p>
            """;

    private static bool IsDeliveryError(Exception exception)
        => exception is DomainException
            or SmtpCommandException
            or SmtpProtocolException
            or SocketException
            or AuthenticationException
            or IOException;
}
