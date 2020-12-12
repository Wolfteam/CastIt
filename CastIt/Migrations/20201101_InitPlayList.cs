using CastIt.Domain.Entities;
using FluentMigrator;

namespace CastIt.Migrations
{
    [Migration(1)]
    public class InitPlayList : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(PlayList))
                .WithColumn(nameof(PlayList.Id)).AsInt64().PrimaryKey().Identity()
                .WithColumn(nameof(PlayList.Name)).AsString(int.MaxValue).Nullable()
                .WithColumn(nameof(PlayList.Position)).AsInt32().NotNullable()
                .WithColumn(nameof(PlayList.Loop)).AsBoolean().NotNullable()
                .WithColumn(nameof(PlayList.Shuffle)).AsBoolean().NotNullable()
                .WithColumn(nameof(PlayList.CreatedAt)).AsDateTime().NotNullable()
                .WithColumn(nameof(PlayList.UpdatedAt)).AsDateTime().Nullable();
        }

        public override void Down()
        {
            Delete.Table(nameof(PlayList));
        }
    }
}
