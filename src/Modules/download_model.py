from transformers import AutoModelForCausalLM, AutoTokenizer
import sys
import os

def main():
    if len(sys.argv) < 2:
        print("Error: Output Directory Not Specified", file=sys.stderr)
        return 1

    output_dir = sys.argv[1]
    model_name = "HuggingFaceTB/SmolLM-1.7B"

    try:
        print(f"Downloading {model_name} to {output_dir}...")

        #make a directory
        os.makedirs(output_dir, exist_ok=True)

        #Download and save tokenizer and model
        tokenizer = AutoTokenizer.from_pretrained(model_name)
        model = AutoModelForCausalLM.from_pretrained(model_name)

        tokenizer.save_pretrained(output_dir)
        model.save_pretrained(output_dir)

        print("Downloaded Successfully")
        return 0
    except Exception as e:
        print(f"Error Downloading Model: {str(e)}", file=sys.stderr)
        return 1

if __name__ == "__main__":
    sys.exit(main())