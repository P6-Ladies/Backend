# path: src/Modules/api/endpoints.py

import time, logging, torch
from src.Modules.core.model_loader import tokenizer, model

def generate_text_response(request):
    if not request.prompt:
        return {"error": "Prompt cannot be empty."}

    device = "cuda" if torch.cuda.is_available() else "cpu"
    input_ids = tokenizer.encode(request.prompt, return_tensors="pt").to(device)

    with torch.no_grad():
        output_ids = model.generate(
            input_ids,
            max_length=request.max_length,
            do_sample=True,
            top_p=0.9,
            top_k=50
        )

    output_text = tokenizer.decode(output_ids[0], skip_special_tokens=True)
    return {"result": output_text}
