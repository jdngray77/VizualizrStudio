using RtMidi.Core.Devices;
using Vizualizr.Backend.Midi;
using Vizualizr.Backend.Midi.Handling;

namespace Vizualizr.MidiMapper
{
    public class DeviceService
    {
        private MidiIO io;

        public DeviceService(MidiIO io)
        {
            this.io = io;
        }

        public string[] GetDeviceNames()
        {
            return io.GetDeviceNames();
        }
        public void AddHandler(IMidiHandler handler)
        {
            io.RegisterHandler(handler);
        }

        internal void Initialize()
        {
            io.Initialize();
        }
    }
}
