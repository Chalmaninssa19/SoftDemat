namespace SoftDemat.Domain.Rules;

public static class PasswordPolicy
{
    public const int MinimumLength = 12;
    public const string FailureMessage =
        "Le mot de passe doit contenir au moins 12 caractères dont au moins 3 des familles suivantes : majuscule, minuscule, chiffre, caractère spécial.";

    public static bool IsSatisfied(string? password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < MinimumLength)
            return false;

        var families = 0;
        if (password.Any(char.IsUpper))
            families++;
        if (password.Any(char.IsLower))
            families++;
        if (password.Any(char.IsDigit))
            families++;
        if (password.Any(character => !char.IsLetterOrDigit(character)))
            families++;

        return families >= 3;
    }
}
