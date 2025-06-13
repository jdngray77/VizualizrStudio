using Vizualizr.Backend.Audio.Player;

namespace Vizualizr.Backend.Midi.CommandProcessing.CommandHandlers
{
    internal class PlayPauseDeckCommandHandler : DeckCommandHandler
    {
        public PlayPauseDeckCommandHandler(DeckManager deckm, byte deck) 
            : base(deckm, deck, EDeckCommand.Button_PlayPause)
        {
        }

        protected override void DoHandleControl(byte channel, byte control, byte value)
        {

        }

        protected override void DoHandleNote(byte channel, byte note, byte velocity)
        {
            if (!deck!.TrackLoaded)
            {
                return;
            }

            if (deck.IsPlaying)
            {
                deck.Pause();
            } else
            {
                deck.Play();
            }
        }
    }
}
