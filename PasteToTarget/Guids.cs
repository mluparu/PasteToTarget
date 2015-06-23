// Guids.cs
// MUST match guids.h
using System;

namespace Microsoft.PasteToTarget
{
    static class GuidList
    {
        public const string guidPasteToTargetPkgString = "9bf684bc-af38-4ade-85b3-984586f64c8c";
        public const string guidPasteToTargetCmdSetString = "5a1062c0-3b4f-400d-8eaa-1cab5426de5b";

        public static readonly Guid guidPasteToTargetCmdSet = new Guid(guidPasteToTargetCmdSetString);
    };
}