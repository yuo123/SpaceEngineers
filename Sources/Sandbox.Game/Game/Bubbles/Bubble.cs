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
        //MTODO: To-do list:
         Add methods to add and remove objects from bubble
         Implement bubble "transitions" inside UpdateAfterSimulation
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

            re.AddEntityToBubble(grid);
            //grid2.OnAddedToScene(null, re.m_internWorld);

            //add character to debug bubble
            MyEntities.Remove(MySession.LocalCharacter);
            re.AddEntityToBubble(MySession.LocalCharacter);

            MyPhysics.Bubbles.Add(re);
        }

        public override void UpdateAfterSimulation()
        {
            if (this.PositionComp.GetPosition() != VRageMath.Vector3D.Zero)
                System.Windows.Forms.MessageBox.Show("Bubble moved");
        }

        public void AddEntityToBubble(MyEntity entity)
        {
            entity.Physics.Bubble = this;
            entity.Physics.InBubble = true;
            entity.OnAddedToScene(null, m_internWorld);

            //MTODO: add location and velocity conversions
        }
    }
}
