using RtMidi.Core.Devices;
using RtMidi.Core.Messages;
using Vizualizr.Backend.Midi.FileModel;
using Vizualizr.Backend.Midi.Model;

namespace Vizualizr.Backend.Midi.Handling
{
    public class MappingMidiHandler : IMidiHandler
    {
        private readonly MidiMapping _mapping;

        public MappingMidiHandler(MidiMapping mapping)
        {
            _mapping = mapping;
        }

        public bool IsFor(string controllerName)
        {
            return _mapping.Meta.DeviceName == controllerName;
        }

        public void OnNoteOn(IMidiInputDevice sender, in NoteOnMessage msg)
        {
            foreach (var note in _mapping.Inputs.OfType<NoteInputMapping>())
            {
                if (note.Channel == (byte)msg.Channel + 1 && note.Control == (byte)msg.Key)
                {
                    Console.WriteLine($"NoteOn matched: {note.MapsTo} (channel: {msg.Channel}, key: {msg.Key})");
                    // TODO: Dispatch command to system (e.g., play track, toggle FX)
                    return;
                }
            }
            
            Console.WriteLine($"NoteOn unmatched: (channel: {msg.Channel}, key: {msg.Key}, velocity: {msg.Velocity})");
        }

        public void OnNoteOff(IMidiInputDevice sender, in NoteOffMessage msg)
        {
            foreach (var note in _mapping.Inputs.OfType<NoteInputMapping>())
            {
                if (note.Channel == (byte)msg.Channel + 1 && note.Control == (byte)msg.Key)
                {
                    Console.WriteLine($"NoteOff matched: {note.MapsTo}");
                    // Optional: toggle off visual cue or stop hold behavior
                    return;
                }
            }
            Console.WriteLine($"NoteOff unmatched: (channel: {msg.Channel}, key: {msg.Key}, velocity: {msg.Velocity})");
        }

        public void OnControlChange(IMidiInputDevice sender, in ControlChangeMessage msg)
        {
            foreach (var ctrl in _mapping.Inputs.OfType<ControlInputMapping>())
            {
                if (ctrl.Channel == (byte)msg.Channel + 1 && ctrl.Control == msg.Control)
                {
                    int interpretedValue;

                    if (ctrl.Absolute)
                    {
                        interpretedValue = msg.Value;
                    }
                    else
                    {
                        interpretedValue = msg.Value == ctrl.IncrementValue ? 1 :
                            msg.Value == ctrl.DecrementValue ? -1 : 0;
                    }

                    Console.WriteLine($"Control matched: {ctrl.MapsTo} -> delta={interpretedValue}");
                    // TODO: apply interpretedValue to gain/pitch/etc
                    return;
                }
            }
            
            Console.WriteLine($"ControlChange unmatched: (channel: {msg.Channel}, control: {msg.Control}, Value: {msg.Value})");
        }
    }
}