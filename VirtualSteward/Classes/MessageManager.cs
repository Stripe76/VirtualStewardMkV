using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform;
using ShadUI;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Enums;
using SoundFlow.Providers;
using SoundFlow.Structs;
using VirtualSteward.Features.ProgressBar.ViewModel;

namespace VirtualSteward.Classes;

public class MessageManager( ToastManager toastManager )
{
    public void ShowSuccess( string text,string? title = null )
    {
        toastManager.CreateToast( text ).WithDelay(2).ShowSuccess();
    }
    public async Task ShowError( string title,string message )
    {
        toastManager.CreateToast( title ).WithContent( message ).WithDelay(1024).ShowError( );

        await PlaySoundAsync( );
    }
    public void ShowInformation(string text, string? title = null)
    {
        toastManager.CreateToast( text ).Show();
    }

    public void ShowProgress( string text,VMProgress progress )
    {
        toastManager.CreateToast( text ).WithContent(progress).WithDelay(2048).WithDismissListen(progress).Show();
    }

    private MiniAudioEngine? _audioEngine;
    private DeviceInfo? _defaultPlaybackDevice;
    private AudioPlaybackDevice? _device;
    private StreamDataProvider? _dataProvider;
    private SoundPlayer? _player;

    private async Task PlaySoundAsync( )
    {
        if( _audioEngine == null )
        {
            _audioEngine = new MiniAudioEngine( );
            _defaultPlaybackDevice = _audioEngine.PlaybackDevices.FirstOrDefault( d => d.IsDefault );

            if( _defaultPlaybackDevice is null || _defaultPlaybackDevice.Value.Id == IntPtr.Zero )
                return;

            var audioFormat = new AudioFormat
            {
                Format = SampleFormat.F32,
                SampleRate = 48000,
                Channels = 2
            };
            _device = _audioEngine.InitializePlaybackDevice( _defaultPlaybackDevice,audioFormat );

            if( _device != null )
            {
                var uri = new Uri( "avares://VirtualSteward/Assets/error.mp3" );
                if( AssetLoader.Exists( uri ) )
                {
                    var sound = AssetLoader.Open( uri );
                    byte[] buffer = new byte[sound.Length];
                    await sound.ReadExactlyAsync( buffer,0,buffer.Length );

                    _dataProvider = new StreamDataProvider( _audioEngine,audioFormat,new MemoryStream( buffer ) );

                    if( _dataProvider != null )
                    {
                        _player = new SoundPlayer( _audioEngine,audioFormat,_dataProvider );
                        _device.MasterMixer.AddComponent( _player );
                        _device?.Start( );
                    }
                }
            }
        }
        _player?.Stop(  );
        _player?.Play( );
    }
}