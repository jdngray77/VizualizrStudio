using System.Xml.Serialization;
using Vizualizr.Backend.Midi.CommandProcessing;
using Vizualizr.Backend.Midi.Model;

namespace Vizualizr.Backend.Midi.FileModel
{
    /// <summary>
    /// Attributes that apply to all midi mappings.
    /// </summary>
    [XmlInclude(typeof(ControlInputMapping))]
    [XmlInclude(typeof(NoteInputMapping))]
    public abstract class BaseMidiInputMapping
    {
        [XmlAttribute]
        public byte Deck { get; set; }

        [XmlAttribute]
        public byte Channel { get; set; }
        
        [XmlAttribute]
        public byte Control { get; set; }
        
        [XmlAttribute]
        public EDeckCommand MapsTo { get; set; }
    }
}