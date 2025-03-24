using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Pickup.PickupInStore.Domain;

namespace Nop.Plugin.Pickup.PickupInStore.Data.Migrations;

[NopMigration("2025/02/03 09:30:17:6455422", "Pickup.PickupInStore schema 2", MigrationProcessType.Update)]
public class CustomSchemaMigration : MigrationBase
{
    public override void Down()
    {

    }

    public override void Up()
    {
        if (!Schema.Table(nameof(StorePickupPoint)).Column(nameof(StorePickupPoint.PictureId)).Exists())
        {
            Alter.Table(nameof(StorePickupPoint))
          .AddColumn(nameof(StorePickupPoint.PictureId))
              .AsInt32().WithDefault(0);
        }
        if (!Schema.Table(nameof(StorePickupPoint)).Column(nameof(StorePickupPoint.ClosedDays)).Exists())
        {
            Alter.Table(nameof(StorePickupPoint))
            .AddColumn(nameof(StorePickupPoint.ClosedDays))
                .AsString().Nullable();
        }

    }
}