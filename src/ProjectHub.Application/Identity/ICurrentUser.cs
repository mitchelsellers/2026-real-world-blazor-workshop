namespace ProjectHub.Application.Identity;

public interface ICurrentUser
{    
    Task<Guid?> GetUserIdAsync();
}
