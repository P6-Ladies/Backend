# src\Modules\download_model.py
import os
import sys
import torch

from transformers import AutoTokenizer, AutoModelForCausalLM

MODEL_DIR = "/usr/src/app/docker/dev/local-models/Llama3.2-3B-Instruct"
MODEL_NAME = "meta-llama/Llama-3.2-3B-Instruct"

def main():
    """
    Checks whether the model directory has already been downloaded.
    If not, download it from the Hugging Face repo.
    """

    # Quick check: if certain critical files exist, assume it's downloaded
    essential_files = ["config.json"]
    device = "cuda" if torch.cuda.is_available() else "cpu"
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
        tokenizer = AutoTokenizer.from_pretrained(MODEL_NAME, use_auth_token="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")
        model = AutoModelForCausalLM.from_pretrained(MODEL_NAME, torch_dtype=torch.bfloat16, use_auth_token="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").to(device)

        tokenizer.save_pretrained(MODEL_DIR)
        model.save_pretrained(MODEL_DIR)

        print("Model download complete.")
    except Exception as e:
        print(f"Error downloading model: {e}", file=sys.stderr)
        sys.exit(1)  # non-zero exit code to signal error

if __name__ == "__main__":
    main()