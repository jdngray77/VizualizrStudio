using Vizualizr.Backend.Midi.FileModel;

namespace Vizualizr.Backend.Midi.Model
{
    public class NoteInputMapping : BaseMidiInputMapping
    {
        // For weird controls that use velocity max and min
        // to indicate that the note is on or off instead of 
        // NoteOff
        public bool UsesVelocityInsteadOfNoteOff { get; set; }
    }
}