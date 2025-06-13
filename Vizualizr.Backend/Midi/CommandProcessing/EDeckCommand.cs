namespace Vizualizr.Backend.Midi.CommandProcessing
{
    public enum EDeckCommand : short
    {   
        Control_CrossFade,
        Control_Scroll,
        Control_TempoShift,
        Control_Seek,
        Control_Pitch,
        Control_Tempo,
        Control_Gain,
        Control_MixVolume,
        Control_High,
        Control_Mid,
        Control_Low,

        Button_Play = Private_ButtonFlag + 1,
        Button_Stop = Private_ButtonFlag + 2,
        Button_PlayPause = Private_ButtonFlag + 3,
        Button_Cue = Private_ButtonFlag + 4,
        Button_Sync = Private_ButtonFlag + 5,
        Button_Master = Private_ButtonFlag + 6,
        Button_CuePoint = Private_ButtonFlag + 7,
        Button_Select = Private_ButtonFlag + 8,
        Button_Load = Private_ButtonFlag + 9,
        Button_ToggleMonitor = Private_ButtonFlag + 10,

        Private_ButtonFlag = 0b0100_0000_0000_0000
    }

    public static class EDeckCommandExtensions
    {
        public static bool IsButton(this EDeckCommand command)
        {
            return ((short)command & (short)EDeckCommand.Private_ButtonFlag) != 0;
        }

        public static bool IsControl(this EDeckCommand command)
        {
            return !IsButton(command);
        }
    }
}
