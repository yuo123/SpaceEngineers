﻿using VRage.ObjectBuilders;
using ProtoBuf;

namespace Sandbox.Common.ObjectBuilders
{
    [ProtoContract]
    [MyObjectBuilderDefinition]
    public class MyObjectBuilder_MotorAdvancedRotor : MyObjectBuilder_MotorRotor
    {
    }
}