using System.Collections.Generic;

namespace Redstor.OvfLib
{
    internal class DiskElementsFactory
    {
        // https://wiki.abiquo.com/display/ABI38/Template+Compatibility+Table#TemplateCompatibilityTable-SupportedDiskFormatTypes
        private readonly IDictionary<DiskFormat, string> formatStringLookup = new Dictionary<DiskFormat, string>
        {
            { DiskFormat.VmdkStreamOptimized, "http://www.vmware.com/interfaces/specifications/vmdk.html#streamOptimized" },
            { DiskFormat.VmdkSparse, "http://www.vmware.com/interfaces/specifications/vmdk.html#sparse" },
            { DiskFormat.VhdSparse, "http://technet.microsoft.com/en-us/virtualserver/bb676673.aspx#monolithic_sparse" },
            { DiskFormat.VhdxSparse, "http://technet.microsoft.com/en-us/library/hh831446.aspx#monolithic_sparse" },
        };

        public DiskElements CreateDiskModels(IList<Disk> diskModels)
        {
            var diskFiles = new List<File_Type>();
            var disks = new List<VirtualDiskDesc_Type>();
            var diskDrives = new List<RASD_Type>();

            int ideDiskCount = 0;
            int scsiDiskCount = 0;

            for (var i = 0; i < diskModels.Count; i++)
            {
                var diskModel = diskModels[i];
                var diskFile = new File_Type { href = diskModel.Path, id = "diskFile" + i, sizeSpecified = true, size = diskModel.FileSize };
                var disk = new VirtualDiskDesc_Type
                {
                    fileRef = diskFile.id,
                    diskId = "diskId" + i,
                    capacity = diskModels[i].CapacityMb.ToString(),
                    format = formatStringLookup[diskModels[i].Format],
                    capacityAllocationUnits = "byte * 2^20"
                };

                string parent;
                int addressOnParent;
                if (diskModel.ControllerType == ControllerType.IDE)
                {
                    parent = "ide" + ideDiskCount / 2;
                    addressOnParent = ideDiskCount % 2;
                    ideDiskCount++;
                }
                else
                {
                    parent = "scsi0";
                    addressOnParent = scsiDiskCount;
                    scsiDiskCount++;
                }

                var diskDrive = new RASD_Type
                {
                    AddressOnParent = new cimString { Value = addressOnParent.ToString() },
                    AutomaticAllocation = new cimBoolean { Value = false },
                    AutomaticDeallocation = new cimBoolean { Value = false },
                    InstanceID = new cimString { Value = disk.diskId },
                    ElementName = new cimString { Value = "Disk " + disk.diskId },
                    ResourceType = new ResourceType { Value = "17" },
                    HostResource = new[] { new cimString { Value = "ovf:/disk/" + disk.diskId } },
                    Parent = new cimString { Value = parent }
                };

                diskFiles.Add(diskFile);
                disks.Add(disk);
                diskDrives.Add(diskDrive);
            }

            return new DiskElements(diskFiles, disks, diskDrives);
        }
    }

    internal class DiskElements
    {
        public DiskElements(IList<File_Type> diskFiles, IList<VirtualDiskDesc_Type> disks, IList<RASD_Type> diskDrives)
        {
            this.DiskFiles = diskFiles;
            this.Disks = disks;
            this.DiskDrives = diskDrives;
        }

        public IList<File_Type> DiskFiles { get; }
        public IList<VirtualDiskDesc_Type> Disks { get; }
        public IList<RASD_Type> DiskDrives { get; }
    }
}
