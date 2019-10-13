/// <summary>
/// GTA audio sharp namespace
/// </summary>
namespace GTAAudioSharp
{
    /// <summary>
    /// On close GTA audio file event handler delegate
    /// </summary>
    /// <typeparam name="StreamType">Stream type</typeparam>
    /// <param name="file">GTA audio file</param>
    /// <param name="stream">Audio stream</param>
    internal delegate void OnCloseGTAAudioFileEventHandler(AGTAAudioFile file, CommitableMemoryStream stream);
}
