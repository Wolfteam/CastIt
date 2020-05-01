using CastIt.Models.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CastIt.Models.EntitiesConfiguration
{
    public class FileItemConfigurationType : BaseConfiguration<FileItem>
    {
        public override void Configure(EntityTypeBuilder<FileItem> builder)
        {
            base.Configure(builder);

            builder.Property(p => p.Path).IsRequired();
        }
    }
}
