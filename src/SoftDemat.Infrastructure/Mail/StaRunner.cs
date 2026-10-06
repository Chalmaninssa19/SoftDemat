using SoftDemat.Domain.Exceptions;

namespace SoftDemat.Infrastructure.Mail;

internal static class StaRunner
{
    public static T Run<T>(Func<T> action)
    {
        T result = default!;
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                result = action();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        if (!thread.Join(TimeSpan.FromMinutes(2)))
            throw new DomainException("L'action sur ce poste a expiré.");
        if (failure is null)
            return result;
        if (failure is DomainException)
            throw failure;

        var message = failure.Message.Trim();
        throw new DomainException(message.Length <= 180 ? message : message[..180]);
    }
}
