import pytest
from fastapi.testclient import TestClient
from Modules.main import app
from Modules.core.model_loader import load_assessment_models

@pytest.fixture(scope="module", autouse=True)
def preload_assessment_models():
    load_assessment_models()

@pytest.fixture(scope="module")
def client():
    with TestClient(app) as c:
        yield c

def test_assess_conversation(client):
    payload = {
        "conversation": "Hi, how can I help you today? I feel that you’re not listening to me. Let’s try to resolve this."
    }

    response = client.post("/assess", json=payload)
    assert response.status_code == 200
    result = response.json()

    assert "body" in result
    assert "conflict_management_strategy" in result
    assert all(trait in result for trait in [
        "openness", "conscientiousness", "extroversion", "agreeableness", "neuroticism"
    ])
