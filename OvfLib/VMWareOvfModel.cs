using System;
using System.Collections.Generic;
using System.Linq;
using Redstor.OvfLib.VMWare;

namespace Redstor.OvfLib
{
    public class VMWareOvfModel : OvfModel
    {
        public VMWareOvfModel(string vmName, int numCpus, int memoryMb, IList<Disk> diskModels, string network, OperatingSystemType operatingSystemType) : base(vmName, numCpus, memoryMb, diskModels, network, operatingSystemType)
        {
            var operatingSystemSection = envelope.Item.Items.OfType<OperatingSystemSection_Type>().Single();

            var osType = GetOsType(operatingSystemType);

            operatingSystemSection.osType = osType;
            operatingSystemSection.osTypeSpecified = true;
        }

        private GuestOsType GetOsType(OperatingSystemType operatingSystemType)
        {
            switch (operatingSystemType)
            {
                case OperatingSystemType.Windows2008R2:
                    return GuestOsType.windows7Server64Guest;
                case OperatingSystemType.Windows2012:
                    return GuestOsType.windows8Server64Guest;
                case OperatingSystemType.Windows7x86:
                    return GuestOsType.windows7Guest;
                case OperatingSystemType.Windows7x64:
                    return GuestOsType.windows7_64Guest;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operatingSystemType), operatingSystemType, null);
            }
        }
    }
}
