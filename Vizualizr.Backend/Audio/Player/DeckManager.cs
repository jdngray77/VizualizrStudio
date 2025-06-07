using System.Collections.Immutable;

namespace Vizualizr.Backend.Audio.Player
{
    /// <summary>
    /// Creates and manages track players.
    /// </summary>
    public class DeckManager
    {
        private IList<Deck> decks = new List<Deck>();
        
        private readonly AudioHypervisor audioHypervisor;

        public DeckManager(AudioHypervisor audioHypervisor)
        {
            this.audioHypervisor = audioHypervisor;
        }

        public Deck CreateDeck()
        {
            var player = new Deck(audioHypervisor);
            decks.Add(player);
            return player;
        }

        public IList<Deck> GetDecks()
        {
            return decks.ToImmutableList();
        }
    }
}