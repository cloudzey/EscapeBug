
using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public class LlmTrialRecorder : MonoBehaviour
{
    [Header("Trial")]
    [SerializeField] private string testId = "S1";
    [SerializeField, Min(1)] private int trial = 1;

    [Header("Version")]
    [SerializeField] private string modelName = "qwen2.5:7b";
    [SerializeField] private string promptVersion = "";
    [SerializeField] private string commitHash = "";

    private readonly Stopwatch timer = new Stopwatch();

    private string directory;
    private string csvPath;
    private bool running;
    private bool evidenceAtStart;

    private const string CsvHeader =
        "test_id,trial,model,prompt_version," +
        "evidence_shown,latency_ms,http_status," +
        "json_valid,proposed_action,applied_action," +
        "rejection_reason,raw_file,commit";

    private void Awake()
    {
        directory = Path.Combine(
            Application.persistentDataPath,
            "llm-measurements"
        );

        csvPath = Path.Combine(
            directory,
            "llm-results.csv"
        );

        Directory.CreateDirectory(directory);
        Directory.CreateDirectory(
            Path.Combine(directory, "raw")
        );

        if (!File.Exists(csvPath))
        {
            File.WriteAllText(
                csvPath,
                CsvHeader + Environment.NewLine,
                new UTF8Encoding(false)
            );
        }

        Debug.Log("[LLMTrial] CSV: " + csvPath);
    }

    public void BeginTrial(bool actualEvidenceShown)
    {
        if (running)
        {
            Debug.LogWarning(
                "[LLMTrial] A request is already running."
            );
            return;
        }

        evidenceAtStart = actualEvidenceShown;
        running = true;
        timer.Restart();

        Debug.Log(
            $"[LLMTrial] START {testId}-{trial}"
        );
    }

    public void FinishTrial(
        bool jsonValid,
        string proposedAction,
        string appliedAction,
        string rejectionReason,
        string rawContent,
        long httpStatus = -1,
        long latencyMilliseconds = -1)
    {
        if (!running)
        {
            Debug.LogWarning(
                "[LLMTrial] No active trial."
            );
            return;
        }

        timer.Stop();
        running = false;
        long recordedLatency = latencyMilliseconds >= 0
            ? latencyMilliseconds
            : timer.ElapsedMilliseconds;

        string uniqueId = Guid.NewGuid()
            .ToString("N")
            .Substring(0, 8);

        string baseName =
            testId + "-" +
            trial.ToString("00") + "-" +
            uniqueId;

        string relativeRaw =
            "raw/" + baseName + ".txt";

        string fullRaw = Path.Combine(
            directory,
            "raw",
            baseName + ".txt"
        );

        try
        {
            File.WriteAllText(
                fullRaw,
                rawContent ?? "",
                new UTF8Encoding(false)
            );

            string[] fields =
            {
                testId,
                trial.ToString(),
                modelName,
                promptVersion,
                evidenceAtStart ? "true" : "false",
                recordedLatency.ToString(),
                httpStatus >= 0
                    ? httpStatus.ToString()
                    : "unknown",
                jsonValid ? "true" : "false",
                proposedAction,
                appliedAction,
                rejectionReason,
                relativeRaw,
                commitHash
            };

            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = CsvEscape(fields[i]);
            }

            File.AppendAllText(
                csvPath,
                string.Join(",", fields) +
                Environment.NewLine,
                new UTF8Encoding(false)
            );

            Debug.Log(
                $"[LLMTrial] FINISH {testId}-{trial} " +
                $"Latency={recordedLatency}ms " +
                $"JSON={jsonValid} " +
                $"Proposed={proposedAction} " +
                $"Applied={appliedAction}"
            );
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "[LLMTrial] Write failed: " + ex.Message
            );
        }
    }

    public void CancelTrial(string reason)
    {
        if (!running)
            return;

        timer.Stop();
        running = false;

        Debug.LogWarning(
            "[LLMTrial] CANCELLED: " + reason +
            " | This request is not a valid live trial."
        );
    }

    private static string CsvEscape(string value)
    {
        return "\"" +
            (value ?? "").Replace("\"", "\"\"") +
            "\"";
    }
}
