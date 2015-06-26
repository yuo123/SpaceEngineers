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
using VRageMath;

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
        private Vector3D velSum;
        private Vector3D posSum;
        private HashSet<MyEntity> entities;

        #endregion

        #region properties

        public HkWorld InternalWorld
        {
            get { return m_internWorld; }
        }

        public Vector3D VelocitySum
        {
            get
            {
                return velSum;
            }
        }
        public Vector3D PositionsSum
        {
            get
            {
                return posSum;
            }
        }

        #endregion

        public Bubble()
        {
            m_internWorld = MyPhysics.CreateHkWorld(200);
            entities = new HashSet<MyEntity>();
        }

        public static void CreateDebugBubble()
        {
            Bubble re = new Bubble();
            re.EntityId = 500;
            re.Save = false;

            re.Physics = new BubblePhysicsBody(re, VRage.Components.RigidBodyFlag.RBF_DISABLE_COLLISION_RESPONSE);
            string prefabName;
            MyDefinitionManager.Static.GetBaseBlockPrefabName(MyCubeSize.Large, false, true, out prefabName);
            MyObjectBuilder_CubeGrid[] gridBuilders = MyPrefabManager.Static.GetGridPrefab("bp");
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
            MySession.LocalCharacter.EnableJetpack(true, false, true);
            MySession.LocalCharacter.EnableDampeners(false, true);
            //re.Physics.LinearVelocity = new Vector3(10, 0, 0);

            MyPhysics.Bubbles.Add(re);
        }

        public override void UpdateAfterSimulation()
        {
            //calculate average position and velocity. posSum and velSum are accumulated in MyPhysics
            Vector3D avgPos = posSum / (double)entities.Count;

            Vector3D avgVel = velSum / (double)entities.Count;

            //don't know if this is needed, it seems to be related to multithreading (which isn't being done)
            m_internWorld.MarkForWrite();

            //remove the average velocity from entities inside the bubble
            //it seems like setting the position on the entity but not on its rigid body doesn't
            //work (the entity moves but then clips back into its previous place), but doing the same with velocity does.
            foreach (MyEntity ent in entities)
            {
                ent.Physics.LinearVelocity -= avgVel;

                //apply position update to rigid bodies
                if (ent.Physics.RigidBody != null)
                {
                    var rb = ent.Physics.RigidBody;
                    rb.Position -= avgPos;
                }
                if (ent.Physics.CharacterProxy != null && ent.Physics.CharacterProxy.CharacterRigidBody != null)
                {
                    var rb = ent.Physics.CharacterProxy.CharacterRigidBody;
                    rb.Position -= avgPos;
                }
            }

            //see above for MarkForWrite
            m_internWorld.UnmarkForWrite();

            // add the average position and velocity to the bubble itself
            Physics.LinearVelocity += avgVel;
            Matrix blmat = PositionComp.WorldMatrix;
            blmat.Translation += avgPos;
            PositionComp.WorldMatrix = blmat;

            //reset the position and velocity sums
            ClearPositionsSum();
            ClearVelocitySum();
        }

        public void AddEntityToBubble(MyEntity entity)
        {
            entity.Physics.Bubble = this;
            entity.Physics.InBubble = true;
            entities.Add(entity);
            entity.OnAddedToScene(null, m_internWorld);

            //MTODO: add location and velocity conversions
        }

        public void AddToVelocitySum(Vector3D amount)
        {
            this.velSum += amount;
        }

        public void AddToPositionsSum(Vector3D amount)
        {
            this.posSum += amount;
        }

        public void ClearVelocitySum()
        {
            this.velSum = Vector3D.Zero;
        }

        public void ClearPositionsSum()
        {
            this.posSum = Vector3D.Zero;
        }
    }
}
