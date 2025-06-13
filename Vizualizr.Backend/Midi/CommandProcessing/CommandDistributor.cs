using Vizualizr.Backend.Audio.Player;
using Vizualizr.Backend.Midi.CommandProcessing.CommandHandlers;

namespace Vizualizr.Backend.Midi.CommandProcessing
{
    internal class CommandDistributor
    {
        private IList<IList<DeckCommandHandler>> allDeckHandlers;
        private DeckManager manager;

        public CommandDistributor(DeckManager manager)
        {
            this.manager = manager;

            Populate();
        }

        public void DistributeCommand(
            EDeckCommand e, 
            byte deckIndex, 
            byte channel, 
            byte control, 
            byte value,
            bool noteOn = false)
        {

            var deckHandlers = allDeckHandlers.ElementAtOrDefault(deckIndex);

            if (deckHandlers == null)
            {
                return;
            }

            var handler = deckHandlers.FirstOrDefault(it => it.DeckCommand == e);

            if (handler == null) 
            {
                return;
            }

            if (!handler.CanExecute)
            {
                return;
            }

            if (e.IsButton())
            {
                 handler.HandleNote(noteOn, channel, control, value);
            } 
            else
            {
                handler.HandleControl(channel, control, value);
            }
        }
        private void Populate()
        {
            for (byte i = 0; i < manager.DeckCount(); i++)
            {
                allDeckHandlers.Add(CreateCommandsForDeck(i));
            }
        }

        private IList<DeckCommandHandler> CreateCommandsForDeck(byte deckIndex)
        {
            List<DeckCommandHandler> handlers = new List<DeckCommandHandler>();

            handlers.Add(new PlayPauseDeckCommandHandler(manager, deckIndex));


            return handlers;
        }
    }
}
