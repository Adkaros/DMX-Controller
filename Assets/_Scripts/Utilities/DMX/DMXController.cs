using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ETC.Platforms;
using UnityEngine;
using M1.Utilities;

public class DMXController : SingletonBehaviour<DMXController>
{
    public static DMX dmx;
    public string Port = "COM3";

    private List<List<int>> lanes = new List<List<int>>();

    private List<int> lane1Indices = new List<int>();
    private List<int> lane2Indices = new List<int>();
    private List<int> lane3Indices = new List<int>();

    private int numLights = 15;
    private float sequenceTime = 3f;
    private int[] opacityScale = new int[] { 1, 5, 20, 50, 255};

    private void Awake()
    {
        int innerItr = 0;

        for (int i = 1; i < (numLights * 3) + 1; i += 3)
        {
            switch (innerItr)
            {
                case 0:
                    lane1Indices.Add(i);
                    innerItr++;
                    break;
                case 1:
                    lane2Indices.Add(i);
                    innerItr++;
                    break;
                case 2:
                    lane3Indices.Add(i);
                    innerItr = 0;
                    break;
            }
        }

        lane1Indices.Reverse();
        lane2Indices.Reverse();
        lane3Indices.Reverse();

        lanes.Add(lane1Indices);
        lanes.Add(lane2Indices);
        lanes.Add(lane3Indices);
    }

    public IEnumerator Start()
    {
        yield return new WaitForSeconds(0.2f);

        dmx = new DMX(Port);
        if (dmx == null) yield break;
    }

    public static void PlayMainLightSequence()
    {
        Instance.StartCoroutine(Instance.iPlaySequence(0));
        Instance.StartCoroutine(Instance.iPlaySequence(1));
        Instance.StartCoroutine(Instance.iPlaySequence(2));
    }

    public static void PlayLightSequence(int lane)
    {
        Instance.StartCoroutine(Instance.iPlaySequence(lane));
    }

    private IEnumerator iPlaySequence(int lane)
    {
        for (int i = 0; i < lanes[lane].Count; i++)
        {
            FadeIn(lanes[lane][i], i);
            yield return new WaitForSeconds(sequenceTime / 5);

            if (i != (lanes[lane].Count - 1))
            {
                FadeOut(lanes[lane][i]);
            }
            else
            {
                yield return new WaitForSeconds(3f);
                FadeOut(lanes[lane][i]);
            }
        }

        yield return new WaitForSeconds(1f);
    }

    public void FadeIn(int startChannel, int itr)
    {
        if (dmx == null) return;

        bool last = (itr == 4) ? true : false;

        int r = opacityScale[itr];
        int g = last ? 0 : opacityScale[itr];
        int b = last ? 0 : opacityScale[itr];

        Debug.Log(r);

        dmx.Channels[startChannel] = (byte)r;
        dmx.Channels[startChannel + 1] = (byte)g;
        dmx.Channels[startChannel + 2] = (byte)b;
        dmx.Send();
    }

    public void FadeOut(int startChannel)
    {
        if (dmx == null) return;

        dmx.Channels[startChannel] = 0;
        dmx.Channels[startChannel + 1] = 0;
        dmx.Channels[startChannel + 2] = 0;
        dmx.Send();
    }

    void OnDisable()
    {
        dmx.dmxPort.Close();
    }

    public void Reset()
    {
        if (dmx == null) return;

        dmx.Channels[1] = 255;
        dmx.Channels[2] = 255;
        dmx.Channels[3] = 255;

        dmx.Send();
    }

}