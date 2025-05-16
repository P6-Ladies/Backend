// src\Extensions\ApplicationBuilder.cs
using backend.Data;
using backend.Entities.Agents;
using backend.Entities.Assessments;
using backend.Entities.Conversations;
using backend.Entities.Messages;
using backend.Entities.Scenarios;
using backend.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Extensions
{
    public static class ApplicationBuilder
    {
        public static async Task SeedDataAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var dbContext = scope.ServiceProvider.GetRequiredService<PrototypeDbContext>();

            // Ensure database schema is up-to-date
            await dbContext.Database.MigrateAsync();

            // 1) Seed a default user
            var user = await userManager.FindByEmailAsync("user@user.com");
            if (user == null)
            {
                user = new User { UserName = "user@user.com", Email = "user@user.com" };
                var createResult = await userManager.CreateAsync(user, "Password123!");
                if (!createResult.Succeeded)
                {
                    throw new Exception($"User creation failed: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }

            // 2) Seed agents for this user
            if (!dbContext.Agents.Any(a => a.UserId == user.Id))
            {
                var defaultAgent = new Agent
                {
                    UserId = user.Id,
                    Name = "Default Assistant",
                    PromptBody = "You are a helpful assistant.",
                    Openness = 5,
                    Conscientiousness = 5,
                    Extroversion = 5,
                    Agreeableness = 5,
                    Neuroticism = 5
                };
                dbContext.Agents.Add(defaultAgent);
                await dbContext.SaveChangesAsync();
            }

            // 3) Seed scenarios for this user
            if (!dbContext.Scenarios.Any(s => s.UserId == user.Id))
            {
                var defaultScenario = new Scenario
                {
                    UserId = user.Id,
                    Name = "Default Scenario",
                    SettingPrompt = "You are in a default scenario.",
                    ConflictPrompt = "A sample conflict arises.",
                    AdditionalPrompt = "Additional context information.",
                    CreatedAt = DateTime.UtcNow
                };
                dbContext.Scenarios.Add(defaultScenario);
                await dbContext.SaveChangesAsync();
            }

            // Retrieve seeded agent and scenario
            var agent = dbContext.Agents.First(a => a.UserId == user.Id);
            var scenario = dbContext.Scenarios.First(s => s.UserId == user.Id);

            // 4) Seed conversations
            if (!dbContext.Conversations.Any(c => c.UserId == user.Id && c.AgentId == agent.Id))
            {
                var defaultConversation = new Conversation
                {
                    Title = "Default Conversation",
                    UserId = user.Id,
                    AgentId = agent.Id,
                    ScenarioId = scenario.Id,
                    CreatedAt = DateTime.UtcNow
                };
                dbContext.Conversations.Add(defaultConversation);
                await dbContext.SaveChangesAsync();
            }

            // Retrieve the seeded conversation
            var conversation = dbContext.Conversations.First(c => c.UserId == user.Id && c.AgentId == agent.Id);

            // 5) Seed messages for the conversation
            if (!dbContext.Messages.Any(m => m.ConversationId == conversation.Id))
            {
                var messages = new List<Message>
                {
                    new Message
                    {
                        ConversationId = conversation.Id,
                        UserSent = true,
                        Body = "Hello, how are you?",
                        ReceivedAt = DateTime.UtcNow
                    },
                    new Message
                    {
                        ConversationId = conversation.Id,
                        UserSent = false,
                        Body = "I'm good, thanks! How can I assist you today?",
                        ReceivedAt = DateTime.UtcNow
                    }
                };
                dbContext.Messages.AddRange(messages);
                await dbContext.SaveChangesAsync();
            }

            // 6) Seed an assessment for the conversation
            if (!dbContext.Assessments.Any(a => a.ConversationId == conversation.Id))
            {
                var defaultAssessment = new Assessment
                {
                    UserId = user.Id,
                    ConversationId = conversation.Id,
                    Body = "Sample assessment of the conversation.",
                    ConflictManagementStrategy = "Collaboration",
                    Openness = 7,
                    Conscientiousness = 6,
                    Extroversion = 8,
                    Agreeableness = 9,
                    Neuroticism = 3
                };
                dbContext.Assessments.Add(defaultAssessment);
                await dbContext.SaveChangesAsync();
            }

        }
        
        public static async Task ApplyMigrationsAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PrototypeDbContext>();
            await dbContext.Database.MigrateAsync();
        }
    }
}