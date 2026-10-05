namespace SoftDemat.Domain.Constants;

public static class AuthConstants
{
    public const int SystemUserId = 1;
    public const string LoginFailureMessage =
        "Une erreur s'est produite lors de la connexion. Vérifiez le nom d'utilisateur ou le mot de passe.";

    public const string MustChangePasswordClaim = "must_change_password";
}
