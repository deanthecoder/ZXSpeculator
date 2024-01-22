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

using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using CSharp.Utils.Extensions;
using CSharp.Utils.JsonConverters;
using Newtonsoft.Json;

namespace CSharp.Utils.Settings;

/// <summary>
/// Persistent application user settings.
/// </summary>
/// <remarks>
/// Implementers add their required properties, each one calling Get()/Set() as appropriate.
/// Also implement ApplyDefaults() to set any property defaults (that differ from their type's default).
/// Disposing will automatically save settings in a user- and platform-specific location.
/// Setting a property will automatically raise a property change event.
/// </remarks>
public abstract class UserSettingsBase : INotifyPropertyChanged, IDisposable
{
    private readonly FileInfo m_filePath = Assembly.GetEntryAssembly().GetAppSettingsPath().GetFile("settings.json");
    private readonly Dictionary<string, object> m_state = new Dictionary<string, object>();

    public event PropertyChangedEventHandler PropertyChanged;

    abstract protected void ApplyDefaults();

    protected T Get<T>([CallerMemberName] string key = null)
    {
        m_state.TryGetValue(key ?? throw new ArgumentNullException(nameof(key)), out var value);
        if (typeof(T) == typeof(FileInfo) && value is string s)
        {
            value = new FileInfo(s);
            m_state[key] = value;
        }
        
        return (T)value;
    }

    protected void Set(object value, [CallerMemberName] string key = null)
    {
        if (m_state.TryGetValue(key ?? throw new ArgumentNullException(nameof(key)), out var oldValue) && Equals(oldValue, value))
            return;
        m_state[key] = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
    }

    protected UserSettingsBase()
    {
        ApplyDefaults();
        if (m_filePath.Exists)
            JsonConvert.PopulateObject(m_filePath.ReadAllText(), m_state, CreateSerializerSettings());
    }

    public void Dispose() =>
        m_filePath.WriteAllText(JsonConvert.SerializeObject(m_state, Formatting.Indented, CreateSerializerSettings()));

    private static JsonSerializerSettings CreateSerializerSettings() =>
        new JsonSerializerSettings
        {
            Converters = new JsonConverter[]
            {
                new FileInfoConverter()
            }
        };
}