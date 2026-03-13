using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Exporting;
using DiscordChatExporter.Core.Exporting.Filtering;
using DiscordChatExporter.Core.Exporting.Partitioning;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class CsvAttachmentOnlySpecs
{
    private static ExportContext CreateContext(string outputPath, ExportFormat format)
    {
        var guild = new Guild(Snowflake.Parse("1"), "Test Guild", "");
        var channel = new Channel(
            Snowflake.Parse("2"),
            ChannelKind.GuildTextChat,
            guild.Id,
            Parent: null,
            Name: "general",
            Position: 0,
            IconUrl: null,
            Topic: null,
            IsArchived: false,
            LastMessageId: Snowflake.Parse("3")
        );

        var request = new ExportRequest(
            guild,
            channel,
            outputPath,
            assetsDirPath: null,
            format,
            after: null,
            before: null,
            PartitionLimit.Null,
            MessageFilter.Null,
            shouldFormatMarkdown: false,
            shouldDownloadAssets: false,
            shouldReuseAssets: false,
            locale: null,
            isUtcNormalizationEnabled: true
        );

        return new ExportContext(new DiscordClient("fake"), request);
    }

    [Fact]
    public async Task I_can_export_attachment_only_messages_to_csv()
    {
        var attachment = new Attachment(
            Snowflake.Parse("10"),
            "https://cdn.test/attachment.bin",
            "attachment.bin",
            Description: null,
            Width: null,
            Height: null,
            FileSize.FromBytes(1024)
        );

        var attachmentOnlyMessage = new Message(
            Snowflake.Parse("4"),
            MessageKind.Default,
            MessageFlags.None,
            new User(
                Snowflake.Parse("5"),
                IsBot: false,
                Discriminator: 1,
                Name: "user",
                DisplayName: "user",
                AvatarUrl: ""
            ),
            Timestamp: DateTimeOffset.Parse("2026-03-13T00:00:00Z"),
            EditedTimestamp: null,
            CallEndedTimestamp: null,
            IsPinned: false,
            Content: "",
            Attachments: new[] { attachment },
            Embeds: [],
            Stickers: [],
            Reactions: [],
            MentionedUsers: [],
            Snapshots: [],
            Reference: null,
            ReferencedMessage: null,
            Interaction: null
        );

        var emptyMessage = attachmentOnlyMessage with
        {
            Id = Snowflake.Parse("6"),
            Content = "",
            Attachments = [],
        };

        await using var stream = new MemoryStream();
        var context = CreateContext(
            Path.Combine(Path.GetTempPath(), "csv-export.csv"),
            ExportFormat.Csv
        );
        await using (var writer = new CsvMessageWriter(stream, context))
        {
            await writer.WritePreambleAsync();
            await writer.WriteMessageAsync(attachmentOnlyMessage);
            await writer.WriteMessageAsync(emptyMessage);
            await writer.WritePostambleAsync();
        }

        var csv = Encoding.UTF8.GetString(stream.ToArray());

        csv.Should().Contain("https://cdn.test/attachment.bin");
        csv.Should().Contain("\"[]\""); // empty attachments column should be an empty list, not NaN
    }
}
