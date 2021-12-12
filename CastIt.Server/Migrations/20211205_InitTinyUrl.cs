using CastIt.Domain.Entities;
using FluentMigrator;

namespace CastIt.Server.Migrations
{
    [Migration(3)]
    public class _20211205_InitTinyUrl : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(TinyUrl))
                .WithColumn(nameof(TinyUrl.Id)).AsInt64().PrimaryKey().Identity()
                .WithColumn(nameof(TinyUrl.Code))
                    .AsString(100)
                    .NotNullable()
                    .Unique()
                .WithColumn(nameof(TinyUrl.Base64)).AsString(int.MaxValue).NotNullable()
                .WithColumn(nameof(TinyUrl.CreatedAt)).AsDateTime().NotNullable()
                .WithColumn(nameof(TinyUrl.UpdatedAt)).AsDateTime().Nullable();
        }

        public override void Down()
        {
            Delete.Table(nameof(TinyUrl));
        }
    }
}
