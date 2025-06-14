using NAudio.CoreAudioApi;
using RtMidi.Core;
using RtMidi.Core.Devices;
using Vizualizr.Backend.Midi.Handling;

namespace Vizualizr.Backend.Midi
{  
    /// <summary>
    /// MIDI inputs and outputs using RtMidi.
    /// </summary>

    public class MidiIO : IDisposable
    {
        private readonly List<IMidiInputDevice> _devices = new();
        private readonly List<IMidiHandler> _handlers = new();
        private bool _isInitialized = false;

        public void RegisterHandler(IMidiHandler handler)
        {
            if (_handlers.Contains(handler))
            {
                return;
            }

            _handlers.Add(handler);

            if (_isInitialized)
            {
                // Already initialized; add now.
                // If not yet initialised, will be plugged in later.
                foreach (var device in GetDevices())
                {
                    device.NoteOn += handler.OnNoteOn;
                    device.NoteOff += handler.OnNoteOff;
                    device.ControlChange += handler.OnControlChange;
                }
            }
        }
        
        public void DeregisterHandler(IMidiHandler handler)
        {
            if (!_handlers.Contains(handler))
            {
                return;
            }

            _handlers.Remove(handler);

            if (!_isInitialized)
            {
                foreach (var device in GetDevices())
                {
                    try
                    {
                        device.NoteOn -= handler.OnNoteOn;
                    } catch { }
                    
                    try
                    {
                       device.NoteOff -= handler.OnNoteOff;
                    } catch { }
                    
                    try
                    {
                        device.ControlChange -= handler.OnControlChange;
                    } catch { }
                }
            }
        }

        public IMidiInputDevice[] GetDevices()
        {
            return _devices.ToArray();
        }

        public string[] GetDeviceNames()
        {
            return _devices.Select(x => x.Name).ToArray();
        }

        public void Initialize()
        {
            if (_isInitialized) return;

            foreach (var api in MidiDeviceManager.Default.GetAvailableMidiApis())
            {
                Console.WriteLine($"Available API: {api}");
            }

            foreach (var inputDeviceInfo in MidiDeviceManager.Default.InputDevices)
            {
                Console.WriteLine($"Opening {inputDeviceInfo.Name}");

                var device = inputDeviceInfo.CreateDevice();

                // Wire up each registered handler to this device
                foreach (var handler in _handlers)
                {
                    device.NoteOn += handler.OnNoteOn;
                    device.NoteOff += handler.OnNoteOff;
                    device.ControlChange += handler.OnControlChange;
                }

                device.Open();
                _devices.Add(device);
            }

            _isInitialized = true;
        }

        public void Dispose()
        {
            foreach (var device in _devices)
            {
                device.Dispose();
            }

            _devices.Clear();
            _isInitialized = false;
        }
    }
}