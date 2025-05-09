from fastapi import FastAPI # type: ignore
from pydantic import BaseModel # type: ignore
from typing import Optional, List, Dict
from transformers import pipeline, AutoTokenizer, AutoModelForCausalLM
import logging, time, torch, re # type: ignore


# ────────────────────────── logging ──────────────────────────
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s | %(levelname)s | %(message)s",
    datefmt="%Y-%m-%d %H:%M:%S",
)


# ────────────────────────── app / constants ──────────────────────────
app = FastAPI()

# Language model(s)
MODEL_DIR = "/usr/src/app/docker/dev/local-models/Llama3.2-3B-Instruct"
# Assessment models
SUMMARIZER_MODEL = "facebook/bart-large-cnn"
PERSONALITY_MODEL = "Nasserelsaman/microsoft-finetuned-personality"
MNLI_MODEL = "facebook/bart-large-mnli"

# ────────────────────────── DTOs ──────────────────────────
class AgentPayload(BaseModel):
    Name: str
    PromptBody: str
    Openness: int
    Conscientiousness: int
    Extroversion: int
    Agreeableness: int
    Neuroticism: int

class ScenarioPayload(BaseModel):
    Name: str
    SettingPrompt: str
    ConflictPrompt: str
    AdditionalPrompt: Optional[str] = ""

class TemplateMessage(BaseModel):
    Sender: str
    Message: str

class GenerateRequest(BaseModel):
    Agent: AgentPayload
    Scenario: ScenarioPayload
    Prompt: str
    History: List[TemplateMessage]
    MaxLength: Optional[int] = 8192

class AssessRequest(BaseModel):
    Conversation: str

class Assessment(BaseModel):
    Body: str
    ConflictManagementStrategy: str
    Openness: int
    Conscientiousness: int
    Extroversion: int
    Agreeableness: int
    Neuroticism: int

# ────────────────────────── startup: load models ──────────────────────────
@app.on_event("startup")
def load_language_model():
    global tokenizer, model, device
    logging.info(f"Loading language model from {MODEL_DIR}")
    t0 = time.time()
    tokenizer = AutoTokenizer.from_pretrained(MODEL_DIR, trust_remote_code=True,torch_dtype=torch.bfloat16)
    device = "cuda" if torch.cuda.is_available() else "cpu"
    model = AutoModelForCausalLM.from_pretrained(MODEL_DIR, trust_remote_code=True, torch_dtype=torch.bfloat16).to(device)
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
        return_all_scores=True,
        token = "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"
    )

    logging.info(f"Loading zero-shot classification model: {MNLI_MODEL}")
    zero_shot = pipeline(
        "zero-shot-classification",
        model=MNLI_MODEL
    )


@app.middleware("http")
async def log_request_response(request, call_next):
    if request.url.path == "/generate":
        body = await request.body()
        logging.info(f">>> /generate in  | {body.decode()[:500]}")

    response = await call_next(request)

    if request.url.path == "/generate":
        # FastAPI streams resp.body; need to read & replace
        content = b"".join([chunk async for chunk in response.body_iterator])
        logging.info(f"<<< /generate out | {content.decode()[:500]}")

        # Define an async generator to stream the content back properly
        async def new_body_iterator():
            yield content

        response.body_iterator = new_body_iterator()  # put it back

    return response

# ────────────────────────── text generation ──────────────────────────

@app.post("/generate")
def generate_text(request: GenerateRequest):
    """Generate agent reply to the user prompt."""
    if not request.Prompt:
        return {"error": "Prompt cannot be empty."}
        
    # ---- build system + agent prompts --------------------------------
    scenario = request.Scenario
    system_content = (
        f"[Scenario: {scenario.Name}]\n"
        f"{scenario.SettingPrompt}\n"
        f"{scenario.ConflictPrompt}\n"
        f"{scenario.AdditionalPrompt or ''}".strip() 
    )

    # Build agent description
    agent = request.Agent
    agent_content = (
        f"[Assistant: {agent.Name}]\n"
        f"{agent.PromptBody}\n"
        #f"Personality: Openness={agent.Openness}, Conscientiousness={agent.Conscientiousness}, "
        #f"Extroversion={agent.Extroversion}, Agreeableness={agent.Agreeableness}, Neuroticism={agent.Neuroticism}"
    )

    # Assemble chat messages
    messages = [
        {"role": "system", "content": system_content},
        {"role": "system", "content": agent_content},
    ]

    # Append previous messages into prompt with correct sender
    for message in request.History:
        logging.info(message.Sender)
        logging.info(message.Message)
        messages.append({"role" : message.Sender, "content" : message.Message})
    
    # The newest prompt
    messages.append({"role": "user",   "content": request.Prompt})

    # Empty message that the agent continues
    messages.append({"role": "assistant", "content": ""})

    # ---- create input tensors with graceful fallback ------------------
        # template really exists → safe to call
    prompt_text = tokenizer.apply_chat_template(
        messages,
        tokenize=False,
        add_generation_prompt=True,
    )

    logging.info(f"⇢ prompt --{prompt_text}... (len={len(request.Prompt)})")

    inputs = tokenizer(prompt_text, return_tensors="pt").to(device)
    input_ids      = inputs["input_ids"]
    attention_mask = inputs["attention_mask"]

    # ---- generation ---------------------------------------------------
    t0 = time.time()
    with torch.no_grad():
        output_ids = model.generate(
            input_ids=input_ids,
            attention_mask=attention_mask,
            max_length=request.MaxLength,
            do_sample=True,
            top_p=0.9,
            top_k=50,
            eos_token_id=tokenizer.eos_token_id,
        )
    logging.info(f"Tokenized in {time.time()-t0:0.2f}s → shape {tuple(input_ids.shape)}")

    # ---- extract Agent reply -------------------------------------
    # number of tokens in input prompt
    input_length = input_ids.shape[-1]

    # get generated tokens past the prompt
    generated_tokens = output_ids[0][input_length:]

    # decode them
    Agent_reply = tokenizer.decode(generated_tokens, skip_special_tokens=True)


    # if getattr(tokenizer, "chat_template", None):
        # template path
        # Agent_reply = tokenizer.chat_template.get_response(raw_text).strip()
        # print(tokenizer.decode(Agent_reply[0]))
    # else:
        # fallback – look for the last "Agent:" marker, otherwise use full text
        # if "Agent:" in raw_text:
        #    Agent_reply = raw_text.split("Agent:", 1)[-1].strip()
        # else:
        #    Agent_reply = raw_text.strip()

    logging.info(
        "assistant_reply len=%d – first 120 chars: %s",
        len(Agent_reply),
        Agent_reply[:120].replace("\n", "\\n")
    )
    logging.info(f"⇠ result len={len(Agent_reply)} chars | total {time.time()-t0:0.2f}s")
    return {
        "result": Agent_reply,
        # "raw_len": len(raw_text),          # help .NET side verify
        "reply_len": len(Agent_reply)
    }

    

# ────────────────────────── assessment endpoint ──────────────────────────
@app.post("/assess", response_model=Assessment)
def assess(request: AssessRequest):
    conversation = request.Conversation

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
        Body=summary,
        ConflictManagementStrategy=cms,
        Openness=scaled.get("Openness", 5),
        Conscientiousness=scaled.get("Conscientiousness", 5),
        Extroversion=scaled.get("Extraversion", 5),
        Agreeableness=scaled.get("Agreeableness", 5),
        Neuroticism=scaled.get("Neuroticism", 5),
    )