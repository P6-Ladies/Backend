from transformers import AutoTokenizer, AutoModelForCausalLM

import sys

model_path = sys.argv[1]
prompt = sys.argv[2]

tokenizer = AutoTokenizer.from_pretrained(model_path)
model = AutoModelForCausalLM.from_pretrained(model_path)

# ✅ Fix: make sure this stays a dictionary
inputs = tokenizer(prompt, return_tensors="pt")

# Generate safely
output = model.generate(**inputs, max_new_tokens=20)

# Decode output
decoded = tokenizer.decode(output[0], skip_special_tokens=True)
print(decoded)
