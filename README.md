# VizualizrStudio
A WIP FOSS Dj Program made with .NET MAUI and 💜

Major backlog items include:
 - [ ] MIDI support + mapping customization
 - [ ] Audio mixing, better streams, effects and the like
 - [ ] Track analysis for stats such as BPM and Key.
 - [ ] State retention - to recover in case of crash
 - [ ] Mac support (though i'm currently using NAudio).

![image](https://github.com/user-attachments/assets/b71664fc-9cc5-47e4-b4d5-329eac30054d)


---

![d3acde85-c93a-4403-bbf4-78e8395c37ee](https://github.com/user-attachments/assets/a39d6372-fc90-43a0-ae22-28b702bae367)


--- 
# Libraries in use
NAudio       -  loading audio samples and determining sample rates - Will be removed as it's windows only.

PortAudio    -  Audio renderer.

SyncFusion   -  Controls


# Building
Basic .NET MAUI build, but currently i'm experimenting with SyncFusion for better controls - which will need a community license locally to build.

