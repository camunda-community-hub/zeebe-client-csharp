using System.Runtime.InteropServices;

namespace Zeebe
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct GoString
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
