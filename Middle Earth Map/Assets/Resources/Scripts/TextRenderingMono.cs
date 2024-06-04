using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextRenderingMono : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            TextRendering.ParseFont(Environment.CurrentDirectory + @"\Assets\Resources\Fonts\JetBrainsMono-Bold.ttf");
    }
}
