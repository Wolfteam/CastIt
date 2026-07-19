using CastIt.Domain.Entities;
using FluentMigrator;

namespace CastIt.Server.Migrations
{
    [Migration(5)]
    public class _20260705_InitCustomEndpoint : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(CustomEndpoint))
                .WithColumn(nameof(CustomEndpoint.Id)).AsInt64().PrimaryKey().Identity()
                .WithColumn(nameof(CustomEndpoint.Name)).AsString(200).NotNullable()
                .WithColumn(nameof(CustomEndpoint.Host)).AsString(200).NotNullable()
                .WithColumn(nameof(CustomEndpoint.Port)).AsInt32().NotNullable()
                .WithColumn(nameof(CustomEndpoint.CreatedAt)).AsDateTime().NotNullable()
                .WithColumn(nameof(CustomEndpoint.UpdatedAt)).AsDateTime().Nullable();
        }

        public override void Down()
        {
            Delete.Table(nameof(CustomEndpoint));
        }
    }
}
