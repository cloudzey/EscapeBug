
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public class AgentTrialRecorder : MonoBehaviour
{
    [Header("Trial Information")]
    [SerializeField] private string startId = "A";
    [SerializeField, Min(1)] private int trial = 1;
    [SerializeField] private string commitHash = "SET_COMMIT";

    [Header("Experiment Timeout")]
    [SerializeField, Min(1f)]
    private float maxDurationSeconds = 90f;

    private readonly Stopwatch stopwatch = new Stopwatch();

    private bool running;
    private bool finished;
    private string outputPath;

    private const string Header =
        "test_id,start_id,trial,success," +
        "duration_s,failure_reason,commit,valid";

    private void Awake()
    {
        outputPath = Path.Combine(
            Application.persistentDataPath,
            "agent-results.csv"
        );

        try
        {
            Directory.CreateDirectory(
                Application.persistentDataPath
            );

            if (!File.Exists(outputPath))
            {
                File.WriteAllText(
                    outputPath,
                    Header + Environment.NewLine,
                    new UTF8Encoding(false)
                );
            }

            UnityEngine.Debug.Log(
                "[AgentTrial] CSV location: " + outputPath
            );
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError(
                "[AgentTrial] File setup failed: " +
                ex.Message
            );
        }
    }

    public void OnAgentCommandAccepted()
    {
        if (running || finished)
        {
            UnityEngine.Debug.LogWarning(
                "[AgentTrial] Duplicate start ignored."
            );
            return;
        }

        stopwatch.Restart();
        running = true;

        UnityEngine.Debug.Log(
            $"[AgentTrial] START {startId}-{trial}"
        );
    }

    public void OnDoorFullyOpened()
    {
        if (!running || finished)
            return;

        Finish(true, "", true);
    }

    public void OnAgentFailed(string reason)
    {
        if (!running || finished)
            return;

        Finish(
            false,
            string.IsNullOrWhiteSpace(reason)
                ? "agent_failed"
                : reason,
            true
        );
    }

    public void MarkInvalid(string reason)
    {
        if (finished)
            return;

        Finish(
            false,
            string.IsNullOrWhiteSpace(reason)
                ? "invalid_setup"
                : reason,
            false
        );
    }

    private void Update()
    {
        if (!running || finished)
            return;

        if (stopwatch.Elapsed.TotalSeconds >=
            maxDurationSeconds)
        {
            Finish(
                false,
                "experiment_timeout",
                true
            );

            UnityEngine.Debug.LogWarning(
                "[AgentTrial] Measurement timed out. " +
                "Reset the scene before another trial."
            );
        }
    }

    private void Finish(
        bool success,
        string reason,
        bool valid
    )
    {
        double? duration = running
            ? stopwatch.Elapsed.TotalSeconds
            : (double?)null;

        stopwatch.Stop();
        running = false;
        finished = true;

        string id =
            startId + "-" + trial + "-" +
            Guid.NewGuid().ToString("N").Substring(0, 8);

        string timeText = duration.HasValue
            ? duration.Value.ToString(
                "F3",
                CultureInfo.InvariantCulture
            )
            : "";

        string line = string.Join(",", new[]
        {
            Escape(id),
            Escape(startId),
            trial.ToString(CultureInfo.InvariantCulture),
            success ? "true" : "false",
            timeText,
            Escape(reason),
            Escape(commitHash),
            valid ? "true" : "false"
        });

        try
        {
            File.AppendAllText(
                outputPath,
                line + Environment.NewLine,
                new UTF8Encoding(false)
            );
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError(
                "[AgentTrial] CSV write failed: " +
                ex.Message
            );
            return;
        }

        UnityEngine.Debug.Log(
            $"[AgentTrial] FINISH | " +
            $"Success={success} | " +
            $"Valid={valid} | " +
            $"Duration={timeText} | Reason={reason}"
        );
    }

    private static string Escape(string value)
    {
        return "\"" +
            (value ?? "").Replace("\"", "\"\"") +
            "\"";
    }
}
