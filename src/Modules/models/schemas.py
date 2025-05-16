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
    Conversation: str

class Assessment(BaseModel):
    Body: str
    Conflict_management_strategy: str
    Openness: int
    Conscientiousness: int
    Extroversion: int
    Agreeableness: int
    Neuroticism: int
