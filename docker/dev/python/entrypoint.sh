#!/usr/bin/env sh
set -e

# 1) Download the model if needed
python src/Modules/scripts/download_model.py

# 2) Launch your server
exec uvicorn src.Modules.server:app --host 0.0.0.0 --port 5000