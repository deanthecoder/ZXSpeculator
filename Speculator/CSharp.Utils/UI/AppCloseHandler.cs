// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any non-commercial
// purpose.
//
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
//
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.

using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Material.Icons;

namespace CSharp.Utils.UI;

/// <summary>
/// Closing an app, especially on the Mac, is a bit of a mess.
/// Closing the main app window triggers close events that can be cancelled.
/// Closing using the Mac app menu or Command-Q will close the app window
/// and kill the UI thread, preventing the window from triggering a confirmation
/// dialog.
///
/// This class ensures both types of closure play nicely with each other.
/// </summary>
public class AppCloseHandler
{
    private bool m_isCloseConfirmed;
    private bool m_isConfirmationDialogActive;
    private IClassicDesktopStyleApplicationLifetime m_desktop;

    public static AppCloseHandler Instance { get; } = new AppCloseHandler();

    /// <summary>
    /// Call from App.axaml.cs as early as possible.
    /// </summary>
    public void Init(IClassicDesktopStyleApplicationLifetime desktop)
    {
        m_desktop = desktop;

        // Triggered when the user hits Command-Q or uses the Mac app menu.
        desktop.ShutdownRequested += (_, args) =>
        {
            if (m_isCloseConfirmed)
                return;

            args.Cancel = true;
            PromptForConfirmationAsync();
        };
    }

    /// <summary>
    /// Subscribe this to the main window's Closing event.
    /// </summary>
    public void OnMainWindowClosing(WindowClosingEventArgs args)
    {
        if (m_isCloseConfirmed)
            return; // Allow the close.

        args.Cancel = true;
        if (!m_isConfirmationDialogActive)
        {
            // User hasn't seen the confirmation dialog yet, so show it.
            PromptForConfirmationAsync();
        }
    }

    private void PromptForConfirmationAsync()
    {
        m_isConfirmationDialogActive = true;
        DialogService.Instance.Warn(
            "Confirm Exit?",
            "Any unsaved changes will be lost.",
            "CANCEL",
            "EXIT",
            confirmed =>
            {
                m_isConfirmationDialogActive = false;
                if (confirmed)
                {
                    m_isCloseConfirmed = true;
                    m_desktop.Shutdown(); // Repeat the shutdown request, confirmed this time.
                }
            },
            MaterialIconKind.CloseBold);
    }
}