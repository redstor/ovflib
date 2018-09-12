using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Redstor.OvfLib
{
    public class OvfModel
    {
        private readonly EnvelopeType envelope;
        // https://wiki.abiquo.com/display/ABI38/Template+Compatibility+Table#TemplateCompatibilityTable-SupportedDiskFormatTypes
        private readonly IDictionary<DiskFormat, string> formatStringLookup = new Dictionary<DiskFormat, string>
        {
            { DiskFormat.VmdkStreamOptimized, "http://www.vmware.com/interfaces/specifications/vmdk.html#streamOptimized" },
            { DiskFormat.VmdkSparse, "http://www.vmware.com/interfaces/specifications/vmdk.html#sparse" },
            { DiskFormat.VhdSparse, "http://technet.microsoft.com/en-us/virtualserver/bb676673.aspx#monolithic_sparse" },
            { DiskFormat.VhdxSparse, "http://technet.microsoft.com/en-us/library/hh831446.aspx#monolithic_sparse" },
        };

        public OvfModel(string vmName, int numCpus, int memoryMb, IList<Disk> diskModels, string network)
        {
            var diskFiles = diskModels.Select((d, i) => new File_Type {href = d.Path, id = "diskFile" + i, sizeSpecified = true, size=d.FileSize }).ToList();
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
                },
                new RASD_Type
                {
                    AutomaticAllocation = new cimBoolean {Value = true},
                    AutomaticDeallocation = new cimBoolean { Value = false},
                    InstanceID = new cimString {Value = "network"},
                    ElementName = new cimString {Value = "Network adapter 1"},
                    ResourceType = new ResourceType {Value = "10"},
                    ResourceSubType = new cimString {Value = "E1000"},
                    VirtualQuantity = new cimUnsignedLong {Value = (ulong)memoryMb},
                    Connection = new [] { new cimString {  Value = network} }
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
                Items = new Section_Type[]
                {
                    new DiskSection_Type
                    {
                        Info = new Msg_Type
                        {
                            Value = "Disks"
                        },
                        Disk = disks.ToArray()
                    },
                    new NetworkSection_Type
                    {
                        Info = new Msg_Type
                        {
                            Value = "List of networks"
                        },
                        Network = new []
                        {
                            new NetworkSection_TypeNetwork
                            {
                                name = "VM Network"
                            }
                        }
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
