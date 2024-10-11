using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using TMPro;

public class BenchmarkTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] List<Transform> cameraPath; // List of waypoints for the camera to follow
    [SerializeField] float moveSpeed = 5f;       // Speed at which the camera moves
    [SerializeField] TextMeshProUGUI currentFpsText;        // UI Text element for displaying the current FPS
    [SerializeField] TextMeshProUGUI ramUsageText;          // UI Text element for displaying the RAM usage
    [SerializeField] TextMeshProUGUI vramUsageText;         // UI Text element for displaying the VRAM usage
    [SerializeField] TextMeshProUGUI benchmarkResultText;   // UI Text element to display results after the test

    [Header("Static Benchmark")]
    [SerializeField] bool isStaticTest = false;  // If true, perform a static test (no camera movement)
    [SerializeField] float staticTestDuration = 30f; // Duration of the static test in seconds

    [Header("Benchmark Variables")]
    int currentWaypointIndex = 0; // Index of the current transform the camera is moving to
    List<float> fpsList = new List<float>(); // List to track FPS throughout the test
    float minFps = float.MaxValue;
    float maxFps = 0f;
    float totalFps = 0f;
    Stopwatch benchmarkTimer;
    bool testRunning = true;

    void Start()
    {
        if (!isStaticTest && (cameraPath == null || cameraPath.Count == 0))
        {
            UnityEngine.Debug.LogError("Camera path is not set or empty for test!");
            enabled = false;
            return;
        }

        benchmarkTimer = new Stopwatch();
        benchmarkTimer.Start();

        if (isStaticTest)
        {
            // For a static test, start a coroutine to end after the specified duration
            StartCoroutine(StaticTestTimer());
        }
    }

    void Update()
    {
        if (!testRunning) return; // If the test has ended, stop updating

        if (isStaticTest)
        {
            // For static test, only track performance without moving the camera
            TrackPerformance();
        }
        else
        {
            // Move the camera along the list of transforms
            if (currentWaypointIndex < cameraPath.Count)
            {
                MoveCamera();
                TrackPerformance();
            }
            else
            {
                EndBenchmark();
            }
        }
    }

    void MoveCamera()
    {
        Transform target = cameraPath[currentWaypointIndex];
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            currentWaypointIndex++;
        }
    }

    void TrackPerformance()
    {
        //Track FPS
        float currentFps = 1.0f / Time.deltaTime;
        fpsList.Add(currentFps);
        totalFps += currentFps;
        minFps = Mathf.Min(minFps, currentFps);
        maxFps = Mathf.Max(maxFps, currentFps);
        currentFpsText.text = $"Current FPS: {currentFps:F2}";

        //Track RAM Usage
        long ramUsage = System.GC.GetTotalMemory(false) / (1024 * 1024); // Convert bytes to MB
        ramUsageText.text = $"RAM: {ramUsage} MB";

        // Track VRAM usage (placeholder for now)
        vramUsageText.text = "VRAM: 512 MB (Placeholder)";
    }

    IEnumerator StaticTestTimer()
    {
        //Wait Until The Static Test Timer Ends Then End Benchmark
        yield return new WaitForSeconds(staticTestDuration);
        EndBenchmark();
    }

    void EndBenchmark()
    {
        benchmarkTimer.Stop();
        testRunning = false;

        float avgFps = totalFps / fpsList.Count;

        //Display Final Benchmark Results In The Console
        benchmarkResultText.text = $"Benchmark Completed\n" +
                                   $"Min FPS: {minFps:F2}\n" +
                                   $"Max FPS: {maxFps:F2}\n" +
                                   $"Average FPS: {avgFps:F2}\n" +
                                   $"Duration: {benchmarkTimer.Elapsed.TotalSeconds:F2} seconds";

        enabled = false;
    }
}
