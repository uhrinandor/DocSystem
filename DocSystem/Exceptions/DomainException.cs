using DocSystem.Errors;

namespace DocSystem.Exceptions;

public class DomainException(DomainError error, Exception? innerException = null)
    : Exception(error.Message, innerException)
{
    public DomainError Error { get; } = error;
}
