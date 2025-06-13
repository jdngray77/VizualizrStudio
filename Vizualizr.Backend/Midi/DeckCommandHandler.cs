using Vizualizr.Backend.Audio.Player;
using Vizualizr.Backend.Midi.CommandProcessing;

namespace Vizualizr.Backend.Midi
{
    internal abstract class DeckCommandHandler
    {
        protected Deck? deck;

        protected DeckCommandHandler(
            DeckManager deckm, 
            byte deck, 
            EDeckCommand deckCommand)
        {
            this.deck = deckm.GetDeck(deck);
            Deck = deck;
            DeckCommand = deckCommand;
        }

        public byte Deck { get; init; }

        public EDeckCommand DeckCommand { get; init; }

        public virtual bool CanExecute => deck != null;

        public void HandleNote(bool on, byte channel, byte note, byte velocity)
        {
            if (!CanExecute)
            {
                return;
            }

            DoHandleNote(channel, note, velocity);
        }

        public void HandleControl(byte channel, byte control, byte value)
        {
            if (!CanExecute)
            {
                return;
            }

            DoHandleControl(channel, control, value);
        }

        protected abstract void DoHandleNote(byte channel, byte note, byte velocity);

        protected abstract void DoHandleControl(byte channel, byte control, byte value);
    }
}
