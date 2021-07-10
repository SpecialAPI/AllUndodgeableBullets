using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace AllUndodgeableBullets
{
    public class AllUndodgeableModule : ETGModule
    {
        public override void Init()
        {
        }

        public override void Start()
        {
            ETGModConsole.Commands.AddUnit("undodgeablemode", Toggle);
            Hook h = new Hook(
                typeof(Projectile).GetMethod("OnPreCollision", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(AllUndodgeableModule).GetMethod("PreCollisionHook")
            );
            Hook h2 = new Hook(
                typeof(Projectile).GetMethod("HandleDamage", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(UndodgeableProjectile).GetMethod("HandleDamageHook", BindingFlags.NonPublic | BindingFlags.Static)
            );
            ETGModConsole.Log("All Undodgeable Bullets mod successfully initialized. Write undodgeablemode into the console to toggle All Undodgeable Bullets mode.");
            ETGModConsole.Log("All Undodgeable Bullets mode is now DISABLED.");
        }

        public delegate void Action<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

        public static void PreCollisionHook(Action<Projectile, SpeculativeRigidbody, PixelCollider, SpeculativeRigidbody, PixelCollider> orig, Projectile self, SpeculativeRigidbody myRigidbody, PixelCollider myCollider, SpeculativeRigidbody 
            otherRigidbody, PixelCollider otherCollider)
        {
            if(self.GetType() == typeof(Projectile) && (self.Owner == null || !(self.Owner is PlayerController)) && undodgeablemode)
            {
                if (otherRigidbody == self.Shooter && !self.allowSelfShooting)
                {
                    PhysicsEngine.SkipCollision = true;
                    return;
                }
                if (otherRigidbody.gameActor != null && otherRigidbody.gameActor is PlayerController && (!self.collidesWithPlayer || (otherRigidbody.gameActor as PlayerController).IsGhost || (otherRigidbody.gameActor as PlayerController).IsEthereal))
                {
                    PhysicsEngine.SkipCollision = true;
                    return;
                }
                if (otherRigidbody.aiActor)
                {
                    if (self.Owner is PlayerController && !otherRigidbody.aiActor.IsNormalEnemy)
                    {
                        PhysicsEngine.SkipCollision = true;
                        return;
                    }
                    if (self.Owner is AIActor && !self.collidesWithEnemies && otherRigidbody.aiActor.IsNormalEnemy && !otherRigidbody.aiActor.HitByEnemyBullets)
                    {
                        PhysicsEngine.SkipCollision = true;
                        return;
                    }
                }
                if (!GameManager.PVP_ENABLED && self.Owner is PlayerController && otherRigidbody.GetComponent<PlayerController>() != null && !self.allowSelfShooting)
                {
                    PhysicsEngine.SkipCollision = true;
                    return;
                }
                if (GameManager.Instance.InTutorial)
                {
                    PlayerController component = otherRigidbody.GetComponent<PlayerController>();
                    if (component)
                    {
                        if (component.spriteAnimator.QueryInvulnerabilityFrame())
                        {
                            GameManager.BroadcastRoomTalkDoerFsmEvent("playerDodgedBullet");
                        }
                        else if (component.IsDodgeRolling)
                        {
                            GameManager.BroadcastRoomTalkDoerFsmEvent("playerAlmostDodgedBullet");
                        }
                        else
                        {
                            GameManager.BroadcastRoomTalkDoerFsmEvent("playerDidNotDodgeBullet");
                        }
                    }
                }
                if (self.collidesWithProjectiles && self.collidesOnlyWithPlayerProjectiles && otherRigidbody.projectile && !(otherRigidbody.projectile.Owner is PlayerController))
                {
                    PhysicsEngine.SkipCollision = true;
                    return;
                }
            }
            else
            {
                orig(self, myRigidbody, myCollider, otherRigidbody, otherCollider);
            }
        }

        public void Toggle(string[] args)
        {
            undodgeablemode = !undodgeablemode;
            if (undodgeablemode)
            {
                ETGModConsole.Log("All Undodgeable Bullets mode enabled.");
                List<string> l = new List<string>()
                {
                    "Good luck beating dragun with that on.",
                    "You can go unbind your dodgeroll key now.",
                    "You're lucky that this doesn't work on beams.",
                    "Why?",
                    "Can you beat this mode? I don't think you can."
                };
                ETGModConsole.Log(BraveUtility.RandomElement(l));
            }
            else
            {
                ETGModConsole.Log("All Undodgeable Bullets mode disabled.");
            }
        }

        public override void Exit()
        {
        }

        public static bool undodgeablemode;
    }
}
