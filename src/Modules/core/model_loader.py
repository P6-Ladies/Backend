# path : src/Modules/scripts/model_loader.py

import time, logging, torch
from transformers import AutoTokenizer, AutoModelForCausalLM, pipeline
from Modules.core.config import MODEL_DIR, SUMMARIZER_MODEL, PERSONALITY_MODEL, MNLI_MODEL

class Store:
    tokenizer = None,
    model = None,
    summarizer = None,
    personality_clf = None,
    zero_shot = None

store = Store()

def load_all_models():
    global store

    logging.basicConfig(level=logging.INFO, format='%(asctime)s %(levelname)s %(message)s')

    logging.info(f"Loading language model from {MODEL_DIR}")
    t0 = time.time()
    store.tokenizer = AutoTokenizer.from_pretrained(MODEL_DIR)
    device = "cuda" if torch.cuda.is_available() else "cpu"
    store.model = AutoModelForCausalLM.from_pretrained(MODEL_DIR, torch_dtype=torch.bfloat16).to(device)
    logging.info(f"Model loaded on {device} in {time.time()-t0:0.1f}s")

    load_assessment_models()
    logging.info("Loading assessment models")

def load_assessment_models():
    #store.summarizer = pipeline("summarization", model=SUMMARIZER_MODEL, torch_dtype=torch.bfloat16)
    #store.personality_clf = pipeline("text-classification", model=PERSONALITY_MODEL, return_all_scores=True, torch_dtype=torch.bfloat16, token = "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")
    #store.zero_shot = pipeline("zero-shot-classification", model=MNLI_MODEL, torch_dtype=torch.bfloat16)
    return