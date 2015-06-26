﻿using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using SteamSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sandbox.Game.Multiplayer
{
    [PreloadRequired]
    class MySyncUserControllableGun
    {
        MyUserControllableGun m_block = null;
        [MessageIdAttribute(16286, P2PMessageEnum.Reliable)]
        struct ShootOnceMessage : IEntityMessage
        {
            public long EntityId;
            public long GetEntityId() { return EntityId; }
        }

        [MessageIdAttribute(16287, P2PMessageEnum.Reliable)]
        struct BeginShootMessage : IEntityMessage
        {
            public long EntityId;
            public long GetEntityId() { return EntityId; }
        }

        [MessageIdAttribute(16288, P2PMessageEnum.Reliable)]
        struct EndShootMessage : IEntityMessage
        {
            public long EntityId;
            public long GetEntityId() { return EntityId; }
        }

        public MySyncUserControllableGun(MyUserControllableGun block)
        {
            m_block = block;
        }

        static MySyncUserControllableGun()
        {
            MySyncLayer.RegisterMessage<ShootOnceMessage>(ShootOnceSuccess, MyMessagePermissions.Any, MyTransportMessageEnum.Success);
            MySyncLayer.RegisterMessage<BeginShootMessage>(BeginShootSuccess, MyMessagePermissions.Any, MyTransportMessageEnum.Success);
            MySyncLayer.RegisterMessage<EndShootMessage>(EndShootSuccess, MyMessagePermissions.Any, MyTransportMessageEnum.Success);
        }

        static void ShootOnceSuccess(ref ShootOnceMessage msg, MyNetworkClient sender)
        {
            MyUserControllableGun block = null;
            MyEntities.TryGetEntityById(msg.EntityId, out block);
            if (block != null)
            {
                block.Shoot();
            }
        }

        static void BeginShootSuccess(ref BeginShootMessage msg, MyNetworkClient sender)
        {
            MyUserControllableGun block = null;
            MyEntities.TryGetEntityById(msg.EntityId, out block);
            if (block != null)
            {
                block.BeginShoot();
            }
        }

        static void EndShootSuccess(ref EndShootMessage msg, MyNetworkClient sender)
        {
            MyUserControllableGun block = null;
            MyEntities.TryGetEntityById(msg.EntityId, out block);
            if (block != null)
            {
                block.EndShoot();
            }
        }

        public void SendShootOnceMessage()
        {
            m_block.SyncRotationAndOrientation();
            m_block.Shoot();
            var msg = new ShootOnceMessage();
            msg.EntityId = m_block.EntityId;
            Sync.Layer.SendMessageToAll(ref msg, MyTransportMessageEnum.Success);
        }

        public void SendBeginShootMessage()
        {
            m_block.SyncRotationAndOrientation();
            m_block.BeginShoot();
            var msg = new BeginShootMessage();
            msg.EntityId = m_block.EntityId;
            Sync.Layer.SendMessageToAll(ref msg, MyTransportMessageEnum.Success);
        }

        public void SendEndShootMessage()
        {
            m_block.EndShoot();
            var msg = new EndShootMessage();
            msg.EntityId = m_block.EntityId;
            Sync.Layer.SendMessageToAll(ref msg, MyTransportMessageEnum.Success);
        }
    }
}
