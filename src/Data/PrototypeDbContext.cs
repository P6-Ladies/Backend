// src\Data\PrototypeDbContext.cs
using Backend.Entities.Users;
using Backend.Entities.Agents;
using Backend.Entities.Assessments;
using Backend.Entities.Conversations;
using Backend.Entities.Messages;
using Backend.Entities.Scenarios;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public class PrototypeDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public PrototypeDbContext(DbContextOptions<PrototypeDbContext> options)
            : base(options)
        {
        }

        // Register custom entities as DbSets
        public DbSet<Agent> Agents { get; set; } = null!;
        public DbSet<Scenario> Scenarios { get; set; } = null!;
        public DbSet<Conversation> Conversations { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<Assessment> Assessments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users
            modelBuilder.Entity<User>(entity =>
            {
                // Renames the table: users
                entity.ToTable("users");
                // Makes the ID auto-incrementing
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id).ValueGeneratedOnAdd();
                // Email required & unique
                entity.Property(u => u.Email).IsRequired();
                entity.HasIndex(u => u.Email).IsUnique();
                // CreatedAt default
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Agents
            modelBuilder.Entity<Agent>(entity =>
            {
                // Renames the table: agents
                entity.ToTable("agents");
                // Makes the ID auto-incrementing & User.Id required
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Id).ValueGeneratedOnAdd();
                entity.Property(a => a.UserId).IsRequired();
                // Relationship: Agent -> User (many agents per user)
                entity.HasOne(a => a.User)
                    .WithMany(u => u.Agents)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Scenarios
            modelBuilder.Entity<Scenario>(entity =>
            {
                entity.ToTable("scenarios");
                entity.HasKey(s => s.Id);

                entity.Property(s => s.Id).ValueGeneratedOnAdd();
                // user_id required
                entity.Property(s => s.UserId).IsRequired();
                // Relationship: Scenario -> User
                entity.HasOne(s => s.User)
                    .WithMany(u => u.Scenarios)
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(s => s.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
            // Conversations
            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.ToTable("conversations");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).ValueGeneratedOnAdd();

                // user_id, agent_id required
                entity.Property(c => c.UserId).IsRequired();
                entity.Property(c => c.AgentId).IsRequired();
                entity.Property(c => c.ScenarioId).IsRequired();
                entity.Property(c => c.Title).IsRequired();
                // Relationship: Conversation -> User
                entity.HasOne(c => c.User)
                    .WithMany(u => u.Conversations)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relationship: Conversation -> Agent
                entity.HasOne(c => c.Agent)
                    .WithMany()
                    .HasForeignKey(c => c.AgentId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relationship: Conversation -> Scenario
                entity.HasOne(c => c.Scenario)
                    .WithMany()
                    .HasForeignKey(c => c.ScenarioId)
                    .OnDelete(DeleteBehavior.SetNull);

                // CreatedAt default
                entity.Property(c => c.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(c => c.TimeElapsed).HasColumnType("interval");
            });

            // Messages
            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("messages");
                entity.HasKey(m => m.Id);

                entity.Property(m => m.Id).ValueGeneratedOnAdd();

                // conversation_id required
                entity.Property(m => m.ConversationId).IsRequired();

                // Relationship: Message -> Conversation
                entity.HasOne(m => m.Conversation)
                    .WithMany()
                    .HasForeignKey(m => m.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);

                // ReceivedAt default
                entity.Property(m => m.ReceivedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Assessments
            modelBuilder.Entity<Assessment>(entity =>
            {
                entity.ToTable("assessments");
                entity.HasKey(a => a.Id);

                entity.Property(a => a.Id).ValueGeneratedOnAdd();

                // user_id, conversation_id required
                entity.Property(a => a.UserId).IsRequired();
                entity.Property(a => a.ConversationId).IsRequired();

                // Relationship: Assessment -> User
                entity.HasOne(a => a.User)
                    .WithMany(u => u.Assessments)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relationship: Assessment -> Conversation
                entity.HasOne(a => a.Conversation)
                    .WithMany(c => c.Assessments)
                    .HasForeignKey(a => a.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}