using CastIt.Domain.Entities;
using FluentMigrator;

namespace CastIt.Server.Migrations
{
    [Migration(2)]
    public class InitFileItem : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(FileItem))
                .WithColumn(nameof(FileItem.Id)).AsInt64().PrimaryKey().Identity()
                .WithColumn(nameof(FileItem.Name)).AsString(int.MaxValue).Nullable()
                .WithColumn(nameof(FileItem.Description)).AsString(int.MaxValue).Nullable()
                .WithColumn(nameof(FileItem.TotalSeconds)).AsDouble().NotNullable()
                .WithColumn(nameof(FileItem.Path)).AsString(int.MaxValue).NotNullable()
                .WithColumn(nameof(FileItem.Position)).AsInt32().NotNullable()
                .WithColumn(nameof(FileItem.PlayedPercentage)).AsDouble().NotNullable()
                .WithColumn(nameof(FileItem.CreatedAt)).AsDateTime().NotNullable()
                .WithColumn(nameof(FileItem.UpdatedAt)).AsDateTime().Nullable()
                .WithColumn(nameof(FileItem.PlayListId)).AsInt64().NotNullable().ForeignKey(nameof(PlayList), nameof(PlayList.Id));
        }

        public override void Down()
        {
            Delete.Table(nameof(FileItem));
        }
    }
}
