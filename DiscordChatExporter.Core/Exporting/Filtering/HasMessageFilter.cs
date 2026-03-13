using System;
using System.Linq;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Markdown.Parsing;

namespace DiscordChatExporter.Core.Exporting.Filtering;

internal class HasMessageFilter(MessageContentMatchKind kind) : MessageFilter
{
    public override bool IsMatch(Message message) =>
        kind switch
        {
            MessageContentMatchKind.Link => new[] { message.Content }
                .Concat(message.Snapshots.Select(s => s.Content))
                .Any(content => MarkdownParser.ExtractLinks(content).Any()),
            MessageContentMatchKind.Embed => message.Embeds.Any(),
            MessageContentMatchKind.File => message.Attachments.Any(),
            MessageContentMatchKind.Video => message.Attachments.Any(file => file.IsVideo),
            MessageContentMatchKind.Image => message.Attachments.Any(file => file.IsImage),
            MessageContentMatchKind.Sound => message.Attachments.Any(file => file.IsAudio),
            MessageContentMatchKind.Pin => message.IsPinned,
            MessageContentMatchKind.Invite => new[] { message.Content }
                .Concat(message.Snapshots.Select(s => s.Content))
                .SelectMany(MarkdownParser.ExtractLinks)
                .Select(l => l.Url)
                .Select(Invite.TryGetCodeFromUrl)
                .Any(c => !string.IsNullOrWhiteSpace(c)),
            _ => throw new InvalidOperationException(
                $"Unknown message content match kind '{kind}'."
            ),
        };
}
