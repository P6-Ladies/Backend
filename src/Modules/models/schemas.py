# path: src/Modules/api/endpoints.py

from pydantic import BaseModel
from typing import Optional

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
