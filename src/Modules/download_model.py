from transformers import AutoModelForCausalLM, AutoTokenizer
import sys

model_dir = sys.argv[1] if len(sys.argv) > 1 else "./models/smol_lm_1.7b"
checkpoint = "HuggingFaceTB/SmolLM-1.7B"

print(f"Downloading model to {model_dir}...")
tokenizer = AutoTokenizer.from_pretrained(checkpoint)
model = AutoModelForCausalLM.from_pretrained(checkpoint)

tokenizer.save_pretrained(model_dir)
model.save_pretrained(model_dir)
print("Model downloaded successfully!")