from fastapi import FastAPI
from pydantic import BaseModel
from typing import Optional
from transformers import AutoTokenizer, AutoModelForCausalLM
import logging, time, torch

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s | %(levelname)s | %(message)s",
    datefmt="%Y-%m-%d %H:%M:%S",
)

app = FastAPI()

MODEL_DIR = "/usr/src/app/docker/dev/local-models/smol_lM_1.7b"


class GenerateRequest(BaseModel):
    prompt: str
    max_length: Optional[int] = 128

@app.on_event("startup")
def load_model_once():
    """
    This method runs automatically when the server starts.
    Load model+tokenizer exactly once and store them globally.
    """
    global tokenizer, model
    logging.info(f"Loading model from {MODEL_DIR}")
    t0 = time.time()
    tokenizer = AutoTokenizer.from_pretrained(MODEL_DIR)
    device = "cuda" if torch.cuda.is_available() else "cpu"
    model = AutoModelForCausalLM.from_pretrained(MODEL_DIR).to(device)
    logging.info(f"Model loaded on {device} in {time.time()-t0:0.1f}s")

@app.post("/generate")
def generate_text(request: GenerateRequest):
    """
    Generate text from a prompt. The request body includes the prompt
    and optionally a max_length or other generation parameters.
    """
    if not request.prompt:
        return {"error": "Prompt cannot be empty."}

    logging.info(f"⇢ prompt ‑‑{request.prompt[:60]}... (len={len(request.prompt)})")
    t0 = time.time()

    device = "cuda" if torch.cuda.is_available() else "cpu"
    # Tokenize
    input_ids = tokenizer.encode(request.prompt, return_tensors="pt").to(device)

    logging.info(f"Tokenized in {time.time()-t0:0.2f}s → shape {tuple(input_ids.shape)}")

    # Generate
    with torch.no_grad():
        output_ids = model.generate(
            input_ids,
            max_length=request.max_length,
            do_sample=True,
            top_p=0.9,
            top_k=50
        )

    logging.info(f"Generation finished in {time.time()-t0:0.2f}s")

    # Decode
    output_text = tokenizer.decode(output_ids[0], skip_special_tokens=True)

    logging.info(f"⇠ result len={len(output_text)} chars | total {time.time()-t0:0.2f}s")
    return {"result": output_text}

@app.post("/PythonServerTest")
def generate_text_test(request: GenerateRequest):
    return {"Throughput"}