using System;

namespace Berlinator.Console.Core
{
    public class FoundTerminEventArgs : EventArgs
    {
        public DateTime UtcFreeTermin { get; set; }

        public string Payload { get; set; } = string.Empty;
    }
}
