from fastapi import FastAPI
from Modules.api.endpoints import router as api_router
from Modules.core.model_loader import load_all_models

app = FastAPI()
app.include_router(api_router)

@app.on_event("startup")
def startup_event():
    load_all_models()