using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.Engine.Physics;
using System.Reflection;

namespace Sandbox.Game.Bubbles
{
    class BubblePhysicsBody : MyPhysicsBody
    {
        public BubblePhysicsBody(VRage.ModAPI.IMyEntity entity, VRage.Components.RigidBodyFlag flags)
            : base(entity, flags)
        {

        }

        public static void CopyFromBase(BubblePhysicsBody bphys, MyPhysicsBody ophys)
        {
            foreach (PropertyInfo prop in ophys.GetType().GetProperties())
            {
                if (prop.GetSetMethod() != null)
                    prop.SetValue(bphys, prop.GetValue(ophys, null), null);
            }
            foreach (FieldInfo field in ophys.GetType().GetFields())
            {
                    field.SetValue(bphys, field.GetValue(ophys));
            }
        }
    }
}
