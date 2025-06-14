using RtMidi.Core.Devices;
using RtMidi.Core.Messages;
using Vizualizr.Backend.Midi.Handling;
using Vizualizr.MidiMapper.ViewModels;

namespace Vizualizr.MidiMapper.MIDI
{
    /// <summary>
    /// Notifies the AddInputMappingDialogViewModel when
    /// the user provides midi inputs.
    /// </summary>
    public class AddInputMappingDialogMidiHandler : IMidiHandler
    {
        private AddInputMappingDialogViewModel viewModel;

        public AddInputMappingDialogMidiHandler(AddInputMappingDialogViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool IsFor(string controllerName)
        {
            return viewModel.DeviceName == controllerName;
        }

        public void OnControlChange(IMidiInputDevice sender, in ControlChangeMessage msg)
        {
            LogMapping((byte)(msg.Channel + 1), (byte)msg.Control, EInputMappingType.CC);
        }

        public void OnNoteOff(IMidiInputDevice sender, in NoteOffMessage msg)
        {
            LogMapping((byte)(msg.Channel + 1), (byte)(msg.Key + 1), EInputMappingType.Note);
        }

        public void OnNoteOn(IMidiInputDevice sender, in NoteOnMessage msg)
        {
            LogMapping((byte)(msg.Channel + 1), (byte)(msg.Key + 1), EInputMappingType.Note);
        }

        private void LogMapping(byte channel, byte control, EInputMappingType type)
        {
            viewModel.ReceivedNewInput(new InputMappingViewModel()
            {
                Channel = channel,
                Control = control,
                Type = type
            });
        }
    }
}
