namespace SoftDemat.Domain.Interfaces;

public interface IPayslipMailer
{
    Task<string> SendAsync(OutgoingMail mail, bool onSenderMachine, CancellationToken cancellationToken = default);
}
