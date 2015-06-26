using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.Engine.Physics;
using System.Reflection;

using VRage.ModAPI;

namespace Sandbox.Game.Bubbles
{
    class BubblePhysicsBody : MyPhysicsBody
    {
        public BubblePhysicsBody(IMyEntity entity, VRage.Components.RigidBodyFlag flags) : base(entity, flags) { }

        public override VRageMath.Vector3 LinearVelocity { get; set; }
        
    }
}
