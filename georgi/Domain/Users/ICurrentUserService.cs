namespace Domain.Users;

public interface ICurrentUserService
{
    UserId GetUserId();
}
