using Improbable.Gdk.Core;
using Improbable.Gdk.Health;
using Unity.Collections;
using Unity.Entities;

namespace Improbable.Gdk.Guns
{
    [UpdateInGroup(typeof(SpatialOSUpdateGroup))]
    public class ServerShootingSystem : ComponentSystem
    {
        private struct PlayersShooting
        {
            public readonly int Length;
            [ReadOnly] public ComponentDataArray<SpatialEntityId> EntityId;
            [ReadOnly] public ComponentDataArray<ShootingComponent.ReceivedEvents.Shots> Shots;
            [ReadOnly] public ComponentDataArray<GunComponent.Component> Gun;
            public EntityArray Entities;
        }

        [Inject] private PlayersShooting playersShooting;
        [Inject] private CommandSystem commandSystem;

        protected override void OnUpdate()
        {
            for (var i = 0; i < playersShooting.Length; i++)
            {
                var commandSent = false;
                var gunId = playersShooting.Gun[i].GunId;
                var gunSettings = GunDictionary.Get(gunId);
                var damage = gunSettings.ShotDamage;

                foreach (var shot in playersShooting.Shots[i].Events)
                {
                    var shotInfo = shot;
                    if (!ValidateShot(shotInfo))
                    {
                        continue;
                    }

                    // Send command to entity being shot.
                    var modifyHealthRequest = new HealthComponent.ModifyHealth.Request(
                        new EntityId(shotInfo.EntityId),
                        new HealthModifier()
                        {
                            Amount = -damage,
                            Origin = shotInfo.HitOrigin,
                            AppliedLocation = shotInfo.HitLocation
                        });
                    commandSystem.SendCommand(modifyHealthRequest, playersShooting.Entities[i]);
                    commandSent = true;
                }
            }
        }

        private bool ValidateShot(ShotInfo shot)
        {
            return shot.HitSomething && shot.EntityId > 0;
        }
    }
}
