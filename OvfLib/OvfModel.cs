using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Redstor.OvfLib
{
    public class OvfModel
    {
        private readonly EnvelopeType envelope;
        private readonly IDictionary<DiskFormat, string> formatStringLookup = new Dictionary<DiskFormat, string>
        {
            { DiskFormat.VmdkSparse, "http://www.vmware.com/interfaces/specifications/vmdk.html#streamOptimized" }
        };

        public OvfModel(string vmName, int numCpus, int memoryMb, IList<Disk> diskModels)
        {
            var diskFiles = diskModels.Select((d, i) => new File_Type {href = "file://" + d.Path, id = "diskFile" + i, sizeSpecified = true, size=d.FileSize }).ToList();
            var disks = diskFiles.Select((f, i) => new VirtualDiskDesc_Type {fileRef = f.id, diskId = "diskId" + i, capacity = diskModels[i].CapacityMb.ToString(), format = formatStringLookup[diskModels[i].Format], capacityAllocationUnits = "byte * 2^20" })
                .ToList();

            var diskDrives = disks.Select(d =>
                new RASD_Type
                {
                    AutomaticAllocation = new cimBoolean {Value = false},
                    AutomaticDeallocation = new cimBoolean {Value = false},
                    InstanceID = new cimString {Value = d.diskId},
                    ElementName = new cimString {Value = "Disk " + d.diskId},
                    ResourceType = new ResourceType {Value = "17"},
                    HostResource = new[] {new cimString {Value = "ovf:/disk/" + d.diskId}}
                }
            );

            var systemHardware = new[]
            {
                new RASD_Type
                {
                    AllocationUnits = new cimString { Value = "hertz * 10 ^ 6" },
                    AutomaticAllocation = new cimBoolean {Value = false},
                    AutomaticDeallocation = new cimBoolean { Value = false},
                    InstanceID = new cimString {Value = "cpu"},
                    ElementName = new cimString {Value = "Virtual CPUs"},
                    ResourceType = new ResourceType {Value = "3"},
                    VirtualQuantity = new cimUnsignedLong {Value = (ulong)numCpus}
                },
                new RASD_Type
                {
                    AllocationUnits = new cimString { Value = "byte * 2^20" },
                    AutomaticAllocation = new cimBoolean {Value = false},
                    AutomaticDeallocation = new cimBoolean { Value = false},
                    InstanceID = new cimString {Value = "memory"},
                    ElementName = new cimString {Value = "Memory"},
                    ResourceType = new ResourceType {Value = "4"},
                    VirtualQuantity = new cimUnsignedLong {Value = (ulong)memoryMb}
                }
            };

            var hardwareItems = diskDrives.Union(systemHardware).ToArray();
            envelope = new EnvelopeType()
            {
                Item = new VirtualSystem_Type
                {
                    id = "vm1",
                    Info = new Msg_Type
                    {
                        Value = "A virtual system"
                    },
                    Name = new Msg_Type()
                    {
                        Value = vmName
                    },
                    Items = new []
                    {
                        new VirtualHardwareSection_Type
                        {
                            Info = new Msg_Type
                            {
                                Value = "Hardware"
                            },
                            Item = hardwareItems
                        }
                    }
                },
                References = new References_Type
                {
                    File = diskFiles.ToArray()
                },
                Items = new []
                {
                    new DiskSection_Type
                    {
                        Info = new Msg_Type
                        {
                            Value = "Disks"
                        },
                        Disk = disks.ToArray()
                    }
                }
            };
        }

        public void WriteToStream(Stream stream)
        {
            var serializer = new XmlSerializer(typeof(EnvelopeType));
            serializer.Serialize(stream, envelope);
        }
    }
}
