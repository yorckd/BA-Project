using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;

public class ListListFloatConverter : JsonConverter<List<List<float>>>
{
    public override List<List<float>> ReadJson(JsonReader reader, Type objectType, List<List<float>> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JToken token = JToken.Load(reader);
        if (token.Type == JTokenType.Integer)
        {
            // Handle the case where an integer is sent instead of a list of lists
            long intValue = token.Value<long>();
            Debug.LogError($"Expected List<List<float>> but received integer: {intValue}");
            // You can decide how to handle this case. Here, returning an empty list
            return new List<List<float>>();
        }
        else if (token.Type == JTokenType.Array)
        {
            // Deserialize as List<List<float>>
            return token.ToObject<List<List<float>>>();
        }
        else
        {
            throw new JsonSerializationException($"Unexpected token type: {token.Type}");
        }
    }

    public override void WriteJson(JsonWriter writer, List<List<float>> value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

public enum Bodyparts
{
    HEAD,
    L_EYE,
    R_EYE,
    L_EAR,
    R_EAR,
    L_SHOULDER,
    R_SHOULDER,
    L_ELBOW,
    R_ELBOW,
    L_WRIST,
    R_WRIST,
    L_HIP,
    R_HIP,
    L_KNEE,
    R_KNEE,
    L_ANKLE,
    R_ANKLE
}

public class Keypoints : IEnumerable<KeyValuePair<Bodyparts, Vector2>>
{
    public Dictionary<Bodyparts, Vector2> keypoints;

    public IEnumerator<KeyValuePair<Bodyparts, Vector2>> GetEnumerator()
    {
        return keypoints.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }



    public Keypoints()
    {
        keypoints = new Dictionary<Bodyparts, Vector2>();

        foreach (Bodyparts bodypart in Enum.GetValues(typeof(Bodyparts)))
        {
            keypoints.Add(bodypart, new Vector2(0,0));
        }
    }

    public void UpdateKeypoints(string message)
    {
        if (message == null)
        {
            return;
        }

        var keypointsList = new List<List<float>>();
        try
        {
            keypointsList = JsonConvert.DeserializeObject<List<List<float>>>(message);
        }
        catch (JsonReaderException ex)
        {
            Debug.LogError($"JSON Deserialization Error: {ex.Message}\nJSON: {message}");
        }

        int i = 0;
        foreach (Bodyparts bodypart in Enum.GetValues(typeof(Bodyparts)))
        {
            if (i < keypointsList.Count)
            {
                keypoints[bodypart] = new Vector2(1 - keypointsList[i][0], 1 - keypointsList[i][1]);
                i++;
            }
            else
            {
                break;
            }
        }
    }

    public float AngleBetweenPoints(Bodyparts pointA, Bodyparts pointAngle, Bodyparts pointB)
    {
        Vector2 vector1 = keypoints[pointA] - keypoints[pointAngle];
        Vector2 vector2 = keypoints[pointB] - keypoints[pointAngle];

        // Calculate the angle using the dot product and magnitude
        float angle = Mathf.Atan2(vector2.y, vector2.x) - Mathf.Atan2(vector1.y, vector1.x);

        // Convert the angle from radians to degrees
        angle = angle * Mathf.Rad2Deg;

        // Ensure the angle is positive
        if (angle < 0)
        {
            angle += 360f;
        }

        // Ensure the angle is below 180 degrees for simplicity
        if (angle > 180)
        {
            angle -= 180f;
        }

        return angle;
    }

    public float DistanceBetweenPoints(Bodyparts pointA, Bodyparts pointB)
    {
        return Vector2.Distance(keypoints[pointA], keypoints[pointB]);
    }
}