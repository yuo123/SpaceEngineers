using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Havok;

using Sandbox.Game.Entities;
using Sandbox.Definitions;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.World;
using Sandbox.Engine.Physics;

namespace Sandbox.Game.Bubbles
{
    public class Bubble : MyEntity
    {
        /*
        To-do list:
         add a HkWorld as a member of Bubble
         add a bubbles list to MyPhysics, to be updated every frame: 
          first in MyPhysics (like the Clusters list),
          then in UpdateAfterFrame, in Bubble, handling bubble "transitions"
         
         current target milestone: 
          having two physics objects in a bubble, thet interact with each other and 
          render correctly, but do not interact with anything else.
         
          - done? hard to tell, since I currently hav no way of interacting with objects inside bubbles.
        */

        #region fields

        private HkWorld m_internWorld;

        #endregion

        #region properties

        public HkWorld InternalWorld
        {
            get { return m_internWorld; }
        }

        #endregion

        public Bubble()
        {
            m_internWorld = MyPhysics.CreateHkWorld(200);
        }

        public static void CreateDebugBubble()
        {
            Bubble re = new Bubble();
            re.EntityId = 500;
            re.Save = false;

            re.Physics = new MyPhysicsBody(re, VRage.Components.RigidBodyFlag.RBF_DISABLE_COLLISION_RESPONSE);
            string prefabName;
            MyDefinitionManager.Static.GetBaseBlockPrefabName(MyCubeSize.Small, false, true, out prefabName);
            MyObjectBuilder_CubeGrid[] gridBuilders = MyPrefabManager.Static.GetGridPrefab(prefabName);
            //MyDefinitionManager.Static.GetBaseBlockPrefabName(MyCubeSize.Large, false, true, out prefabName);
            //MyObjectBuilder_CubeGrid[] gridBuilders2 = MyPrefabManager.Static.GetGridPrefab(prefabName);

            var blockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(gridBuilders[0].CubeBlocks.First().GetId());
            MyCubeGrid grid = MyEntities.CreateFromObjectBuilder(gridBuilders[0]) as MyCubeGrid;
            //MyCubeGrid grid2 = MyEntities.CreateFromObjectBuilder(gridBuilders2[0]) as MyCubeGrid;

            //grid2.PositionComp.SetPosition(new VRageMath.Vector3D(0, 1, 0));

            grid.OnAddedToScene(null, re.m_internWorld);
            //grid2.OnAddedToScene(null, re.m_internWorld);

            MyPhysics.Bubbles.Add(re);
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (this.PositionComp.GetPosition() != VRageMath.Vector3D.Zero)
                System.Windows.Forms.MessageBox.Show("Bubble moved");
        }
    }
}
