# testing\backend.IntegrationTests\python\conftest.py
import os
import pytest
from fastapi.testclient import TestClient
from src.Modules.main import app

os.environ["TEST_MODE"] = "1"

@pytest.fixture(scope="module")
def client():
    with TestClient(app) as c:
        yield c
