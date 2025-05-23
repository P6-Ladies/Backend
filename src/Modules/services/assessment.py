# path: src/Modules/api/endpoints.py

import time, logging
from typing import List, Dict
from Modules.core.model_loader import store
from Modules.models.schemas import Assessment, AssessRequest

def assess_conversation(request: AssessRequest):
    conversation = request.conversation
    short_conversation = truncate_conversation(conversation, store.zero_shot.tokenizer, 480)
    logging.info(short_conversation)

    summary = store.summarizer(short_conversation, max_length=150, min_length=40)[0]["summary_text"]

    raw_scores: List[Dict] = store.personality_clf(short_conversation)
    logging.info(f"Personality scores raw: {raw_scores}")
    bm = {d["label"]: d["score"] for d in raw_scores[0]}
    logging.info(f"Extracted labels: {list(bm.keys())}")
    scaled = {k: int(round(v * 9 + 1)) for k, v in bm.items()}

    cms_labels = ["Collaboration","Competition","Avoidance","Accommodation","Compromise"]
    zs = store.zero_shot(short_conversation, candidate_labels=cms_labels)
    logging.info(f"Zero-shot result: {zs}")
    cms = zs["labels"][0]

    return Assessment(
        body=summary,
        conflict_management_strategy=cms,
        openness=scaled.get("Openness", 5),
        conscientiousness=scaled.get("Conscientiousness", 5),
        extroversion=scaled.get("Extraversion", 5),
        agreeableness=scaled.get("Agreeableness", 5),
        neuroticism=scaled.get("Neuroticism", 5),
    )

#Removes characters from start so that it fits with 512 tokens.
def truncate_conversation(conversation: str, tokenizer, max_tokens: int = 480) -> str:
    encoded = tokenizer.encode(conversation, add_special_tokens=False)
    if len(encoded) > max_tokens:
        encoded = encoded[-max_tokens:]
    truncated = tokenizer.decode(encoded, skip_special_tokens=True)
    return truncated