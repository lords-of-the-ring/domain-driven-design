using System.ComponentModel.DataAnnotations;

using ErrorOr;

namespace Domain.Abstractions;

public static class ErrorOrExtensions
{
    public static void ThrowIfError<T>(this ErrorOr<T> errorOr)
    {
        if (errorOr.IsError)
        {
            throw new ValidationException(errorOr.FirstError.Code);
        }
    }
}
