# Path : src\Modules\main.py
import os
from fastapi import FastAPI
from Modules.api.endpoints import router as api_router
from Modules.core.model_loader import load_all_models, load_assessment_models

app = FastAPI()
app.include_router(api_router)

@app.on_event("startup")
def startup_event():
    if os.getenv("TEST_MODE") == "1":
        load_assessment_models()
    else:
        load_all_models()