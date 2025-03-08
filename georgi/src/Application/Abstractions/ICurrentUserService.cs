using Domain.Users;

namespace Application.Abstractions;

public interface ICurrentUserService
{
    UserId GetUserId();
}
