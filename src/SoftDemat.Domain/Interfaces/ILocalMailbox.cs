namespace SoftDemat.Domain.Interfaces;

public interface ILocalMailbox
{
    string ReadDefaultAddress();
    string Send(OutgoingMail mail);
}
