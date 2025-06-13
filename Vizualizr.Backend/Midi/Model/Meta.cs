using System.Xml.Serialization;
using Vizualizr.Backend.Utility;


namespace Vizualizr.Backend.Midi.FileModel
{
    public class Meta
    {
        [XmlAttribute]
        public string DeviceName { get; set; }

        public void Validate()
        {
            DeviceName.ThrowIfNull();
        }
    }
}