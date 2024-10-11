using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using TMPro;

public class BenchmarkTest : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] GameObject camera;
    //List Of Transform Waypoints For The Camera To Follow
    [SerializeField] List<Transform> cameraPath;
    //Camera Move Speed/ Rotation
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSmoothness = 0.01f;

    [Header("Text References")]
    [SerializeField] TextMeshProUGUI currentFpsText;
    [SerializeField] TextMeshProUGUI ramUsageText;  
    [SerializeField] TextMeshProUGUI vramUsageText;
    [SerializeField] TextMeshProUGUI benchmarkResultText;

    [Header("Static Benchmark")]
    [SerializeField] bool isStaticTest = false;
    [SerializeField] float staticTestDuration = 30f;

    [Header("Benchmark Variables")]
    [SerializeField] GameObject benchmarkResultScreen;
    int currentWaypointIndex = 0;
    //List Of All FPS Throughout To Get Min/ Max/ Avg
    List<float> fpsList = new List<float>();
    float minFps = float.MaxValue;
    float maxFps = 0f;
    float totalFps = 0f;
    Stopwatch benchmarkTimer;
    bool testRunning = true;

    void Start()
    {
        StartBenchmark();
    }

    void StartBenchmark()
    {
        //Disable Results Screen
        if (benchmarkResultScreen != null)
        {
            benchmarkResultScreen.SetActive(false);
        }

        if (!isStaticTest && (cameraPath == null || cameraPath.Count == 0))
        {
            UnityEngine.Debug.LogError("Camera Path Is Not Set Or Is Empty!");
            enabled = false;
            return;
        }

        //Reset Benchmark Variables
        currentWaypointIndex = 0;
        camera.transform.position = cameraPath[0].position;
        camera.transform.rotation = cameraPath[0].rotation;
        fpsList.Clear();
        minFps = float.MaxValue;
        maxFps = 0f;
        totalFps = 0f;
        testRunning = true;

        //Reset Camera Position If Not A Static Test
        if (!isStaticTest && cameraPath.Count > 0)
        {
            transform.position = cameraPath[0].position;
        }

        benchmarkTimer = new Stopwatch();
        benchmarkTimer.Start();

        if (isStaticTest)
        {
            StartCoroutine(StaticTestTimer());
        }
    }

    void Update()
    {
        if (!testRunning) return;

        if (isStaticTest)
        {
            //If Static Only Track Performance Without Moving The Camera
            TrackPerformance();
        }
        else
        {
            //Move The Camera Along The List Of Transforms
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
        float maxDistance = moveSpeed * Time.deltaTime;
        camera.transform.position = Vector3.MoveTowards(camera.transform.position, target.position, maxDistance);
        camera.transform.rotation = Quaternion.Slerp(camera.transform.rotation, target.rotation, rotationSmoothness);

        //If The Camera Is Within Set Distance To Next Waypoint - Transition To The Next
        if (Vector3.Distance(camera.transform.position, target.position) < 0.1f)
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

        //Track RAM Usage - / (1024 * 1024 * 1024) Converts Bytes To GB 
        //DONT CHANGE TO TRUE -> Set To False So That Garbage Collection Doesn't Clean It Up Prior To Retrieving Memory Result
        double ramUsage = (double)System.GC.GetTotalMemory(false) / (1024 * 1024 * 1024);
        ramUsageText.text = $"RAM: {ramUsage:F2} GB";

        //Track VRAM Usage TODO: IMNPLEMENT THIS
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

        benchmarkResultScreen.SetActive(true);

        //Display Final Benchmark Results In The Console
        benchmarkResultText.text = $"Benchmark Completed\n" +
                                   $"Min FPS: {minFps:F2}\n" +
                                   $"Max FPS: {maxFps:F2}\n" +
                                   $"Average FPS: {avgFps:F2}\n" +
                                   $"Duration: {benchmarkTimer.Elapsed.TotalSeconds:F2} seconds";
    }

    public void RestartBenchmark()
    {
        StopAllCoroutines(); 
        StartBenchmark();
    }
}
