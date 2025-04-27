using Discord;
using Discord.WebSocket;
using Serilog;

namespace Wondie_CSharp.Events.Actions
{
    /// <summary>
    /// Handles rule compliance by monitoring reactions to specific messages
    /// and assigning roles based on those reactions.
    /// </summary>
    public static class RuleComplianceEvent
    {
        // List of tracked message IDs, each mapping to an (Emote, RoleName) pair
        private static readonly Dictionary<ulong, (string Emote, string RoleName)> TrackedMessages = new()
        {
            // Message ID -> (Emote, RoleName)
            { 1365748781055737926, ("👍", "Peeps") }
        };

        /// <summary>
        /// Called when a reaction is added to a message.
        /// </summary>
        public static async Task HandleReactionAdded(
            Cacheable<IUserMessage, ulong> messageCache,
            Cacheable<IMessageChannel, ulong> channelCache,
            SocketReaction reaction)
        {
            if (!TrackedMessages.TryGetValue(reaction.MessageId, out var emoteAndRole)) return;

            var (expectedEmote, roleName) = emoteAndRole;
            
            if (reaction.Emote.Name != expectedEmote) return;

            if (reaction.User is { IsSpecified: true, Value: SocketGuildUser user })
            {
                var guild = user.Guild;
                var role = guild.Roles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));

                if (role == null)
                {
                    Log.Warning($"Role '{roleName}' not found in guild '{guild.Name}'.");
                    return;
                }

                if (!user.Roles.Contains(role))
                {
                    await user.AddRoleAsync(role);
                    Log.Information($"Assigned role '{roleName}' to {user.Username} for reacting to message {reaction.MessageId}.");
                }
            }
        }

        /// <summary>
        /// Called when a reaction is removed from a message.
        /// </summary>
        public static async Task HandleReactionRemoved(
            Cacheable<IUserMessage, ulong> messageCache,
            Cacheable<IMessageChannel, ulong> channelCache,
            SocketReaction reaction)
        {
            if (!TrackedMessages.TryGetValue(reaction.MessageId, out var emoteAndRole)) return;

            var (expectedEmote, roleName) = emoteAndRole;
            
            if (reaction.Emote.Name != expectedEmote) return;

            if (reaction.User is { IsSpecified: true, Value: SocketGuildUser user })
            {
                var guild = user.Guild;
                var role = guild.Roles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));

                if (role == null)
                {
                    Log.Warning($"Role '{roleName}' not found in guild '{guild.Name}'.");
                    return;
                }

                if (user.Roles.Contains(role))
                {
                    await user.RemoveRoleAsync(role);
                    Log.Information($"Removed role '{roleName}' from {user.Username} for removing reaction on message {reaction.MessageId}.");
                }
            }
        }
    }
}