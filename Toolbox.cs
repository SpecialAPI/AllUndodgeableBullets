using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace AllUndodgeableBullets
{
    public class Toolbox
    {
        public static BindingFlags AnyBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        public static FieldInfo ProjectileHealthHaverHitCountInfo = typeof(Projectile).GetField("m_healthHaverHitCount", AnyBindingFlags);
        public static FieldInfo ProjectileHasPiercedInfo = typeof(Projectile).GetField("m_hasPierced", AnyBindingFlags);
        public static MethodInfo ProjectileHandleDelayedDamageInfo = typeof(Projectile).GetMethod("HandleDelayedDamage", AnyBindingFlags);
    }
}
