using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Tar;

namespace Redstor.OvfLib
{
    public class OvaModel
    {
        private readonly OvfModel ovfModel;

        public OvaModel(OvfModel ovfModel)
        {
            this.ovfModel = ovfModel;
        }

        public void WriteToStream(Stream stream)
        {
            var archive = TarArchive.CreateOutputTarArchive(stream);

            var ovfFile = Path.GetTempFileName() + ".ovf";
            using (var fileStream = File.Open(ovfFile, FileMode.Create))
            {
                ovfModel.WriteToStream(fileStream);
            }

            archive.RootPath = Path.GetTempPath().Replace('\\', '/');
            archive.WriteEntry(TarEntry.CreateEntryFromFile(ovfFile), true);

            archive.Close();
        }
    }
}
