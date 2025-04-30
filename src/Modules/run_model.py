# src\Modules\run_model.py
from transformers import AutoTokenizer, AutoModelForCausalLM
import os
import sys
import torch
import time

def print_section(title):
    print(f"\n{'=' * 10} {title} {'=' * 10}")

def main():
    if len(sys.argv) < 3:
        print("Error: Missing Arguments", file=sys.stderr)
        print("Usage: python run_model.py <model_dir> <prompt>", file=sys.stderr)
        return 1

    model_dir = sys.argv[1]
    prompt = sys.argv[2]

    try:
        start_total = time.time()

        print_section("Startup")
        print(f"Prompt: {prompt}")
        print(f"Model directory: {model_dir}")
        print(torch.__version__)
        print(torch.version.cuda)
        print(torch.cuda.is_available())
        print(torch.backends.cudnn.version())
        print(f"Device: {'CUDA' if torch.cuda.is_available() else 'CPU'}")

        if not os.path.exists(model_dir):
            print(f"Error: Model Not Found In {model_dir}", file=sys.stderr)
            return 1

        print_section("Loading Tokenizer & Model")
        load_start = time.time()

        tokenizer = AutoTokenizer.from_pretrained(model_dir)
        print("yooo")
        model = AutoModelForCausalLM.from_pretrained(model_dir).to("cuda" if torch.cuda.is_available() else "cpu")

        load_duration = time.time() - load_start
        print(f"Model + Tokenizer loaded in {load_duration:.2f} seconds")

        print_section("Encoding Prompt")
        encode_start = time.time()
        inputs = tokenizer.encode(prompt, return_tensors="pt").to(model.device)
        print(f"Input shape: {inputs.shape}")
        encode_duration = time.time() - encode_start
        print(f"Prompt encoded in {encode_duration:.2f} seconds")

        print_section("Generating Text")
        gen_start = time.time()
        outputs = model.generate(inputs)
        gen_duration = time.time() - gen_start
        print(f"Text generated in {gen_duration:.2f} seconds")

        print_section("Decoding Output")
        decode_start = time.time()
        result = tokenizer.decode(outputs[0])
        decode_duration = time.time() - decode_start
        print(f"Decoded output in {decode_duration:.2f} seconds")

        print_section("Result")
        print(result)

        print_section("Summary")
        total_duration = time.time() - start_total
        print(f"Total inference time: {total_duration:.2f} seconds")

        return 0
    except Exception as e:
        print(f"Error Generating Text: {str(e)}", file=sys.stderr)
        return 1

if __name__ == "__main__":
    sys.exit(main())
