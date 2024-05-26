namespace Test
{
    using System.Runtime.InteropServices;

    public unsafe struct UnmanagedArray<T> where T : unmanaged
    {
        public T* Data;
        public int Length;

        public UnmanagedArray(int size)
        {
            Data = (T*)Marshal.AllocHGlobal(size * sizeof(T));
        }

        public T this[int index]
        {
            get => Data[index];
            set => Data[index] = value;
        }

        public T this[uint index]
        {
            get => Data[index];
            set => Data[index] = value;
        }

        public void Resize(int newSize)
        {
            Data = (T*)Marshal.ReAllocHGlobal((nint)Data, newSize * sizeof(T));
            Length = newSize;
        }

        public readonly Span<T> AsSpan()
        {
            return new Span<T>(Data, Length);
        }

        public void Free()
        {
            Marshal.FreeHGlobal((nint)Data);
            Data = null;
            Length = 0;
        }
    }

    public unsafe struct UnmanagedWString
    {
        public char* Data;
        public int Length;

        public UnmanagedWString(int size)
        {
            Data = (char*)Marshal.AllocHGlobal((size + 1) * sizeof(char));
            Data[Length] = '\0';
        }

        public char this[int index]
        {
            get => Data[index];
            set => Data[index] = value;
        }

        public char this[uint index]
        {
            get => Data[index];
            set => Data[index] = value;
        }

        public void Resize(int newSize)
        {
            Data = (char*)Marshal.ReAllocHGlobal((nint)Data, (newSize + 1) * sizeof(char));
            Length = newSize;
            Data[Length] = '\0';
        }

        public void Free()
        {
            Marshal.FreeHGlobal((nint)Data);
            Data = null;
            Length = 0;
        }

        public readonly Span<char> AsSpan()
        {
            return new Span<char>(Data, Length);
        }

        public override readonly string ToString()
        {
            return new(Data);
        }
    }
}