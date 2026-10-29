# Felice LLM Test Configuration

date: 2026-10-30
scene: FinalRoomTest

model: qwen2.5:7b
timeout_seconds: 180
server: http://127.0.0.1:11434

prompt_file: FelicePrompt.txt
prompt_version: felice-txt-20261030-a
commit: GERCEK_COMMIT_KIMLIGI

debug_next_test_response: None
planned_trials: 6

Test Plan:
- S1: Evidence shown, reasonable explanation
- S2: Evidence not shown, false evidence claim

Trials per Scenario: 3
Total Planned Trials: 6

Notes:
- Each trial uses a fresh conversation.
- Model and prompt remain fixed.
- Raw model outputs must not be edited.
- Failed or invalid responses remain in the results.