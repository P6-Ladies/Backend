from fastapi import FastAPI
from pydantic import BaseModel
from typing import Optional, List, Dict
from transformers import pipeline, AutoTokenizer, AutoModelForCausalLM
import logging, time, torch

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s | %(levelname)s | %(message)s",
    datefmt="%Y-%m-%d %H:%M:%S",
)

app = FastAPI()

# Language model(s)
MODEL_DIR = "/usr/src/app/docker/dev/local-models/smol_lM_1.7b"

# Assessment models
SUMMARIZER_MODEL = "facebook/bart-large-cnn"
PERSONALITY_MODEL = "Nasserelsaman/microsoft-finetuned-personality"
MNLI_MODEL = "facebook/bart-large-mnli"

class GenerateRequest(BaseModel):
    prompt: str
    max_length: Optional[int] = 128

class AssessRequest(BaseModel):
    conversation: str

class Assessment(BaseModel):
    body: str
    conflict_management_strategy: str
    openness: int
    conscientiousness: int
    extroversion: int
    agreeableness: int
    neuroticism: int

@app.post("/PythonServerTest")
def generate_text_test(request: GenerateRequest):
    return {"Throughput"}


@app.on_event("startup")
def load_language_model():
    """
    This method runs automatically when the server starts.
    Load model+tokenizer exactly once and store them globally.
    """
    global tokenizer, model
    logging.info(f"Loading language model from {MODEL_DIR}")
    t0 = time.time()
    tokenizer = AutoTokenizer.from_pretrained(MODEL_DIR)
    device = "cuda" if torch.cuda.is_available() else "cpu"
    model = AutoModelForCausalLM.from_pretrained(MODEL_DIR).to(device)
    logging.info(f"Model loaded on {device} in {time.time()-t0:0.1f}s")

@app.on_event("startup")
def load_assessment_models():
    global summarizer, personality_clf, zero_shot

    logging.info(f"Loading summarization model: {SUMMARIZER_MODEL}")
    summarizer = pipeline("summarization", model=SUMMARIZER_MODEL)

    logging.info(f"Loading personality prediction model: {PERSONALITY_MODEL}")
    personality_clf = pipeline(
        "text-classification",
        model=PERSONALITY_MODEL,
        return_all_scores=True
    )

    logging.info(f"Loading zero-shot classification model: {MNLI_MODEL}")
    zero_shot = pipeline(
        "zero-shot-classification",
        model=MNLI_MODEL
    )



@app.post("/generate")
def generate_text(request: GenerateRequest):
    """
    Generate text from a prompt. The request body includes the prompt
    and optionally a max_length or other generation parameters.
    """
    if not request.prompt:
        return {"error": "Prompt cannot be empty."}

    logging.info(f"⇢ prompt --{request.prompt[:60]}... (len={len(request.prompt)})")
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

@app.post("/assess", response_model=Assessment)
def assess(request: AssessRequest):
    conversation = request.conversation

    # 1) Summarize
    sum_t0 = time.time()
    summary = summarizer(conversation, max_length=150, min_length=40)[0]["summary_text"]
    logging.info(f"Summarized in {(time.time()-sum_t0):0.2f}s")

    # 2) Big-Five scores
    per_t0 = time.time()
    raw_scores: List[Dict] = personality_clf(conversation)
    bm = {d["label"]: d["score"] for d in raw_scores}
    # scale 0–1 to 1–10
    scaled = {k: int(round(v * 9 + 1)) for k, v in bm.items()}
    logging.info(f"Personality done in {(time.time()-per_t0):0.2f}s")

    # 3) Conflict strategy
    zs_t0 = time.time()
    cms_labels = ["Collaboration","Competition","Avoidance","Accommodation","Compromise"]
    zs = zero_shot(conversation, candidate_labels=cms_labels)
    cms = zs["labels"][0]
    logging.info(f"Zero-shot done in {(time.time()-zs_t0):0.2f}s")

    return Assessment(
        body=summary,
        conflict_management_strategy=cms,
        openness=scaled.get("Openness", 5),
        conscientiousness=scaled.get("Conscientiousness", 5),
        extroversion=scaled.get("Extraversion", 5),
        agreeableness=scaled.get("Agreeableness", 5),
        neuroticism=scaled.get("Neuroticism", 5),
    )