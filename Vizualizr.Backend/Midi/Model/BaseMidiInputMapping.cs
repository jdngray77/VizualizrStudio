using System.Xml.Serialization;

namespace Vizualizr.Backend.Midi.FileModel
{
    /// <summary>
    /// Attributes that apply to all midi mappings.
    /// </summary>
    public abstract class BaseMidiInputMapping
    {
        [XmlAttribute]
        public byte Channel { get; set; }
        
        [XmlAttribute]
        public byte Control { get; set; }
        
        [XmlAttribute]
        public byte Velocity { get; set; }

        [XmlAttribute]
        public string MapsTo { get; set; }
    }
}