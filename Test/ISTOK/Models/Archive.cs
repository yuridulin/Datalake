using System;
using System.Runtime.InteropServices;

namespace ISTOK.Models
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct Archive
    {
        public float nRec;

        public DateTime DateTime;

        [MarshalAs(UnmanagedType.ByValArray, IidParameterIndex = 1, SizeConst = 13)]
        public ArchivePart[] Data;
    }
}