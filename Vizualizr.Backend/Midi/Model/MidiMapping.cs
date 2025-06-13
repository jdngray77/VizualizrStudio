using System.Xml.Serialization;
using Vizualizr.Backend.Midi.Model;

namespace Vizualizr.Backend.Midi.FileModel
{
    [XmlRoot]
    public class MidiMapping
    {
        public Meta Meta { get; set; }
        
        [XmlArray]
        [XmlArrayItem("Control", typeof(ControlInputMapping))]
        [XmlArrayItem("Note", typeof(NoteInputMapping))]
        public IList<BaseMidiInputMapping> Inputs { get; set; }
    }
}