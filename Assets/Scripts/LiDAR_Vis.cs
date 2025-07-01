using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


/*
* Creates and destroys game objects from a set CSV
* In the future it will possibly work for rosbag .db3 files
* 
* Requires a CSV file with the following format:
* Row 1: header row
* Row 2+: Full LiDAR scans (1 scan per row)
* 
* Columns 1-9: timestamp,frame_id,angle_min,angle_max,angle_increment,time_increment,scan_time,range_min,range_max
* Columns 10+: range_0,range_1,range_2... range_n
*/
public class LiDAR_Vis : MonoBehaviour
{

    // Define Public and Private Variables
    public string filePath = "Assets/LiDAR/lidar_data.csv";
    public GameObject obstaclePrefab; 
    
    
    public float playbackSpeed = 0.05f; // seconds between lines
    public bool autoPlay = true;
    private bool endOfFile = false; // indicates if the end is reached
    private Coroutine playbackCoroutine;

    private List<GameObject> spawnedObstacles = new List<GameObject>();
    private List<string> csvLines = new List<string>();
    private int currentLineIndex = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Read all lines from the CSV file at once
        csvLines = new List<string>(File.ReadAllLines(filePath));

        // Skip first line which contains headers (in this case)
        currentLineIndex = 1;

        if (autoPlay)
        {
            playbackCoroutine = StartCoroutine(PlayLines());
        }    
    }

    IEnumerator PlayLines()
    {
        while (!endOfFile)
        {
            PopulateObstacles();
            yield return new WaitForSeconds(playbackSpeed);
        }
    }


// Called once each new frame of lidar data
void PopulateObstacles()
    {
        if (currentLineIndex >= csvLines.Count)
        {
            endOfFile = true;
            Debug.Log("End of LiDAR playback reached.");
            return;
        }

        // Remove previous obstacles
        foreach (var obj in spawnedObstacles)
        {
            Destroy(obj);
        }
        spawnedObstacles.Clear();


        //// OLD READ PER LINE
        //// Read CSV
        //var lines = File.ReadAllLines(filePath);
        //Debug.Log($"CSV has been read.");
        //// Handle each line
        //var line = lines[0]; // READ ONLY ONE LINE FOR NOW

        string line = csvLines[currentLineIndex];
        currentLineIndex++; 


        // Handle a single line
        var parts = line.Split(',');
        Debug.Log($"Line has {parts.Length} parts.");

        float timestamp = float.Parse(parts[0]);
        string frame_id = parts[1];

        float angle_min = float.Parse(parts[2]); // start angle of the scan [rad]
        float angle_max = float.Parse(parts[3]); // end angle of the scan [rad]
        float angle_increment = float.Parse(parts[4]); // angular distance between measurements [rad]
        float time_increment = float.Parse(parts[5]); // time between measurements [seconds] - if your scanner
                                                      // is moving, this will be used in interpolating position
                                                      // of 3d points
        float scan_time = float.Parse(parts[6]); // time between scans [seconds]
        float range_min = float.Parse(parts[7]); // minimum range value [m]
        float range_max = float.Parse(parts[8]); // maximum range value [m]

        Debug.Log($"Timestamp (ns): {timestamp}, Frame: {frame_id}, " +
          $"angle_min: {angle_min}, angle_max: {angle_max}, angle_increment: {angle_increment}, " +
          $"time_increment: {time_increment}, scan_time: {scan_time}, " +
          $"range_min: {range_min}, range_max: {range_max} ");


        // Loop through ranges
        float current_angle = angle_min;
        var data = new List<(float angle, float distance)>(); // Tuple: (angle, distance)
        int count = 9; // STARTER VALUE IS 9


        while (current_angle < angle_max)
        {
            // Attempt to parse the distance value
            bool parsed = float.TryParse(parts[count], out float dist);
            bool isValidDist = parsed && !float.IsInfinity(dist) && dist >= range_min && dist <= range_max;

            if (isValidDist)
            {
                data.Add((current_angle, dist)); // Only add valid points on the scan
                //Debug.Log($"Count: {count}, Angle: {current_angle}, Distance: {parts[count]}");
            }

            count++;
            current_angle += angle_increment;
        }
        Debug.Log($"ENDED!");


        // Create obstacles from data
        foreach (var (angle, dist) in data)
        {
            // Calculate position 
            // TODO  make sure angle is correct (sin vs cos)
            float x = (dist * Mathf.Cos(angle));
            float z = (dist * Mathf.Sin(angle));
            Vector3 position = new Vector3(x, (float)0.5, z);

            // Create the obstacle GameObject  
            GameObject obstacle = Instantiate(obstaclePrefab, position, Quaternion.identity);
            spawnedObstacles.Add(obstacle);
        }
        
          
    }



    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Populate obstacles when space is pressed
            PopulateObstacles();
        }
    }
    
}
