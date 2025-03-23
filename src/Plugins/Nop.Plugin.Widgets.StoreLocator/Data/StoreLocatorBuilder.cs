using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Widgets.StoreLocator.Domain;

namespace Nop.Plugin.Widgets.StoreLocator.Data;

public class StoreLocatorBuilder : NopEntityBuilder<StoreLocation>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(StoreLocation.Id))
                .AsInt32().PrimaryKey().Identity()
                .NotNullable()
            .WithColumn(nameof(StoreLocation.Latitude))
                .AsDecimal(18, 8)
                .Nullable()
            .WithColumn(nameof(StoreLocation.Longitude))
            .AsDecimal(18, 8)
                .Nullable()
                ;
    }
}
