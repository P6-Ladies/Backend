# path: src/Modules/api/endpoints.py

from pydantic import BaseModel
from typing import Optional, List

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
    conversation: str

class Assessment(BaseModel):
    body: str
    conflict_management_strategy: str
    openness: int
    conscientiousness: int
    extroversion: int
    agreeableness: int
    neuroticism: int
