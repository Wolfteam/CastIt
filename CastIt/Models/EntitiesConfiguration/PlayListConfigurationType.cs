using CastIt.Models.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CastIt.Models.EntitiesConfiguration
{
    public class PlayListConfigurationType : BaseConfiguration<PlayList>
    {
        public override void Configure(EntityTypeBuilder<PlayList> builder)
        {
            base.Configure(builder);
            builder.Property(p => p.Name).IsRequired();
        }
    }
}
