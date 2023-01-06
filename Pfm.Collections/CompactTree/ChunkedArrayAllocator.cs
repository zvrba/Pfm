using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Pfm.Collections.CompactTree;

public class ChunkedArrayAllocator<TValue, TTag> : IAllocator<TValue, TTag>
    where TTag : struct
{
    private readonly int chunkBits;
    private readonly int chunkSize;
    private readonly int chunkMask;
    private readonly List<Node<TValue, TTag>[]> chunks;

    Pointer freeList;

    public ChunkedArrayAllocator(int chunkBits) {
        if (chunkBits < 2 || chunkBits > 8)
            throw new ArgumentOutOfRangeException(nameof(chunkBits));
        this.chunkBits = chunkBits;
        this.chunkSize = 1 << chunkBits;
        this.chunkMask = chunkSize - 1;
        this.chunks = new();
    }

    public ref Node<TValue, TTag> this[Pointer pointer] {
        get {
            if (pointer.IsNull)
                throw new NullReferenceException("Cannot dereference a null pointer.");
            var ichunk = (int)(pointer.Bits >> chunkBits);
            var ioffset = pointer.Bits & chunkMask; 
            return ref chunks[ichunk][ioffset];
        }
    }

    public Pointer Allocate() {
        if (freeList.IsNull)
            freeList = NewChunk();
        
        var ret = freeList;
        freeList = this[freeList].L;
        return ret;

        Pointer NewChunk() {
            var ichunk = chunks.Count;
            var chunk = new Node<TValue, TTag>[chunkSize];
            chunks.Add(chunk);

            var o = ichunk * chunkSize;
            for (int i = 0; i < chunkSize - 1; ++i)
                chunk[i].L = new Pointer(o + i + 1);
            
            return new Pointer(o == 0 ? 1 : o);    // Don't return "null pointer".
        }
    }
    
    public void Free(Pointer p) {
        this[p].L = freeList;   // Indexer checks for null
        freeList = p;
    }

    public void Compact(float threshold) => throw new NotSupportedException();
}
