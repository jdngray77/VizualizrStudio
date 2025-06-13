using RtMidi.Core.Devices;
using RtMidi.Core.Messages;
using Vizualizr.Backend.Midi.Handling;
using Vizualizr.MidiMapper.ViewModels;

namespace Vizualizr.MidiMapper
{
    public class MainPageMidiHandler : IMidiHandler
    {
        private MainPageViewModel viewModel;

        public MainPageMidiHandler(MainPageViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool IsFor(string controllerName)
        {
            return viewModel.SelectedDeviceName == controllerName;
        }

        public void OnControlChange(IMidiInputDevice sender, in ControlChangeMessage msg)
        {
            viewModel.LastInput = $"[{sender.Name}]         [CONTROL]         [Channel : {msg.Channel}]         [Control : {msg.Control}]         [Value : {msg.Value}]";
            SelectMapping((byte)(msg.Channel + 1), (byte)msg.Control);
        }

        public void OnNoteOff(IMidiInputDevice sender, in NoteOffMessage msg)
        {
            viewModel.LastInput = $"[{sender.Name}]         [NOTE/BUTTON (OFF)]         [Channel : {msg.Channel}]         [Note : {msg.Key + 1}]         [Velocity : {msg.Velocity}]";
            SelectMapping((byte)(msg.Channel + 1), (byte)(msg.Key + 1));
        }

        public void OnNoteOn(IMidiInputDevice sender, in NoteOnMessage msg)
        {
            viewModel.LastInput = $"[{sender.Name}]         [NOTE/BUTTON (ON)]         [Channel : {msg.Channel}]         [Note : {msg.Key + 1}]         [Velocity : {msg.Velocity}]";
            SelectMapping((byte)(msg.Channel + 1), (byte)(msg.Key + 1));
        }

        private void SelectMapping(byte channel, byte control)
        {
            var x = viewModel.InputMappings.FirstOrDefault(x => x.Channel == channel && x.Control == control);

            if (x != null) 
            {
                return;
            }

            viewModel.SelectedInputMapping = x;
        }
    }
}
