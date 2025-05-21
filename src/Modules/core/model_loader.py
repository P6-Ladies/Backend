# path : src/Modules/scripts/model_loader.py

import time, logging, torch
from transformers import AutoTokenizer, AutoModelForCausalLM, pipeline
from Modules.core.config import MODEL_DIR, SUMMARIZER_MODEL, PERSONALITY_MODEL, MNLI_MODEL


class ModelStore:
    tokenizer = None
    model = None
    summarizer = None
    personality_clf = None
    zero_shot = None

store = ModelStore()

def load_all_models():
    global store

    logging.basicConfig(level=logging.INFO, format='%(asctime)s %(levelname)s %(message)s')

    logging.info(f"Loading language model from {MODEL_DIR}")
    t0 = time.time()
    store.tokenizer = AutoTokenizer.from_pretrained(MODEL_DIR)
    device = "cuda" if torch.cuda.is_available() else "cpu"
    store.model = AutoModelForCausalLM.from_pretrained(MODEL_DIR, torch_dtype=torch.bfloat16).to(device)
    logging.info(f"Model loaded on {device} in {time.time()-t0:0.1f}s")

<<<<<<< HEAD
    if store.tokenizer is None or store.model is None:
        raise logging.info("Model or tokenizer failed to load!")

    store.summarizer = pipeline("summarization", model=SUMMARIZER_MODEL)
    store.personality_clf = pipeline("text-classification", model=PERSONALITY_MODEL, top_k=None, token = "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")
    store.zero_shot = pipeline("zero-shot-classification", model=MNLI_MODEL)
    return store
=======
    load_assessment_models()
    logging.info("Loading assessment models")

def load_assessment_models():
    global summarizer, personality_clf, zero_shot
    summarizer = pipeline("summarization", model=SUMMARIZER_MODEL)
    personality_clf = pipeline("text-classification", model=PERSONALITY_MODEL, return_all_scores=True)
    zero_shot = pipeline("zero-shot-classification", model=MNLI_MODEL)
>>>>>>> origin/Testing-full-with-python
