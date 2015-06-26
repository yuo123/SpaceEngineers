﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using VRage;
using VRage.Library.Utils;

namespace VRage.Serialization
{
    // TODO: OP! Create one generic IL, we per-type is not necessary
    // It seems that DynamicMethod cannot be generic, I suppose we'll have to add assembly with set of generic methods pre-generated by Mono.Cecil
    public class BlitSerializer<T> : ISerializer<T>
    {
        unsafe delegate void Writer(ref T data, byte* buffer);
        unsafe delegate void Reader(out T data, byte* buffer);
        unsafe delegate UInt32 Size();

        private static Reader m_reader = GenerateReader();
        private static Writer m_writer = GenerateWriter();

        public static int StructSize = (int)GenerateSize()();
        public static readonly BlitSerializer<T> Default = new BlitSerializer<T>();

        public BlitSerializer()
        {
            MyLibraryUtils.ThrowNonBlittable<T>();
        }

        public unsafe void Serialize(ByteStream destination, ref T data)
        {
            destination.EnsureCapacity(destination.Position + StructSize);
            fixed (byte* writePos = &destination.Data[destination.Position])
            {
                m_writer(ref data, writePos);
            }
            destination.Position += StructSize;
        }

        public unsafe void Deserialize(ByteStream source, out T data)
        {
            source.CheckCapacity(source.Position + StructSize);
            fixed (byte* readPos = &source.Data[source.Position])
            {
                m_reader(out data, readPos);
            }
            source.Position += StructSize;
        }

        public void SerializeList(ByteStream destination, List<T> data)
        {
            int count = data.Count;
            destination.Write7BitEncodedInt(count);
            for (int i = 0; i < count; i++)
			{
                var array = data.GetInternalArray();
                Serialize(destination, ref array[i]);
            }
        }

        public void DeserializeList(ByteStream source, List<T> resultList)
        {
            int count = source.Read7BitEncodedInt();
            if (resultList.Capacity < count)
                resultList.Capacity = count;

            T item;
            for (int i = 0; i < count; i++)
            {
                Deserialize(source, out item);
                resultList.Add(item);
            }
        }

        static Size GenerateSize()
        {
            DynamicMethod m = new DynamicMethod(String.Empty, typeof(UInt32), new Type[] { }, Assembly.GetExecutingAssembly().ManifestModule);
            var gen = m.GetILGenerator();
            gen.Emit(OpCodes.Sizeof, typeof(T));
            gen.Emit(OpCodes.Ret);
            return (Size)m.CreateDelegate(typeof(Size));
        }

        static Writer GenerateWriter()
        {
            DynamicMethod m = new DynamicMethod(String.Empty, null, new Type[] { typeof(T).MakeByRefType(), typeof(byte*) }, Assembly.GetExecutingAssembly().ManifestModule);
            var gen = m.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Sizeof, typeof(T));
            gen.Emit(OpCodes.Cpblk);
            gen.Emit(OpCodes.Ret);
            return (Writer)m.CreateDelegate(typeof(Writer));
        }

        static Reader GenerateReader()
        {
            DynamicMethod m = new DynamicMethod(String.Empty, null, new Type[] { typeof(T).MakeByRefType(), typeof(byte*) }, Assembly.GetExecutingAssembly().ManifestModule);
            var gen = m.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Sizeof, typeof(T));
            gen.Emit(OpCodes.Cpblk);
            gen.Emit(OpCodes.Ret);
            return (Reader)m.CreateDelegate(typeof(Reader));
        }
    }
}
