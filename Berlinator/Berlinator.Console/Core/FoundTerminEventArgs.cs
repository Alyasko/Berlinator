using System;
using System.Collections.Generic;

namespace Berlinator.Console.Core
{
    public class FoundTerminEventArgs : EventArgs
    {
        public List<DateTime> TerminTimes { get; set; } = new List<DateTime>();

        public string Payload { get; set; } = string.Empty;
    }
}
