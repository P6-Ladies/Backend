# src\Modules\download_model.py
import os
import sys

from transformers import AutoTokenizer, AutoModelForCausalLM

MODEL_DIR = "/usr/src/app/docker/dev/local-models/smol_lM_1.7b"
MODEL_NAME = "HuggingFaceTB/SmolLM-1.7B"

def main():
    """
    Checks whether the model directory has already been downloaded.
    If not, download it from the Hugging Face repo.
    """

    # Quick check: if certain critical files exist, assume it's downloaded
    essential_files = ["model-00001-of-00002.safetensors", "config.json"]
    is_model_present = all(
        os.path.exists(os.path.join(MODEL_DIR, fname)) 
        for fname in essential_files
    )

    if is_model_present:
        print(f"Model '{MODEL_NAME}' is already present in {MODEL_DIR}. Skipping download.")
        return

    # Otherwise, tries to download
    print(f"Downloading model '{MODEL_NAME}' into {MODEL_DIR} ...")
    os.makedirs(MODEL_DIR, exist_ok=True)

    try:
        # Download & save model + tokenizer
        tokenizer = AutoTokenizer.from_pretrained(MODEL_NAME)
        model = AutoModelForCausalLM.from_pretrained(MODEL_NAME)

        tokenizer.save_pretrained(MODEL_DIR)
        model.save_pretrained(MODEL_DIR)

        print("Model download complete.")
    except Exception as e:
        print(f"Error downloading model: {e}", file=sys.stderr)
        sys.exit(1)  # non-zero exit code to signal error

if __name__ == "__main__":
    main()