using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Redstor.OvfLib
{
    public class OvfModel
    {
        protected readonly EnvelopeType envelope;

        public OvfModel(string vmName, int numCpus, int memoryMb, IList<Disk> diskModels, string network, OperatingSystemType osType)
        {
            var diskElements = new DiskElementsFactory().CreateDiskModels(diskModels);

            var systemHardware = GetBaseHardware(numCpus, memoryMb, network);

            var hardwareItems = diskElements.DiskDrives.Union(systemHardware).ToArray();
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
                    Items = new[]
                    {
                        new VirtualHardwareSection_Type
                        {
                            Info = new Msg_Type
                            {
                                Value = "Hardware"
                            },
                            Item = hardwareItems
                        },
                        CreateOperatingSystemSection(osType)
                    },
                },
                References = new References_Type
                {
                    File = diskElements.DiskFiles.ToArray()
                },
                Items = new Section_Type[]
                {
                    new DiskSection_Type
                    {
                        Info = new Msg_Type
                        {
                            Value = "Disks"
                        },
                        Disk = diskElements.Disks.ToArray()
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

        private static IList<RASD_Type> GetBaseHardware(int numCpus, int memoryMb, string network)
        {
            var systemHardware = new List<RASD_Type>
            {
                new RASD_Type
                {
                    AllocationUnits = new cimString {Value = "hertz * 10 ^ 6"},
                    InstanceID = new cimString {Value = "cpu"},
                    ElementName = new cimString {Value = "Virtual CPUs"},
                    ResourceType = new ResourceType {Value = "3"},
                    VirtualQuantity = new cimUnsignedLong {Value = (ulong) numCpus}
                },
                new RASD_Type
                {
                    AllocationUnits = new cimString {Value = "byte * 2^20"},
                    InstanceID = new cimString {Value = "memory"},
                    ElementName = new cimString {Value = "Memory"},
                    ResourceType = new ResourceType {Value = "4"},
                    VirtualQuantity = new cimUnsignedLong {Value = (ulong) memoryMb}
                },
                new RASD_Type
                {
                    InstanceID = new cimString {Value = "ide0"},
                    ElementName = new cimString {Value = "IDE"},
                    ResourceType = new ResourceType {Value = "5"}
                },
                new RASD_Type
                {
                    InstanceID = new cimString {Value = "ide1"},
                    ElementName = new cimString {Value = "IDE"},
                    ResourceType = new ResourceType {Value = "5"}
                },
                new RASD_Type
                {
                    InstanceID = new cimString {Value = "scsi0"},
                    ElementName = new cimString {Value = "SCSI"},
                    ResourceType = new ResourceType {Value = "6"},
                    ResourceSubType = new cimString {Value = "lsilogicsas"}
                }
            };

            if (!string.IsNullOrWhiteSpace(network))
            {
                systemHardware.Add(
                    new RASD_Type
                    {
                        AutomaticAllocation = new cimBoolean {Value = true},
                        InstanceID = new cimString {Value = "network"},
                        ElementName = new cimString {Value = "Network adapter 1"},
                        ResourceType = new ResourceType {Value = "10"},
                        ResourceSubType = new cimString {Value = "E1000"},
                        VirtualQuantity = new cimUnsignedLong {Value = 1},
                        Connection = new[] {new cimString {Value = network}}
                    });
            }

            return systemHardware;
        }

        private static Section_Type CreateOperatingSystemSection(OperatingSystemType osType)
        {
            var section = new OperatingSystemSection_Type();

            switch (osType)
            {
                case OperatingSystemType.Windows2008R2:
                    section.id = 103;
                    section.Info = new Msg_Type {Value = "Windows Server 2008 R2"};
                    break;
                case OperatingSystemType.Windows2012:
                    section.id = 115;
                    section.Info = new Msg_Type { Value = "Windows Server 2012" };
                    break;
                case OperatingSystemType.Windows7x86:
                    section.id = 105;
                    section.Info = new Msg_Type { Value = "Windows 7 32-bit" };
                    break;
                case OperatingSystemType.Windows7x64:
                    section.id = 105;
                    section.version = "64";
                    section.Info = new Msg_Type { Value = "Windows 7 64-bit" };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return section;
        }

        public void WriteToStream(Stream stream)
        {
            var serializer = new XmlSerializer(typeof(EnvelopeType));
            serializer.Serialize(stream, envelope);
        }
    }
}
