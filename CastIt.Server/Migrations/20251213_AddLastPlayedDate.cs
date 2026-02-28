using CastIt.Domain.Entities;
using FluentMigrator;

namespace CastIt.Server.Migrations;

[Migration(4)]
public class _20251213_AddLastPlayedDate : Migration
{
    public override void Up()
    {
        Create.Column(nameof(FileItem.LastPlayedDate))
            .OnTable(nameof(FileItem))
            .AsDateTime().Nullable();
    }

    public override void Down()
    {
        Delete.Column(nameof(FileItem.LastPlayedDate))
            .FromTable(nameof(FileItem));
    }
}