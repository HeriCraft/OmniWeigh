namespace OmniWeigh.Core.Models
{
    public class HardwareProfile
    {
        public string DriverType { get; set; } = "Mock"; // e.g. "Mock", "GenericAscii"
        public string ComPort { get; set; } = "COM1";
        public int BaudRate { get; set; } = 9600;
        public int DataBits { get; set; } = 8;
        public string Parity { get; set; } = "None"; // None, Odd, Even, Mark, Space
        public string StopBits { get; set; } = "One"; // None, One, Two, OnePointFive
    }
}
