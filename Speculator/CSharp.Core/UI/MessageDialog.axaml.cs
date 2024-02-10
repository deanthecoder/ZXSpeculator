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
using Material.Icons;

namespace CSharp.Core.UI;

public partial class MessageDialog : UserControl
{
    private string m_message;
    public static readonly DirectProperty<MessageDialog, string> MessageProperty = AvaloniaProperty.RegisterDirect<MessageDialog, string>(nameof(Message), o => o.Message, (o, v) => o.Message = v);
    private string m_detail;
    public static readonly DirectProperty<MessageDialog, string> DetailProperty = AvaloniaProperty.RegisterDirect<MessageDialog, string>(nameof(Detail), o => o.Detail, (o, v) => o.Detail = v);
    private MaterialIconKind? m_icon;
    public static readonly DirectProperty<MessageDialog, MaterialIconKind?> IconProperty = AvaloniaProperty.RegisterDirect<MessageDialog, MaterialIconKind?>(nameof(Icon), o => o.Icon, (o, v) => o.Icon = v);

    public MessageDialog()
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
    
    public MaterialIconKind? Icon
    {
        get => m_icon;
        set => SetAndRaise(IconProperty, ref m_icon, value);
    }
}