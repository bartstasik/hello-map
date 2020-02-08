using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DataContainer : MonoBehaviour
{
    [NonSerialized] public static bool activated;

    [NonSerialized] public static double?
        northRay,
        northwestRay,
        northeastRay,
        eastRay,
        southRay,
        westRay,
        rotation,
        fitness,
        distanceFromPlayer,
        mouseRotation;

    [NonSerialized] public static bool?
        doorClosed,
        seesKey,
        forwardButtonPressed,
        backButtonPressed,
        leftButtonPressed,
        rightButtonPressed,
        keyButtonPressed;

    [NonSerialized] public static int?
        checkpointMet;

    private Text
        _row1,
        _row2,
        _row3,
        _row4,
        _row5,
        _row6,
        _row7,
        _row8,
        _row9,
        _row10,
        _row11,
        _row12,
        _row13,
        _row14,
        _row15,
        _row16,
        _row17,
        _row18;

    private const string Output = "output.csv";

    private void Start()
    {
        _row1 = transform.Find("Text_1").GetComponent<Text>();
        _row2 = transform.Find("Text_2").GetComponent<Text>();
        _row3 = transform.Find("Text_3").GetComponent<Text>();
        _row4 = transform.Find("Text_4").GetComponent<Text>();
        _row5 = transform.Find("Text_5").GetComponent<Text>();
        _row6 = transform.Find("Text_6").GetComponent<Text>();
        _row7 = transform.Find("Text_7").GetComponent<Text>();
        _row8 = transform.Find("Text_8").GetComponent<Text>();
        _row9 = transform.Find("Text_9").GetComponent<Text>();
        _row10 = transform.Find("Text_10").GetComponent<Text>();
        _row11 = transform.Find("Text_11").GetComponent<Text>();
        _row12 = transform.Find("Text_12").GetComponent<Text>();
        _row13 = transform.Find("Text_13").GetComponent<Text>();
        _row14 = transform.Find("Text_14").GetComponent<Text>();
        _row15 = transform.Find("Text_15").GetComponent<Text>();
        _row16 = transform.Find("Text_16").GetComponent<Text>();
        _row17 = transform.Find("Text_17").GetComponent<Text>();
        _row18 = transform.Find("Text_18").GetComponent<Text>();
        File.WriteAllText(Output,
                          "timestamp,rotation,northRay,northwestRay,northeastRay,eastRay,southRay,westRay,fitness,doorClosed,checkpointMet,distanceFromPlayer,seesKey,mouseRotation,forwardButtonPressed,backButtonPressed,leftButtonPressed,rightButtonPressed,keyButtonPressed\n");
    }

    private void FixedUpdate()
    {
        _row1.text = "Rotation : " + rotation;
        _row2.text = "Ray N : " + northRay;
        _row3.text = "Ray NW : " + northwestRay;
        _row4.text = "Ray NE : " + northeastRay;
        _row5.text = "Ray E : " + eastRay;
        _row6.text = "Ray S : " + southRay;
        _row7.text = "Ray W : " + westRay;
        _row8.text = "Fitness : " + fitness;
        _row9.text = "Door Closed : " + doorClosed;
        _row10.text = "Checkpoint Met : " + checkpointMet;
        _row11.text = "Away from Player : " + distanceFromPlayer;
        _row12.text = "Key Seen : " + seesKey;
        _row13.text = "W : " + forwardButtonPressed;
        _row14.text = "A : " + leftButtonPressed;
        _row15.text = "S : " + rightButtonPressed;
        _row16.text = "D : " + backButtonPressed;
        _row17.text = "E : " + keyButtonPressed;
        _row18.text = "Mouse : " + mouseRotation;

        if (activated)
            outputToCsv(Time.time, rotation, northRay,
                        northwestRay, northeastRay, eastRay,
                        southRay, westRay, fitness, doorClosed,
                        checkpointMet, distanceFromPlayer, seesKey,
                        mouseRotation, forwardButtonPressed,
                        backButtonPressed, leftButtonPressed,
                        rightButtonPressed, keyButtonPressed);
    }

    private void outputToCsv(params object[] stuff)
    {
        File.AppendAllText(Output, String.Join(",", stuff));
        File.AppendAllText(Output, "\n");
    }

    private string NullableString(object o)
    {
        return o != null ? o.ToString() : "";
    }
}