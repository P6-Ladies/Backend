# src\Modules\download_model.py

import os
import sys
from transformers import AutoTokenizer, AutoModelForCausalLM
from Modules.core.config import MODEL_DIR, MODEL_NAME
import torch

ESSENTIAL_FILES = ["config.json"]
device = "cuda" if torch.cuda.is_available() else "cpu"


def is_model_downloaded(path: str) -> bool:
    return all(os.path.exists(os.path.join(path, f)) for f in ESSENTIAL_FILES)

def download_model(model_name: str, target_dir: str):
    print(f"Downloading model '{model_name}' into '{target_dir}'...")
    os.makedirs(target_dir, exist_ok=True)

    try:
        tokenizer = AutoTokenizer.from_pretrained(model_name,  use_auth_token="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")
        model = AutoModelForCausalLM.from_pretrained(model_name, torch_dtype=torch.bfloat16, use_auth_token="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").to(device)

        tokenizer.save_pretrained(target_dir)
        model.save_pretrained(target_dir)

        print("Model download complete.")
    except Exception as e:
        print(f"Error downloading model: {e}", file=sys.stderr)
        sys.exit(1)

def main():
    if is_model_downloaded(MODEL_DIR):
        print(f"Model already exists in '{MODEL_DIR}'. Skipping download.")
    else:
        download_model(MODEL_NAME, MODEL_DIR)

if __name__ == "__main__":
    main()