def test_generate_text_valid(client):
    payload = {
        "prompt": "Once upon a time",
        "max_length": 50
    }

    response = client.post("/generate", json=payload)
    assert response.status_code == 200
    result = response.json()
    assert "result" in result
    assert isinstance(result["result"], str)
    assert len(result["result"]) > 10
