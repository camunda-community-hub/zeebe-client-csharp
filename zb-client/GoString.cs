using System;
using System.Runtime.InteropServices;

namespace zbclient
{
    [StructLayout(LayoutKind.Sequential)]
    public struct GoString
    {
        public string value;
        public long length;

        public GoString(string value)
        {
            this.value = value;
            this.length = value.Length;
        }
    }
}
