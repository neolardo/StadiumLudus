using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;

/// <summary>
/// An FPS counter used for showing the FPS on debug builds.
/// </summary>
public class FPSCounter : MonoBehaviour
{
    #region Properties and Fields

    private TextMeshProUGUI fpsText;
    private const int frameCount = 60;
    private int deltaFrameCount =0;
    private Queue<float> frameLateQueue;

    #endregion

    #region Methods

    void Start()
    {
        fpsText = GetComponent<TextMeshProUGUI>();
        frameLateQueue = new Queue<float>();
        for (int i = 0; i < frameCount; i++)
        {
            frameLateQueue.Enqueue(Time.captureFramerate);
        }
    }


    void Update()
    {
        if (deltaFrameCount == frameCount)
        {
            fpsText.text = $"{Mathf.RoundToInt(frameLateQueue.Sum() / frameCount)} FPS";
            deltaFrameCount = 0;
        }
        frameLateQueue.Dequeue();
        frameLateQueue.Enqueue(1.0f / Time.deltaTime);
        deltaFrameCount += 1;
    }

    #endregion
}
