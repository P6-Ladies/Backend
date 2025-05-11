# src\Modules\download_model.py

import os
import sys
from transformers import AutoTokenizer, AutoModelForCausalLM
from Modules.core.config import MODEL_DIR, MODEL_NAME

ESSENTIAL_FILES = ["model-00001-of-00002.safetensors", "config.json"]


def is_model_downloaded(path: str) -> bool:
    return all(os.path.exists(os.path.join(path, f)) for f in ESSENTIAL_FILES)

def download_model(model_name: str, target_dir: str):
    print(f"Downloading model '{model_name}' into '{target_dir}'...")
    os.makedirs(target_dir, exist_ok=True)

    try:
        tokenizer = AutoTokenizer.from_pretrained(model_name)
        model = AutoModelForCausalLM.from_pretrained(model_name)

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