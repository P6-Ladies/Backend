# path : src/Modules/scripts/model_loader.py

import time, logging, torch
from transformers import AutoTokenizer, AutoModelForCausalLM, pipeline
from Modules.core.config import MODEL_DIR, SUMMARIZER_MODEL, PERSONALITY_MODEL, MNLI_MODEL

tokenizer = None
model = None
summarizer = None
personality_clf = None
zero_shot = None

def load_all_models():
    global tokenizer, model, summarizer, personality_clf, zero_shot

    logging.info(f"Loading language model from {MODEL_DIR}")
    t0 = time.time()
    tokenizer = AutoTokenizer.from_pretrained(MODEL_DIR)
    device = "cuda" if torch.cuda.is_available() else "cpu"
    model = AutoModelForCausalLM.from_pretrained(MODEL_DIR).to(device)
    logging.info(f"Model loaded on {device} in {time.time()-t0:0.1f}s")

    summarizer = pipeline("summarization", model=SUMMARIZER_MODEL)
    personality_clf = pipeline("text-classification", model=PERSONALITY_MODEL, return_all_scores=True)
    zero_shot = pipeline("zero-shot-classification", model=MNLI_MODEL)
