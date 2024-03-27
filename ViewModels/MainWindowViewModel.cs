using Avalonia.Collections;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using LibVLCSharp.Shared;
using MusicPlayer.Models;
using ReactiveUI;

namespace MusicPlayer.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly Window _mainWindow;
        private readonly LibVLC _libVlc;
        private readonly MediaPlayer _mediaPlayer;
        private Track _currentTrack;
        private double _currentPosition;
        private double _duration;

        public AvaloniaList<Track> Playlist { get; } = new AvaloniaList<Track>();

        public Track CurrentTrack
        {
            get => _currentTrack;
            set => this.RaiseAndSetIfChanged(ref _currentTrack, value);
        }

        public double CurrentPosition
        {
            get => _currentPosition;
            set => this.RaiseAndSetIfChanged(ref _currentPosition, value);
        }

        public double Duration
        {
            get => _duration;
            set => this.RaiseAndSetIfChanged(ref _duration, value);
        }

        public ICommand PreviousCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand OpenFilesCommand { get; }

        public MainWindowViewModel(Window mainWindow, Track currentTrack)
        {
            _mainWindow = mainWindow;
            _currentTrack = currentTrack;

            Core.Initialize();
            _libVlc = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVlc);

            PreviousCommand = ReactiveCommand.Create(Previous);
            PlayCommand = ReactiveCommand.Create(Play);
            PauseCommand = ReactiveCommand.Create(Pause);
            StopCommand = ReactiveCommand.Create(Stop);
            NextCommand = ReactiveCommand.Create(Next);
            OpenFilesCommand = ReactiveCommand.Create(OpenFiles);

            _mediaPlayer.PositionChanged += (sender, e) =>
            {
                CurrentPosition = _mediaPlayer.Position;
            };

            _mediaPlayer.LengthChanged += (sender, e) =>
            {
                Duration = _mediaPlayer.Length;
            };
        }

        private async void OpenFiles()
        {
            var options = new FilePickerOpenOptions
            {
                AllowMultiple = true,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Audio Files")
                    {
                        Patterns = new[] { "*.mp3", "*.wav", "*.flac" }
                    }
                }
            };

            var result = await _mainWindow.StorageProvider.OpenFilePickerAsync(options);

            foreach (var file in result)
            {
                var track = new Track
                {
                    Title = System.IO.Path.GetFileNameWithoutExtension(file.Name),
                    Artist = "Unknown",
                    FilePath = file.Path.LocalPath
                };
                Playlist.Add(track);
            }

            if (Playlist.Count > 0)
            {
                CurrentTrack = Playlist.First();
            }
        }

        private void Play()
        {
            if (!_mediaPlayer.IsPlaying)
            {
                if (CurrentTrack.FilePath != null)
                {
                    _mediaPlayer.Play(new Media(_libVlc, CurrentTrack.FilePath));

                    if (_mediaPlayer.State == VLCState.Ended)
                    {
                        Next();
                    }

                    if (_mediaPlayer.State == VLCState.Error)
                    {
                        Stop();
                    }
                }
            }
        }

        private void Pause()
        {
            if (_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Pause();
            }
        }

        private void Stop()
        {
            _mediaPlayer.Stop();
            CurrentPosition = 0; // Это может потребовать дополнительной логики для синхронизации с UI
        }

        private void Next()
        {
            var currentIndex = Playlist.IndexOf(CurrentTrack);
            if (currentIndex >= 0 && currentIndex < Playlist.Count - 1)
            {
                CurrentTrack = Playlist[currentIndex + 1];
                _mediaPlayer.Stop();

                if (CurrentTrack.FilePath != null)
                {
                    _mediaPlayer.Play(new Media(_libVlc, CurrentTrack.FilePath));
                }
            }
        }

        private void Previous()
        {
            var currentIndex = Playlist.IndexOf(CurrentTrack);
            if (currentIndex > 0)
            {
                CurrentTrack = Playlist[currentIndex - 1];
                _mediaPlayer.Stop();

                if (CurrentTrack.FilePath != null)
                {
                    _mediaPlayer.Play(new Media(_libVlc, CurrentTrack.FilePath));
                }
            }
        }

    }
}