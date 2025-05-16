# path: src/Modules/api/endpoints.py

import time, logging
from typing import List, Dict
from Modules.core.model_loader import store
from Modules.models.schemas import Assessment

def assess_conversation(request):
    conversation = request.conversation

    summary = store.summarizer(conversation, max_length=150, min_length=40)[0]["summary_text"]

    raw_scores: List[Dict] = store.personality_clf(conversation)
    bm = {d["label"]: d["score"] for d in raw_scores}
    scaled = {k: int(round(v * 9 + 1)) for k, v in bm.items()}

    cms_labels = ["Collaboration","Competition","Avoidance","Accommodation","Compromise"]
    zs = store.zero_shot(conversation, candidate_labels=cms_labels)
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
