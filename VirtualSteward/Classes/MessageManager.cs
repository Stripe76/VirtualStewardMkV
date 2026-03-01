using ShadUI;
using VirtualSteward.Features.ProgressBar.ViewModel;

namespace VirtualSteward.Classes;

public class MessageManager( ToastManager toastManager )
{
    public void ShowSuccess( string text,string? title = null )
    {
        toastManager.CreateToast( text ).WithDelay(2).ShowSuccess();
    }
    public void ShowError( string title,string message )
    {
        toastManager.CreateToast( title ).WithContent( message ).WithDelay(1024).ShowError( );
    }
    public void ShowInformation(string text, string? title = null)
    {
        toastManager.CreateToast( text ).Show();
    }

    public void ShowProgress( string text,VMProgress progress )
    {
        toastManager.CreateToast( text ).WithContent(progress).WithDelay(2048).WithDismissListen(progress).Show();
    }
}