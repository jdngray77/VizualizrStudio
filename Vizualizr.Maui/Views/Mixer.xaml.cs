using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vizualizr.Views;

public partial class Mixer : ContentView
{
    public View DeckA => deckA;
    public View DeckB => deckB;

    public Mixer()
    {
        InitializeComponent();
    }
}