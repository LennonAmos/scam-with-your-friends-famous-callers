"""Development-only voice asset generation; not required by players."""
import argparse
import json
from pathlib import Path
import numpy as np
import soundfile as sf
import torch
from qwen_tts import Qwen3TTSModel

def main():
    parser=argparse.ArgumentParser()
    parser.add_argument('--model',required=True,help='Downloaded Qwen3-TTS-12Hz-1.7B-VoiceDesign folder or model ID')
    parser.add_argument('--device',default='cpu')
    parser.add_argument('--threads',type=int,default=6)
    parser.add_argument('--output',type=Path)
    args=parser.parse_args()
    root=Path(__file__).resolve().parents[1]
    destination=args.output or root/'voices'
    destination.mkdir(parents=True,exist_ok=True)
    torch.set_num_threads(args.threads)
    model=Qwen3TTSModel.from_pretrained(args.model,device_map=args.device,dtype=torch.float32 if args.device=='cpu' else torch.bfloat16,attn_implementation='sdpa')
    config=json.loads((root/'voice-design.json').read_text(encoding='utf-8'))
    for voice in config['voices']:
        torch.manual_seed(voice['seed'])
        with torch.inference_mode():
            waves,rate=model.generate_voice_design(text=voice['referenceText'],language='English',instruct=voice['description'],max_new_tokens=260,do_sample=True,temperature=0.7,top_p=0.9)
        samples=np.asarray(waves[0],dtype=np.float32)
        peak=float(np.max(np.abs(samples)))
        if not np.isfinite(samples).all() or peak<0.01:
            raise RuntimeError('Invalid generated voice: '+voice['id'])
        sf.write(str(destination/(voice['id']+'.wav')),samples*(0.85/peak),rate,subtype='PCM_16')
        print('Generated:',voice['id'])
if __name__=='__main__':
    main()
