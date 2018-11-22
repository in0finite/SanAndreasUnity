Copy this line to config "public static string GetGameDir { get{ return ReplaceSubstitutions(Get<string>("game_dir")); } }"

Example:

```csharp
//Load all lookup files to binary reader... 
AudioLookupFiles.ReadAllAudioLookupFiles();
//Load all stream files to binary reader... 
AudioStream.LoadAll();
AudioClip stream;
//get first ogg from "BEATS" 
//eg: AudioStream.GetClipGetAudioClip("StreamPakName", "OggIndex");
stream = AudioStream.GetClipGetAudioClip("BEATS",0);
//get first beat_entry from "BEATS" 
//eg: AudioSFX.GetAudioClip("SfxPakName", "BeatIndex") 
beat_entry[] be = AudioStream.GetBeatEntrys("BEATS",0);
//Load all sfx files to binary reader... 
AudioSFX.LoadAll();
//get first sound from first bank of "BEATS" 
//eg: AudioSFX.GetAudioClip("SfxPakName", "BankIndex", "SoundIndex");
AudioSFX.GetAudioClip("AA",0,0);
```