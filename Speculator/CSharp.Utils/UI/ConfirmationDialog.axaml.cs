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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Material.Icons;

namespace CSharp.Utils.UI;

public partial class ConfirmationDialog : UserControl
{
    private string m_message;
    public static readonly DirectProperty<ConfirmationDialog, string> MessageProperty = AvaloniaProperty.RegisterDirect<ConfirmationDialog, string>(nameof(Message), o => o.Message, (o, v) => o.Message = v);
    private string m_detail;
    public static readonly DirectProperty<ConfirmationDialog, string> DetailProperty = AvaloniaProperty.RegisterDirect<ConfirmationDialog, string>(nameof(Detail), o => o.Detail, (o, v) => o.Detail = v);
    private string m_cancelButton;
    public static readonly DirectProperty<ConfirmationDialog, string> CancelButtonProperty = AvaloniaProperty.RegisterDirect<ConfirmationDialog, string>(nameof(CancelButton), o => o.CancelButton, (o, v) => o.CancelButton = v);
    private string m_actionButton;
    public static readonly DirectProperty<ConfirmationDialog, string> ActionButtonProperty = AvaloniaProperty.RegisterDirect<ConfirmationDialog, string>(nameof(ActionButton), o => o.ActionButton, (o, v) => o.ActionButton = v);
    private IImmutableSolidColorBrush m_actionBrush;
    public static readonly DirectProperty<ConfirmationDialog, IImmutableSolidColorBrush> ActionBrushProperty = AvaloniaProperty.RegisterDirect<ConfirmationDialog, IImmutableSolidColorBrush>(nameof(ActionBrush), o => o.ActionBrush, (o, v) => o.ActionBrush = v);
    private MaterialIconKind? m_icon;
    public static readonly DirectProperty<ConfirmationDialog, MaterialIconKind?> IconProperty = AvaloniaProperty.RegisterDirect<ConfirmationDialog, MaterialIconKind?>(nameof(Icon), o => o.Icon, (o, v) => o.Icon = v);

    public ConfirmationDialog()
    {
        InitializeComponent();
    }
    
    public string Message
    {
        get => m_message;
        set => SetAndRaise(MessageProperty, ref m_message, value);
    }
    
    public string Detail
    {
        get => m_detail;
        set => SetAndRaise(DetailProperty, ref m_detail, value);
    }
    
    public string CancelButton
    {
        get => m_cancelButton;
        set => SetAndRaise(CancelButtonProperty, ref m_cancelButton, value);
    }
    
    public string ActionButton
    {
        get => m_actionButton;
        set => SetAndRaise(ActionButtonProperty, ref m_actionButton, value);
    }
    
    public IImmutableSolidColorBrush ActionBrush
    {
        get => m_actionBrush;
        set => SetAndRaise(ActionBrushProperty, ref m_actionBrush, value);
    }
    
    public MaterialIconKind? Icon
    {
        get => m_icon;
        set => SetAndRaise(IconProperty, ref m_icon, value);
    }
}