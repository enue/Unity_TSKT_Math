#nullable enable
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT.Math
{
    struct MemoryBuilder<T>
    {
        readonly System.Memory<T> memory;
        int index;
        public System.Memory<T> Memory => memory[..index];

        public MemoryBuilder(System.Memory<T> memory)
        {
            this.memory = memory;
            index = 0;
        }

        public void Add(in T value)
        {
            memory.Span[index] = value;
            ++index;
        }
    }
}

