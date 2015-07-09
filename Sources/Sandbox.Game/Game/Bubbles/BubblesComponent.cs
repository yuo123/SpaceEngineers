using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sandbox.Common;
using Sandbox.Engine.Physics;

namespace Sandbox.Game.Bubbles
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, 850)]
    public class BubblesComponent : MySessionComponentBase
    {
        public override void UpdateAfterSimulation()
        {
            //call UpdateAfterSimulation for bubbles. This is where bubble "transitions" happen
            //remember count when we started, because we don't want to update bubbles that were created this frame (they already were)
            int count = MyPhysics.Bubbles.Count;
            for (int i = 0; i < count; i++)
            {
                if (i < MyPhysics.Bubbles.Count && MyPhysics.Bubbles[i] != null)
                    MyPhysics.Bubbles[i].UpdateAfterSimulation();
            }
        }

    }
}
