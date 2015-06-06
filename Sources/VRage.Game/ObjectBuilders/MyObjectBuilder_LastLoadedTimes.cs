﻿using ProtoBuf;
using System;
using VRage.ObjectBuilders;
using VRage.Serialization;

namespace Sandbox.Common.ObjectBuilders
{
    [ProtoContract]
    [MyObjectBuilderDefinition]
    public class MyObjectBuilder_LastLoadedTimes : MyObjectBuilder_Base
    {
        [ProtoMember]
        public SerializableDictionary<string, DateTime> LastLoaded;
    }
}
