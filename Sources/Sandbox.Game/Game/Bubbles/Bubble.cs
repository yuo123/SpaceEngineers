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
using Sandbox.ModAPI;
using VRage.ModAPI;
using VRage;

namespace Sandbox.Game.Bubbles
{
    public class Bubble : MyEntity
    {
        /*
        //MTODO: To-do list:
         Implement bubble "transitions" inside UpdateAfterSimulation
         don't execute "transitions"  every frame. implement a counter.
        */

        #region fields

        protected HkWorld m_internWorld;
        protected Vector3D velSum;
        protected Vector3D posSum;
        protected HashSet<MyEntity> m_entities;
        protected BoundingBoxD extents;

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

        public BoundingBoxD Extents
        {
            get
            {
                return extents;
            }
        }

        #endregion

        public Bubble()
        {
            m_internWorld = MyPhysics.CreateHkWorld(200);
            m_entities = new HashSet<MyEntity>();
            extents = new BoundingBoxD();
            Physics = new BubblePhysicsBody(this, VRage.Components.RigidBodyFlag.RBF_DISABLE_COLLISION_RESPONSE);
            Save = false;
            //because of a lack of documentation, I don't know how this value should be assgined, but this seems to work.
            EntityId = MyEntityIdentifier.AllocateId();
        }

        public static void CreateDebugBubble()
        {
            Bubble re = new Bubble();

            string prefabName;
            MyDefinitionManager.Static.GetBaseBlockPrefabName(MyCubeSize.Large, false, true, out prefabName);
            MyObjectBuilder_CubeGrid[] gridBuilders = MyPrefabManager.Static.GetGridPrefab("Fighter");
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
            //MySession.LocalCharacter.EnableDampeners(false, true);

            MyPhysics.Bubbles.Add(re);
        }

        public override void UpdateAfterSimulation()
        {
            if (m_entities.Count == 0)
            {
                Delete();
                return;
            } 
            //MySession.LocalCharacter.Physics.SetLocalPosition(MySession.LocalCharacter.Physics.GetLocalMatrix().Translation + 0.1);
            
            List<MyEntity> toBeRemoved = new List<MyEntity>();

            //calculate average position and velocity. posSum and velSum are accumulated in Entity's UpdateAfterSimulation
            Vector3 avgPos = posSum / (double)m_entities.Count;

            Vector3 avgVel = velSum / (double)m_entities.Count;

            // add the average position and velocity to the bubble itself
            Physics.LinearVelocity += avgVel;
            Matrix blmat = PositionComp.WorldMatrix;
            blmat.Translation += avgPos;
            PositionComp.WorldMatrix = blmat;

            //remove the average velocity from entities inside the bubble
            //it seems like setting the position on the entity but not on its rigid body doesn't
            //work (the entity moves but then snaps back into its previous place), but doing the same with velocity does.
            foreach (MyEntity ent in m_entities)
            {
                //don't know if this is needed, it seems to be related to multithreading (which isn't being done)
                m_internWorld.UnmarkForWrite();

                if (avgVel != Vector3.Zero)
                    ent.Physics.LinearVelocity -= avgVel;

                if (avgPos != Vector3.Zero)
                {
                    //apply position update to rigid bodies
                    ent.Physics.SetLocalPosition(ent.Physics.GetLocalMatrix().Translation + Vector3.TransformNormal(ent.Physics.Center, ent.WorldMatrix) - avgPos);
                    ent.Physics.OnLocalPositionChanged();
                }

                //see above for MarkForWrite
                m_internWorld.MarkForWrite();

                //check if entity can still be in bubble, and if not, find another bubble for it.
                if (!CanStayInBubble(ent))
                {
                    toBeRemoved.Add(ent);
                }
                else
                {

                    //update Extents
                    Vector3D max = new Vector3D();
                    Vector3D min = new Vector3D();

                    //maximum XYZ
                    if (ent.PositionComp.GetPosition().X > max.X)
                    {
                        max.X = ent.PositionComp.GetPosition().X;
                    }
                    if (ent.PositionComp.GetPosition().Y > max.Y)
                    {
                        max.Y = ent.PositionComp.GetPosition().Y;
                    }
                    if (ent.PositionComp.GetPosition().Z > max.Z)
                    {
                        max.Z = ent.PositionComp.GetPosition().Z;
                    }
                    extents.Max = max;

                    //minimum XYZ
                    if (ent.PositionComp.GetPosition().X < min.X)
                    {
                        min.X = ent.PositionComp.GetPosition().X;
                    }
                    if (ent.PositionComp.GetPosition().Y < min.Y)
                    {
                        min.Y = ent.PositionComp.GetPosition().Y;
                    }
                    if (ent.PositionComp.GetPosition().Z < min.Z)
                    {
                        min.Z = ent.PositionComp.GetPosition().Z;
                    }
                    extents.Min = min;
                }
            }

            //reset the position and velocity sums
            ClearPositionsSum();
            ClearVelocitySum();

            //remove the entities to be removed. this is done here because you can't change a collection during a foreach
            foreach (MyEntity entity in toBeRemoved)
            {
                RemoveEntityAndCompensate(entity);
                MyPhysics.AddEntityToBubble(entity);
            }
                      
        }

        public void AddEntityToBubble(MyEntity entity, bool insertToScene = true)
        {
            entity.Physics.Bubble = this;
            entity.Physics.InBubble = true;
            if (insertToScene)
            {
                m_entities.Add(entity);
                entity.OnAddedToScene(null, m_internWorld);
            }
        }

        public void AddEntityAndCompensate(MyEntity entity, bool insertToScene = true)
        {
            AddEntityToBubble(entity, insertToScene);
            entity.Physics.SetLocalPosition(entity.Physics.GetWorldMatrix().Translation - this.PositionComp.GetPosition());
            entity.Physics.LinearVelocity -= this.Physics.LinearVelocity;
        }

        public void RemoveEntityFromBubble(MyEntity entity)
        {
            m_entities.Remove(entity);
            entity.Physics.Bubble = null;
            entity.OnRemovedFromScene(null, m_internWorld);
        }

        public void RemoveEntityAndCompensate(MyEntity entity)
        {
            RemoveEntityFromBubble(entity);
            entity.Physics.SetLocalPosition(entity.Physics.GetWorldMatrix().Translation + this.PositionComp.GetPosition());
            entity.Physics.LinearVelocity += this.Physics.LinearVelocity;
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

        public bool CanStayInBubble(MyEntity entity)
        {
            if (entity.Physics.GetLocalMatrix().Translation.AbsMax() > 100)
                return false;
            if (entity.Physics.LinearVelocity.AbsMax() > 100)
                return false;
            return true;
        }

        public bool CanBeInBubble(MyEntity entity)
        {
            if ((entity.Physics.GetWorldMatrix().Translation - this.PositionComp.GetPosition()).AbsMax() > 100)
                return false;
            if ((entity.Physics.LinearVelocity - this.Physics.LinearVelocity).AbsMax() > 100)
                return false;
            return true;
        }

        public override void Delete()
        {
            this.MarkedForClose = true;
            base.Delete();
            MyPhysics.Bubbles.Remove(this);
        }
    }
}
