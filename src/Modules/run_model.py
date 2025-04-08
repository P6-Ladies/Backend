from transformers import AutoModelForCausalLM, AutoTokenizer
import torch
import sys

model_dir = sys.argv[1]
prompt = sys.argv[2] if len(sys.argv) > 2 else "def print_hello_world():"

device = "cuda" if torch.cuda.is_available() else "cpu"

print(f"Loading model from {model_dir}...")
tokenizer = AutoTokenizer.from_pretrained(model_dir)
model = AutoModelForCausalLM.from_pretrained(model_dir).to(device)

inputs = tokenizer.encode(prompt, return_tensors="pt").to(device)
outputs = model.generate(inputs)
print(tokenizer.decode(outputs[0]))