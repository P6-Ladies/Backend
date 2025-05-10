// src\Entities\Users\User.cs
using Backend.Entities.Agents;
using Backend.Entities.Assessments;
using Backend.Entities.Conversations;
using Backend.Entities.Scenarios;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Backend.Entities.Users;
public class User : IdentityUser<int>
{
    [StringLength(256)]
    public override string? UserName { get; set; }

    [StringLength(256)]
    public override string? NormalizedUserName { get; set; }

    [StringLength(256)]
    [EmailAddress]
    public override string? Email { get; set; }

    [StringLength(256)]
    public override string? NormalizedEmail { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties to the rest of the domain:
    public ICollection<Agent> Agents { get; set; } = new List<Agent>();
    public ICollection<Scenario> Scenarios { get; set; } = new List<Scenario>();
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
}
