using LibVLCSharp.Shared;
using MvvmCross.Platforms.Wpf.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace CastIt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MvxWindow
    {
        //readonly HashSet<RendererItem> _rendererItems = new HashSet<RendererItem>();
        //private static bool loaded = false;
        //LibVLC _libVLC;
        //MediaPlayer _mediaPlayer;
        //RendererDiscoverer _rendererDiscoverer;
        //Media _currentMedia;

        private Storyboard _showWin;
        private Storyboard _hideWin;
        private bool _isCollapsed;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void ToggleWindowHeight(double tabHeight)
        {
            if (!_isCollapsed)
            {
                BeginStoryboard(_hideWin);
                _hideWin.Begin();
            }
            else
            {
                //200 is the minimu height
                (_showWin.Children.First() as DoubleAnimation).To = tabHeight + 200;
                _showWin.Begin();
            }
            _isCollapsed = !_isCollapsed;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void AppMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _showWin = Resources["ShowWinStoryboard"] as Storyboard;
            _hideWin = Resources["HideWinStoryboard"] as Storyboard;
        }

        /*
                protected async override void OnActivated(EventArgs e)
                {
                    base.OnActivated(e);

                    if (loaded)
                        return;

                    // start chromecast discovery
                    DiscoverChromecasts();

                    // hold on a bit at first to give libvlc time to find the chromecast
                    await Task.Delay(2000);

                    // start casting if any renderer found
                    StartCasting();

                    loaded = true;
                }

                private void StartCasting()
                {
                    // abort casting if no renderer items were found
                    if (!_rendererItems.Any())
                    {
                        Debug.WriteLine("No renderer items found. Abort casting...");
                        return;
                    }

                    // create the mediaplayer
                    _mediaPlayer = new MediaPlayer(_libVLC);

                    // set the previously discovered renderer item (chromecast) on the mediaplayer
                    // if you set it to null, it will start to render normally (i.e. locally) again
                    _mediaPlayer.SetRenderer(_rendererItems.First());
                    _mediaPlayer.Playing += Playing;
                    _mediaPlayer.EndReached += EndReached;
                    _mediaPlayer.MediaChanged += MediaChanged;
                    _mediaPlayer.PositionChanged += PositionChanged;
                    _mediaPlayer.TimeChanged += TimeChanged;
                }

                bool DiscoverChromecasts()
                {
                    // load native libvlc libraries
                    Core.Initialize();

                    // create core libvlc object
                    _libVLC = new LibVLC("--verbose=2");

                    // choose the correct service discovery protocol depending on the host platform
                    // Apple platforms use the Bonjour protocol
                    RendererDescription renderer;

                    renderer = _libVLC.RendererList.FirstOrDefault();

                    // create a renderer discoverer
                    _rendererDiscoverer = new RendererDiscoverer(_libVLC, renderer.Name);

                    // register callback when a new renderer is found
                    _rendererDiscoverer.ItemAdded += RendererDiscoverer_ItemAdded;

                    // start discovery on the local network
                    return _rendererDiscoverer.Start();
                }

                void RendererDiscoverer_ItemAdded(object sender, RendererDiscovererItemAddedEventArgs e)
                {
                    Debug.WriteLine($"New item discovered: {e.RendererItem.Name} of type {e.RendererItem.Type}");
                    if (e.RendererItem.CanRenderVideo)
                        Debug.WriteLine("Can render video");
                    if (e.RendererItem.CanRenderAudio)
                        Debug.WriteLine("Can render audio");

                    // add newly found renderer item to local collection
                    _rendererItems.Add(e.RendererItem);
                }

                private void TogglePlayback(object sender, EventArgs e)
                {
                    if (_mediaPlayer.IsPlaying)
                    {
                        _mediaPlayer.Pause();
                    }
                    else
                    {
                        _mediaPlayer.Play();
                    }
                }

                private void StartPlay(object sender, EventArgs e)
                {
                    var path = filePath.Text;
                    if (!File.Exists(path))
                    {
                        Debug.WriteLine($"Path = {path} doesnt exists");
                        //return;
                    };

                    // create new media
                    _currentMedia?.Dispose();
                    _currentMedia = new Media(_libVLC,
                        path,
                        FromType.FromPath);

                    // start the playback
                    _mediaPlayer.Play(_currentMedia);
                }

                private void StopPlayback(object sender, EventArgs e)
                {
                    _mediaPlayer.Stop();
                }

                private void GoToPosition(object sender, EventArgs e)
                {
                    try
                    {
                        bool wasParsed = float.TryParse(positionValue.Text, out float position);
                        if (!wasParsed)
                        {
                            Debug.WriteLine("Could not parse this position");
                            return;
                        }
                        _mediaPlayer.Position = position;
                    }
                    catch (Exception ex)
                    {
                    }

                }

                private void GoToTime(object sender, EventArgs e)
                {
                    try
                    {
                        bool wasParsed = long.TryParse(timeValue.Text, out long time);
                        if (!wasParsed)
                        {
                            Debug.WriteLine("Could not parse this time");
                            return;
                        }
                        var seconds = _mediaPlayer.Time / 1000;
                        Debug.WriteLine($"Time to forward / backward is {time}. File has been played for = {seconds} seconds");
                        seconds += time;
                        Debug.WriteLine($"The new seconds are = {seconds}");
                        if (seconds > 0)
                        {
                            _mediaPlayer.Time = seconds * 1000;
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }

                private void Playing(object sender, EventArgs e)
                {
                    Debug.WriteLine("Playing");
                }

                private void EndReached(object sender, EventArgs e)
                {
                    Debug.WriteLine("End reached");
                }

                private void MediaChanged(object sender, MediaPlayerMediaChangedEventArgs e)
                {
                    //Debug.WriteLine($"Media changed = {e.Media.ToString()}");
                }

                private void PositionChanged(object sender, MediaPlayerPositionChangedEventArgs e)
                {
                    //Debug.WriteLine($"Position changed = {e.Position}");
                    Dispatcher.Invoke(() =>
                    {
                        position.Content = (e.Position * 100).ToString();
                    });

                    //Device.BeginInvokeOnMainThread(() =>
                    //{
                    //    position.Text = e.Position.ToString();
                    //});
                }

                private void TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
                {
                    //Debug.WriteLine($"Time changed = {e.Time}");
                    Dispatcher.Invoke(() =>
                    {
                        time.Content = (e.Time / 1000).ToString();
                    });
                }

                private void CleanThemAll(object sender, EventArgs e)
                {
                    Clean();
                }

                private void Clean()
                {
                    _mediaPlayer.Stop();
                    _currentMedia?.Dispose();
                    _rendererDiscoverer.Stop();
                    _mediaPlayer.Dispose();
                    _rendererDiscoverer.Dispose();
                    _libVLC.Dispose();
                }
            }
            */
    }
}
