using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Embeds;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/resources/message#message-snapshot-structure
public partial record MessageSnapshot(
    DateTimeOffset Timestamp,
    DateTimeOffset? EditedTimestamp,
    string Content,
    IReadOnlyList<Attachment> Attachments,
    IReadOnlyList<Embed> Embeds,
    IReadOnlyList<Sticker> Stickers,
    IReadOnlyList<User> MentionedUsers
)
{
    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(Content)
        && !Attachments.Any()
        && !Embeds.Any()
        && !Stickers.Any();
}

public partial record MessageSnapshot
{
    public static MessageSnapshot Parse(JsonElement json)
    {
        // The snapshot payload is wrapped in the `message` property
        var messageJson = json.GetPropertyOrNull("message") ?? json;

        var timestamp =
            messageJson.GetPropertyOrNull("timestamp")?.GetDateTimeOffsetOrNull()
            ?? DateTimeOffset.MinValue;
        var editedTimestamp = messageJson
            .GetPropertyOrNull("edited_timestamp")
            ?.GetDateTimeOffsetOrNull();

        var content = messageJson.GetPropertyOrNull("content")?.GetStringOrNull() ?? "";

        var attachments =
            messageJson
                .GetPropertyOrNull("attachments")
                ?.EnumerateArrayOrNull()
                ?.Select(Attachment.Parse)
                .ToArray() ?? [];

        var embeds =
            messageJson
                .GetPropertyOrNull("embeds")
                ?.EnumerateArrayOrNull()
                ?.Select(Embed.Parse)
                .ToArray() ?? [];

        var stickers =
            messageJson
                .GetPropertyOrNull("sticker_items")
                ?.EnumerateArrayOrNull()
                ?.Select(Sticker.Parse)
                .ToArray() ?? [];

        var mentionedUsers =
            messageJson
                .GetPropertyOrNull("mentions")
                ?.EnumerateArrayOrNull()
                ?.Select(User.Parse)
                .ToArray() ?? [];

        return new MessageSnapshot(
            timestamp,
            editedTimestamp,
            content,
            attachments,
            embeds,
            stickers,
            mentionedUsers
        );
    }
}
