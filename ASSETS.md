# Asset sources

Portraits are original editable fan art rendered by `tools/RenderPortraits.cs`. Floating heads use a transparent 737 × 734 canvas, thick black contours and muted flat fills, aligned with the installed game’s native expression layers. Native game images and expression textures are not redistributed.

Each voice is an original description-designed synthetic reference sample, generated with [Qwen3-TTS VoiceDesign](https://huggingface.co/Qwen/Qwen3-TTS-12Hz-1.7B-VoiceDesign), revision `5ecdb67327fd37bb2e042aab12ff7391903235d3`. No actor recordings were downloaded or cloned. These are character-style approximations, not official performances. Voice descriptions, original reference text and random seeds are in `voice-design.json`; `tools/GenerateVoices.py` reproduces the generation workflow.

Voice generation is a development task requiring the Qwen model and Python dependencies. Players receive finished WAV samples and use the speech engine already included with the game. No voice-design model weights or Python runtime are bundled.

Rick’s short burp is procedurally synthesized by the companion Ollama mod, using a hidden native pause marker every third eligible utterance. It is mixed only into silence, with no spoken stage direction. Install Ollama mod v1.0.4+ separately to enable the effect. The caller pack remains usable without that companion; its burp playback effect requires the companion.
