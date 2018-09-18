using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Redstor.OvfLib.VMWare;

public partial class OperatingSystemSection_Type
{
    [XmlAttribute(Namespace = "http://www.vmware.com/schema/ovf")]
    public GuestOsType osType { get; set; }

    [XmlIgnore]
    public bool osTypeSpecified { get; set; }
}

public partial class VirtualHardwareSection_Type
{
    [XmlElement("Config", typeof(Config), Namespace = "http://www.vmware.com/schema/ovf")]
    public Config[] Config { get; set; }
}

[XmlType(Namespace = "http://www.vmware.com/schema/ovf")]
public class Config
{
    [XmlAttribute(Namespace = "http://schemas.dmtf.org/ovf/envelope/1")]
    public bool required { get; set; }

    [XmlAttribute]
    public string key { get; set; }

    [XmlAttribute]
    public string value { get; set; }
}