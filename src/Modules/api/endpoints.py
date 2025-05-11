# path: src/Modules/api/endpoints.py

from fastapi import APIRouter
from Modules.models.schemas import GenerateRequest, AssessRequest, Assessment
from Modules.services.generation import generate_text_response
from Modules.services.assessment import assess_conversation

router = APIRouter()

@router.post("/generate")
def generate_text(request: GenerateRequest):
    return generate_text_response(request)

@router.post("/assess", response_model=Assessment)
def assess(request: AssessRequest):
    return assess_conversation(request)

@router.post("/PythonServerTest")
def generate_text_test(request: GenerateRequest):
    return {"Throughput"}
