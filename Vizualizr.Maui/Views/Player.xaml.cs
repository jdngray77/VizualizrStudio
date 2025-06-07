using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ViewModels;
using Vizualizr.Controls;
using Vizualizr.Controls.Waveform;

namespace Vizualizr.Views;

public partial class Player : ContentView
{
    private IWaveform waveform;
    private IView view;
    private GraphicsView? graphicsView;

    object bindingContext;
    PlayerViewModel viewModel;

    public Player()
    {
        InitializeComponent();

        // WaveformFactory factory = new WaveformFactory();
        //
        // (IWaveform waveform, IView waveformView) = factory.CreateWaveform();
        // this.waveform = waveform;
        // this.view = waveformView;
        // graphicsView = (waveformView as GraphicsView);
        //
        // WaveformFrame.AddLogicalChild(view as View);
        // graphicsView?.Invalidate();


        BindingContextChanged += Player_BindingContextChanged;
    }

    private void Player_BindingContextChanged(object? sender, EventArgs e)
    {
        if (bindingContext == BindingContext)
        {
            return;
        }

        bindingContext = BindingContext;

        if (bindingContext != null && viewModel != null)
        {
            viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }

        viewModel = bindingContext as PlayerViewModel;

        if (viewModel == null)
        {
            return;
        }

        viewModel.PropertyChanged += ViewModel_PropertyChanged; ;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(viewModel.Samples))
        {
            WaveformDrawable.Samples = viewModel.Samples;
            WaveformGraphicsView.Invalidate();
        }
        else if (e.PropertyName == nameof(viewModel.ProgressPercentage))
        {
            WaveformDrawable.Progress = viewModel.ProgressPercentage;
            WaveformGraphicsView.Invalidate();
        }
        else if (e.PropertyName == nameof(viewModel.Zoom))
        {
            WaveformDrawable.Zoom = viewModel.Zoom;
            WaveformGraphicsView.Invalidate();
        }
    }
}