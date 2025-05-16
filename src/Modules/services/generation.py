# path: src/Modules/api/endpoints.py

import time, logging, torch
from src.Modules.core.model_loader import tokenizer, model

def generate_text_response(request):
    if not request.prompt:
        return {"error": "Prompt cannot be empty."}
    
    # ---- build system + agent prompts --------------------------------
    scenario = request.Scenario
    system_content = (
        f"[Scenario: {scenario.Name}]\n"
        f"{scenario.SettingPrompt}\n"
        f"{scenario.ConflictPrompt}\n"
        f"{scenario.AdditionalPrompt or ''}".strip() 
    )

    # Build agent description
    agent = request.Agent
    agent_content = (
        f"[Assistant: {agent.Name}]\n"
        f"{agent.PromptBody}\n"
        #f"Personality: Openness={agent.Openness}, Conscientiousness={agent.Conscientiousness}, "
        #f"Extroversion={agent.Extroversion}, Agreeableness={agent.Agreeableness}, Neuroticism={agent.Neuroticism}"
    )

    # Assemble chat messages
    messages = [
        {"role": "system", "content": system_content},
        {"role": "system", "content": agent_content},
    ]

    # Append previous messages into prompt with correct sender
    for message in request.History:
        logging.info(message.Sender)
        logging.info(message.Message)
        messages.append({"role" : message.Sender, "content" : message.Message})
    
    # The newest prompt
    messages.append({"role": "user",   "content": request.Prompt})

    # Empty message that the agent continues
    messages.append({"role": "assistant", "content": ""})

    # ---- create input tensors with graceful fallback ------------------
        # template really exists → safe to call
    prompt_text = tokenizer.apply_chat_template(
        messages,
        tokenize=False,
        add_generation_prompt=True,
    )

    device = "cuda" if torch.cuda.is_available() else "cpu"
    inputs = tokenizer(prompt_text, return_tensors="pt").to(device)
    input_ids      = inputs["input_ids"]
    attention_mask = inputs["attention_mask"] #Correctly indicates which tokens to be ignored, ones that are added through padding

    with torch.no_grad():
        output_ids = model.generate(
            input_ids=input_ids,
            attention_mask = attention_mask,
            max_length=request.max_length,
            do_sample=True, #Probabilistic instead of deterministic.
            top_p=0.9, #Cumulative probability of tokens to consider. 1 Would be just pick until you reach top_k. Increase for more wacky randomness.
            top_k=50, #Max amount of tokens to consider. Increase if you want more wacky randomness
            eos_token_id=tokenizer.eos_token_id, #To indicate correctly that a sentence is over. Does not matter for llama.
        )

    # number of tokens in input prompt
    input_length = input_ids.shape[-1]

    # Only choose generated tokens that are past the length of the input prompt, meaning only new text is returned
    generated_tokens = output_ids[0][input_length:]

    # decode yay
    Agent_reply = tokenizer.decode(generated_tokens, skip_special_tokens=True)
    return {"result": Agent_reply}
