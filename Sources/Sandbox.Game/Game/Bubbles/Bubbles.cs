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
            foreach (Bubble bubble in MyPhysics.Bubbles)
            {
                bubble.UpdateAfterSimulation();
            }
        }

    }
}
