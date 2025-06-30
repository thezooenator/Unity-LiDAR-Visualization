using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;


/*
* Creates and destroys game objects from a set CSV
* In the future it will 
*/
public class LiDAR_Vis : MonoBehaviour
{

    // Define Public and Private Variables
    public string filePath = "Assets/LiDAR/lidar_data.csv";
    public GameObject obstaclePrefab;
    private List<GameObject> spawnedObstacles = new List<GameObject>();



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PopulateObstacles();
    }

    // Called once each new frame of lidar data
    void PopulateObstacles()
    {
        // Remove previous obstacles
        foreach (var obj in spawnedObstacles)
        {
            Destroy(obj);
        }
        spawnedObstacles.Clear();

        // Read CSV
        var lines = File.ReadAllLines(filePath);
        Debug.Log($"CSV has been read.");

        // Handle each line
        var line = lines[0]; // READ ONLY ONE LINE FOR NOW


        // Handle a single line
        var parts = line.Split(',');
        Debug.Log($"Line has {parts.Length} parts.");

        int header = int.Parse(parts[0]);
        long timestampNs = long.Parse(parts[1]);
        string frame_id = parts[2];

        float angle_min = float.Parse(parts[3]); // start angle of the scan [rad]
        float angle_max = float.Parse(parts[4]); // end angle of the scan [rad]
        float angle_increment = float.Parse(parts[5]); // angular distance between measurements [rad]
        float time_increment = float.Parse(parts[6]); // time between measurements [seconds] - if your scanner
                                                      // is moving, this will be used in interpolating position
                                                      // of 3d points
        float scan_time = float.Parse(parts[7]); // time between scans [seconds]
        float range_min = float.Parse(parts[8]); // minimum range value [m]
        float range_max = float.Parse(parts[9]); // maximum range value [m]

        Debug.Log($"Header: {header}, Timestamp (ns): {timestampNs}, Frame: {frame_id}, " +
          $"angle_min: {angle_min}, angle_max: {angle_max}, angle_increment: {angle_increment}, " +
          $"time_increment: {time_increment}, scan_time: {scan_time}, " +
          $"range_min: {range_min}, range_max: {range_max} ");


        // Loop through ranges
        float current_angle = angle_min;
        var data = new List<(float angle, float distance)>(); // Tuple: (angle, distance)
        int count = 10; // STARTER VALUE IS 10
        
        // Temporarily using 138 as limit on DATA
        while (current_angle < angle_max && count < parts.Length && count < 138)
        {
            // Attempt to parse the distance value
            bool parsed = float.TryParse(parts[count], out float dist);
            bool isValidDist = parsed && !float.IsInfinity(dist) && dist >= range_min && dist <= range_max;

            if (isValidDist)
            {
                data.Add((current_angle, dist)); // Only add valid points on the scan
            }

            count++;
            current_angle += angle_increment;
        }
        Debug.Log($"ENDED! Current angle: {current_angle} $Next Value: {parts[count]}");


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



    /*
    // Update is called once per frame
    void Update()
    {

    }
    */
}
