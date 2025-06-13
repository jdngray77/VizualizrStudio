// See https://aka.ms/new-console-template for more information

using Vizualizr.Backend.Midi;
using Vizualizr.Backend.Midi.FileModel;
using Vizualizr.Backend.Midi.Handling;
using Vizualizr.Backend.Midi.Model;

MidiMapping mapping = new MidiMapping()
{
    Meta = new Meta()
    {
        DeviceName = "DJHERCULESMIX Universal DJ "
    },

    Inputs = new List<BaseMidiInputMapping>()
    {
        new ControlInputMapping()
        {
            Channel = 1,
            Control = 50,
            MapsTo = "test"
        },
        
        new NoteInputMapping()
        {
            Channel = 1,
            Control = 17,
            MapsTo = "testButton"
        }
    }
};

MappingMidiHandler handler = new MappingMidiHandler(mapping);

var midiIo = new MidiIO();  
midiIo.RegisterHandler(handler);
midiIo.Initialize();

Console.ReadKey();