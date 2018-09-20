namespace Redstor.OvfLib
{
    public class Disk
    {
        public Disk(string path, ulong fileSize, DiskFormat format, long capacityMb, ControllerType controllerType)
        {
            Path = path;
            FileSize = fileSize;
            Format = format;
            CapacityMb = capacityMb;
            ControllerType = controllerType;
        }

        public string Path { get; private set; }
        public ulong FileSize { get; private set; }
        public DiskFormat Format { get; private set; }
        public long CapacityMb { get; private set; }
        public ControllerType ControllerType { get; }
    }
}