using System.Collections.Generic;
using UnityEngine;
using System.IO;


/*
* Creates and destroys game objects from a set CSV
* In the future it will 
*/
public class LiDAR_Visualization : MonoBehaviour
{

    // Define Public and Private Variables
    public string filePath = "LiDAR/lidar_data.csv";
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


        int testing_count = 0;

        // Loop through ranges
        float current_angle = angle_min;
        List<List<float>> data = new List<List<float>>(); // Col 0 = angle,  Col 1 = Distance
        int count = 10; // STARTER VALUE IS 10
        while (current_angle < angle_max)
        {
            float dist = float.Parse(parts[count]);

            if (dist > range_max || dist < range_min)
            {
                // Do something if doesn't meet range?
                Debug.Log($"Outside ranges: {dist}");
            }
            else
            {
                data[count][0] = current_angle; // Angle (rad)
                data[count][1] = dist; // Distance (m)
            }

            testing_count++;
            count++;

            current_angle += angle_increment;
        }
        Debug.Log($"ENDED! Current angle: {current_angle} $Next Value: {parts[count]}");
    }



    /*
    // Update is called once per frame
    void Update()
    {

    }
    */
}
