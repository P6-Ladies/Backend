using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using backend.Entities.Users;

namespace backend.Data
{
    public class PrototypeDbContext(DbContextOptions<PrototypeDbContext> options) : IdentityDbContext<User>(options)
    {
    }
}