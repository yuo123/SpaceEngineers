﻿using Sandbox.Common.ObjectBuilders.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Utils;


namespace Sandbox.Definitions
{
    [MyDefinitionType(typeof(MyObjectBuilder_WeaponItemDefinition))]
    public class MyWeaponItemDefinition : MyPhysicalItemDefinition
    {
        public MyDefinitionId WeaponDefinitionId;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);

            var ob = builder as MyObjectBuilder_WeaponItemDefinition;
            MyDebug.AssertDebug(ob != null);

            this.WeaponDefinitionId = new MyDefinitionId(ob.WeaponDefinitionId.Type, ob.WeaponDefinitionId.Subtype);
        }
    }
}
