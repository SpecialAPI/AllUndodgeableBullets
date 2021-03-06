using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AllUndodgeableBullets
{
	public class UndodgeableProjectile : Projectile
    {
        public delegate TResult Func<T1, T2, T3, T4, T5, T6, TResult>(T1 arg1, T2 arg2, T3 arg3, out T4 arg4, T5 arg5, T6 arg6);
        protected static HandleDamageResult HandleDamageHook(Func<Projectile, SpeculativeRigidbody, PixelCollider, bool, PlayerController, bool, HandleDamageResult> orig, Projectile self, SpeculativeRigidbody rigidbody, PixelCollider hitPixelCollider, out bool killedTarget,
            PlayerController player, bool alreadyPlayerDelayed)
        {
            if (self.GetType() == typeof(Projectile) && (self.Owner == null || !(self.Owner is PlayerController)) && AllUndodgeableModule.undodgeablemode)
            {
                killedTarget = false;
                if (rigidbody.ReflectProjectiles)
                {
                    return HandleDamageResult.NO_HEALTH;
                }
                if (!rigidbody.healthHaver)
                {
                    return HandleDamageResult.NO_HEALTH;
                }
                if (!alreadyPlayerDelayed && s_delayPlayerDamage && player)
                {
                    return HandleDamageResult.HEALTH;
                }
                bool flag = !rigidbody.healthHaver.IsDead;
                float num = self.ModifiedDamage;
                if (self.Owner is AIActor && rigidbody && rigidbody.aiActor && (self.Owner as AIActor).IsNormalEnemy)
                {
                    num = ProjectileData.FixedFallbackDamageToEnemies;
                    if (rigidbody.aiActor.HitByEnemyBullets)
                    {
                        num /= 4f;
                    }
                }
                int healthHaverHitCount = (int)Toolbox.ProjectileHealthHaverHitCountInfo.GetValue(self);
                if (self.Owner is PlayerController && (bool)Toolbox.ProjectileHasPiercedInfo.GetValue(self) && healthHaverHitCount >= 1)
                {
                    int num2 = Mathf.Clamp(healthHaverHitCount - 1, 0, GameManager.Instance.PierceDamageScaling.Length - 1);
                    num *= GameManager.Instance.PierceDamageScaling[num2];
                }
                if (self.OnWillKillEnemy != null && num >= rigidbody.healthHaver.GetCurrentHealth())
                {
                    self.OnWillKillEnemy(self, rigidbody);
                }
                if (rigidbody.healthHaver.IsBoss)
                {
                    num *= self.BossDamageMultiplier;
                }
                if (self.BlackPhantomDamageMultiplier != 1f && rigidbody.aiActor && rigidbody.aiActor.IsBlackPhantom)
                {
                    num *= self.BlackPhantomDamageMultiplier;
                }
                bool flag2 = false;
                if (self.DelayedDamageToExploders)
                {
                    flag2 = (rigidbody.GetComponent<ExplodeOnDeath>() && rigidbody.healthHaver.GetCurrentHealth() <= num);
                }
                if (!flag2)
                {
                    HealthHaver healthHaver = rigidbody.healthHaver;
                    float damage = num;
                    Vector2 velocity = self.specRigidbody.Velocity;
                    string ownerName = self.OwnerName;
                    CoreDamageTypes coreDamageTypes = self.damageTypes;
                    DamageCategory damageCategory = (!self.IsBlackBullet) ? DamageCategory.Normal : DamageCategory.BlackBullet;
                    healthHaver.ApplyDamage(damage, velocity, ownerName, coreDamageTypes, damageCategory, true, hitPixelCollider, self.ignoreDamageCaps);
                    if (player && player.OnHitByProjectile != null)
                    {
                        player.OnHitByProjectile(self, player);
                    }
                }
                else
                {
                    rigidbody.StartCoroutine((IEnumerator)Toolbox.ProjectileHandleDelayedDamageInfo.Invoke(self, new object[] { rigidbody, num, self.specRigidbody.Velocity, hitPixelCollider }));
                }
                if (self.Owner && self.Owner is AIActor && player)
                {
                    (self.Owner as AIActor).HasDamagedPlayer = true;
                }
                killedTarget = (flag && rigidbody.healthHaver.IsDead);
                if (!killedTarget && rigidbody.gameActor != null)
                {
                    if (self.AppliesPoison && UnityEngine.Random.value < self.PoisonApplyChance)
                    {
                        rigidbody.gameActor.ApplyEffect(self.healthEffect, 1f, null);
                    }
                    if (self.AppliesSpeedModifier && UnityEngine.Random.value < self.SpeedApplyChance)
                    {
                        rigidbody.gameActor.ApplyEffect(self.speedEffect, 1f, null);
                    }
                    if (self.AppliesCharm && UnityEngine.Random.value < self.CharmApplyChance)
                    {
                        rigidbody.gameActor.ApplyEffect(self.charmEffect, 1f, null);
                    }
                    if (self.AppliesFreeze && UnityEngine.Random.value < self.FreezeApplyChance)
                    {
                        rigidbody.gameActor.ApplyEffect(self.freezeEffect, 1f, null);
                    }
                    if (self.AppliesCheese && UnityEngine.Random.value < self.CheeseApplyChance)
                    {
                        rigidbody.gameActor.ApplyEffect(self.cheeseEffect, 1f, null);
                    }
                    if (self.AppliesBleed && UnityEngine.Random.value < self.BleedApplyChance)
                    {
                        rigidbody.gameActor.ApplyEffect(self.bleedEffect, -1f, self);
                    }
                    if (self.AppliesFire && UnityEngine.Random.value < self.FireApplyChance)
                    {
                        rigidbody.gameActor.ApplyEffect(self.fireEffect, 1f, null);
                    }
                    if (self && UnityEngine.Random.value < self.StunApplyChance && rigidbody.gameActor.behaviorSpeculator)
                    {
                        rigidbody.gameActor.behaviorSpeculator.Stun(self.AppliedStunDuration, true);
                    }
                    for (int i = 0; i < self.statusEffectsToApply.Count; i++)
                    {
                        rigidbody.gameActor.ApplyEffect(self.statusEffectsToApply[i], 1f, null);
                    }
                }
                Toolbox.ProjectileHealthHaverHitCountInfo.SetValue(self, healthHaverHitCount + 1);
                return (!killedTarget) ? HandleDamageResult.HEALTH : HandleDamageResult.HEALTH_AND_KILLED;
            }
            else
            {
                return orig(self, rigidbody, hitPixelCollider, out killedTarget, player, alreadyPlayerDelayed);
            }
        }
	}
}
