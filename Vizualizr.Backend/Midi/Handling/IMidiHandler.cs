using RtMidi.Core.Devices;
using RtMidi.Core.Messages;

namespace Vizualizr.Backend.Midi.Handling
{
    public interface IMidiHandler
    {
        public bool IsFor(string controllerName);
        
        void OnNoteOn(IMidiInputDevice sender, in NoteOnMessage msg);
        void OnNoteOff(IMidiInputDevice sender, in NoteOffMessage msg);
        void OnControlChange(IMidiInputDevice sender, in ControlChangeMessage msg);
    }
}