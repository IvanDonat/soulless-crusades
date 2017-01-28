using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SingleEvent
{
    public float timeStamp;
    public string text;

    public SingleEvent(float gameTime, string eventText)
    {
        timeStamp = gameTime;
        text = eventText;
    }
}
    
public class Events : MonoBehaviour 
{
    private static List<SingleEvent> eventList = new List<SingleEvent>();

    public Text textEvents;
    public Text textTimestamps;

    void Update()
    {
        if (eventList.Count == 0)
        {
            textEvents.text = "";
            textTimestamps.text = "";
            return;
        }
        
        string events = "", stamps = "";
        for (int i = Mathf.Max(eventList.Count - 5, 0); i < eventList.Count; i++)
        {
            events += eventList[i].text + '\n';

            float gameTime = eventList[i].timeStamp;
            int minutes = (int)gameTime / 60;
            int seconds = (int)gameTime % 60;
            stamps += "<color=orange>[" + minutes.ToString("D2") + ":" + seconds.ToString("D2") + "]</color>" + '\n';
        }

        textEvents.text = events;
        textTimestamps.text = stamps;
    }

    public static void Add(float timeStamp, string text)
    {
        Add(new SingleEvent(timeStamp, text));
    }


    public static void Add(string text)
    { // this method assumes current time
        Add(new SingleEvent(NetworkGameManager.GetGameTime(), text));
    }

    public static void Add(SingleEvent e)
    {
        eventList.Add(e);
    }
}
