# ./src/services/run_model.py

from transformers import AutoTokenizer, AutoModelForCausalLM

model_name = "deepseek-ai/deepseek-vl-7b-chat"

tokenizer = AutoTokenizer.from_pretrained(model_name)
model = AutoModelForCausalLM.from_pretrained(model_name)

input_ids = tokenizer.encode("Hey babes, how *you* doin'?", return_tensors="pt")

output = model.generate(input_ids, max_new_tokens=30)

decoded = tokenizer.decode(output[0], skip_special_tokens=True)
print("Generated text:", decoded)