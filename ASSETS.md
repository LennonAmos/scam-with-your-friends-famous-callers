# Asset sources

Portraits are original editable fan art rendered by `tools/RenderPortraits.cs`. Floating heads use a transparent 737 × 734 canvas, thick black contours and muted flat fills, aligned with the installed game’s native expression layers. Native game images and expression textures are not redistributed.

Each voice is an original description-designed synthetic reference sample, generated with [Qwen3-TTS VoiceDesign](https://huggingface.co/Qwen/Qwen3-TTS-12Hz-1.7B-VoiceDesign), revision `5ecdb67327fd37bb2e042aab12ff7391903235d3`. No actor recordings were downloaded or cloned. These are character-style approximations, not official performances. Voice descriptions, original reference text and random seeds are in `voice-design.json`; `tools/GenerateVoices.py` reproduces the generation workflow.

Voice generation is a development task requiring the Qwen model and Python dependencies. Players receive finished WAV samples and use the speech engine already included with the game. No voice-design model weights or Python runtime are bundled.

Rick’s short burp is procedurally synthesized by this pack's own `FamousCallers.dll`, using a hidden native pause marker every third eligible utterance. It is mixed only into silence, with no spoken stage direction. It requires no other mod, actor recording or external audio sample.

Setup uses the MIT-licensed Mono.Cecil 0.11.6 libraries inside its own process to patch two speech methods. The libraries are checksum-verified during builds; their notice is in `Mono.Cecil-LICENSE.txt` and embedded in setup (run `setup.exe --licenses` to read it). No game DLLs are redistributed. The installer backs up the exact previous speech assembly, verifies ownership before changes and restores that backup on removal. Unknown game builds and later modifications are refused.

The optional Ollama mod is independent. Existing v1.0.3/v1.0.4 installations are supported, including migration of the older Rick hooks to avoid duplicate effects. Their files and installation record are preserved. Older Ollama installers require Famous Callers to be removed first when updating or removing Ollama; reinstall Famous Callers afterward. This ordering prevents either installer from overwriting the other's assembly changes.

To build from source, pass `-ManagedDirectory <game Managed folder>` to `Build.ps1`. Generation tools and game files stay outside the published source and releases.
