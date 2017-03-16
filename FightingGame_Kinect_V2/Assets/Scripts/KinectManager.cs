﻿using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;
using System;

public class KinectManager : MonoBehaviour {

    // public Text GestureTextGameObject;
    //public Text ConfidenceTextGameObject;
    //public GameObject Player;
    //private Turning turnScript;

    // Kinect 
    private KinectSensor _Sensor;

    // color frame and data 
    private ColorFrameReader colorFrameReader;
    private byte[] colorData;
    private Texture2D colorTexture;

    private BodyFrameReader bodyFrameReader;
    private int bodyCount;
    private Body[] bodies;

    //private string leanLeftGestureName = "Lean_Left";
    //private string leanRightGestureName = "Lean_Right";
    private string straightPunch = "Right_Straight_Punch_Right";

    // GUI output
    //private UnityEngine.Color[] bodyColors;
    //private string[] bodyText;

    /// <summary> List of gesture detectors, there will be one detector created for each potential body (max of 6) </summary>
    private List<GestureDetector> gestureDetectorList = null;

    Animator animator;

    private float rightPunch;

    private int count;

    // Used to assign trackind id's so that two players can be differentiated while in game
    private ulong player_1;
    private ulong player_2;

    // Use this for initialization
    void Start()
    {

        // Get the sensor
        this._Sensor = KinectSensor.GetDefault();

        // If sensor is working
        if (this._Sensor != null)
        {

            bodyFrameReader = this._Sensor.BodyFrameSource.OpenReader();

            this.bodyCount = this._Sensor.BodyFrameSource.BodyCount;

            this.bodies = new Body[this.bodyCount];

            this.gestureDetectorList = new List<GestureDetector>();

            for (int bodyIndex = 0; bodyIndex < this.bodyCount; bodyIndex++)
            {
                this.gestureDetectorList.Add(new GestureDetector(this._Sensor));
               
            }

            // Open the sensor
            this._Sensor.Open();

            Debug.Log("Kinect is open");

            animator = GetComponent<Animator>();
            
        }// End if

     }// End Start

    // Update is called once per frame
    void Update()
    {
        //rightPunch = animator.GetFloat("Punch");

        // process bodies
        bool newBodyData = false;

        using (BodyFrame bodyFrame = this.bodyFrameReader.AcquireLatestFrame())
        {
            if (bodyFrame != null)
            {
                bodyFrame.GetAndRefreshBodyData(this.bodies);
                newBodyData = true;
            }
        }

        if (newBodyData)
        {

            // update gesture detectors with the correct tracking id
            for (int bodyIndex = 0; bodyIndex < this.bodyCount; bodyIndex++)
            {
                var body = this.bodies[bodyIndex];

                if (body != null)
                {
                    var trackingId = body.TrackingId;

                    // Code for Initialisation

                    // Give player 1 a tracking id
                    if (player_1 == 0)
                    {
                        player_1 = trackingId;

                        //Debug.Log(player_1);
                    }
                    
                    // Give player 2 a tracking id
                    if (player_1 != 0 && player_2 == 0)
                    {
                        player_2 = trackingId;
                    }
                    // End Initialisation

                    /*if (player_1 > 0)
                    {
                        player_1 = checkTrackingId(trackingId, player_1);

                        //Debug.Log("CHANGE TRACKING ID " + player_1);
                    }*/
                   
                    // if the current body TrackingId changed, update the corresponding gesture detector with the new value
                    if (trackingId != this.gestureDetectorList[bodyIndex].TrackingId)
                    {
                        //GestureTextGameObject.text = "none";
                        //this.bodyText[bodyIndex] = "none";
                        this.gestureDetectorList[bodyIndex].TrackingId = trackingId;

                        player_1 = checkTrackingId(this.gestureDetectorList[bodyIndex].TrackingId, player_1);

                        // if the current body is tracked, unpause its detector to get VisualGestureBuilderFrameArrived events
                        // if the current body is not tracked, pause its detector so we don't waste resources trying to get invalid gesture results
                        this.gestureDetectorList[bodyIndex].IsPaused = (trackingId == 0);
                       
                        this.gestureDetectorList[bodyIndex].OnGestureDetected += CreateOnGestureHandler(bodyIndex, trackingId);
                       
                    }
                }
            }
        }

    }// End Update

    private EventHandler<GestureEventArgs> CreateOnGestureHandler(int bodyIndex, ulong trackingId)
    {
        return (object sender, GestureEventArgs e) => OnGestureDetected(sender, e, bodyIndex, trackingId);
    }

    private void OnGestureDetected(object sender, GestureEventArgs e, int bodyIndex, ulong trackingId)
    {
        
        var isDetected = e.IsBodyTrackingIdValid && e.IsGestureDetected;

        //Debug.Log(e.GestureID);

        if (e.GestureID == straightPunch)
        {

            //NEW UI FOR GESTURE DETECTed
            //GestureTextGameObject.text = "Gesture Detected: " + isDetected;
            //StringBuilder text = new StringBuilder(string.Format("Gesture Detected? {0}\n", isDetected));
           // ConfidenceTextGameObject.text = "Confidence: " + e.DetectionConfidence;
            //text.Append(string.Format("Confidence: {0}\n", e.DetectionConfidence));
            if (e.DetectionConfidence > 0.7)
            {
                
                ulong player1 = player_1;
                ulong player2 = player_2;

                //Debug.Log("Punch " + " Confidence " + e.DetectionConfidence + "    Counter " + count);
                count = 0;

                /*for (int i =0; i < 6; i++)
                {
                    Debug.Log("Bodies tracking id " + this.bodies[i].TrackingId + "  " + i);
                }*/
               
                if (player1 == player_1 && e.IsGestureDetected)//  this.bodies[0].TrackingId
                {
                    animator.Play("Straight_Right_Punch");
                }

                //Debug.Log("TrackingID " + player1);
            }
            else
            {
                //turnScript.turnLeft = false;
            }
        }

        /*if (e.GestureID == leanRightGestureName)
        {
            //NEW UI FOR GESTURE DETECTed
            //GestureTextGameObject.text = "Gesture Detected: " + isDetected;
            //StringBuilder text = new StringBuilder(string.Format("Gesture Detected? {0}\n", isDetected));
            //ConfidenceTextGameObject.text = "Confidence: " + e.DetectionConfidence;
            //text.Append(string.Format("Confidence: {0}\n", e.DetectionConfidence));
            if (e.DetectionConfidence > 0.65f)
            {
               // turnScript.turnRight = true;
            }
            else
            {
               // turnScript.turnRight = false;
            }
        }*/

        //this.bodyText[bodyIndex] = text.ToString();
    }

    private void OnRightLeanGestureDetected(object sender, GestureEventArgs e, int bodyIndex)
    {
        var isDetected = e.IsBodyTrackingIdValid && e.IsGestureDetected;

        //NEW UI FOR GESTURE DETECTed
        //GestureTextGameObject.text = "Gesture Detected: " + isDetected;
        //StringBuilder text = new StringBuilder(string.Format("Gesture Detected? {0}\n", isDetected));
        //ConfidenceTextGameObject.text = "Confidence: " + e.DetectionConfidence;
        //text.Append(string.Format("Confidence: {0}\n", e.DetectionConfidence));
        if (e.DetectionConfidence > 0.65f)
        {
           // turnScript.turnRight = true;
        }
        else
        {
           // turnScript.turnRight = false;
        }

        //this.bodyText[bodyIndex] = text.ToString();
    }

    void OnApplicationQuit()
    {
        if (this.colorFrameReader != null)
        {
            this.colorFrameReader.Dispose();
            this.colorFrameReader = null;
        }

        if (this.bodyFrameReader != null)
        {
            this.bodyFrameReader.Dispose();
            this.bodyFrameReader = null;
        }

        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }

            _Sensor = null;
        }
    }

    private ulong checkTrackingId(ulong currentTrackingId, ulong playerId)
    {

        if (currentTrackingId == playerId)
        {

            player_1 = playerId;

            return player_1;
        }
        else
        {

            player_1 = currentTrackingId;

            Debug.Log("New tracking id =====> " + player_1);

            return player_1;
        }

    }

    // bodies = new Body[this.kinectSensor.BodyFrameSource.BodyCount];

    /* this.bodyCount = this.kinectSensor.BodyFrameSource.BodyCount;

     var id = this.kinectSensor.UniqueKinectId;

     Debug.Log("id is " + id);

     Debug.Log("Body count is " + this.bodyCount);*/

    /*if (this.kinectSensor != null)
    {
        this.bodyCount = this.kinectSensor.BodyFrameSource.BodyCount;

        // color reader
        this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();

        // create buffer from RGBA frame description
        var desc = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);


        // body data
        this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

        // body frame to use
        this.bodies = new Body[this.bodyCount];

        // initialize the gesture detection objects for our gestures
        //this.gestureDetectorList = new List<GestureDetector>();
        for (int bodyIndex = 0; bodyIndex < this.bodyCount; bodyIndex++)
        {
            //PUT UPDATED UI STUFF HERE FOR NO GESTURE
            //GestureTextGameObject.text = "none";
            //this.bodyText[bodyIndex] = "none";
            //this.gestureDetectorList.Add(new GestureDetector(this.kinectSensor));
        }

        // start getting data from runtime
        this.kinectSensor.Open();
    }
    else
    {
        //kinect sensor not connected
    }*/



    // ========================  Put in update  ================================

    /*
        // process bodies
        bool newBodyData = false;

        using (BodyFrame bodyFrame = this.bodyFrameReader.AcquireLatestFrame())
        {
            if (bodyFrame != null)
            {
                bodyFrame.GetAndRefreshBodyData(this.bodies);
                newBodyData = true;

                Debug.Log("you Andrejs");
            }
        }

        if (newBodyData)
        {
            // update gesture detectors with the correct tracking id
            for (int bodyIndex = 0; bodyIndex < this.bodyCount; bodyIndex++)
            {
                var body = this.bodies[bodyIndex];
                if (body != null)
                {
                    var trackingId = body.TrackingId;

                    Debug.Log(trackingId);

                    // if the current body TrackingId changed, update the corresponding gesture detector with the new value
                    if (trackingId != this.gestureDetectorList[bodyIndex].TrackingId)
                    {
                        //GestureTextGameObject.text = "none";
                        //this.bodyText[bodyIndex] = "none";
                        this.gestureDetectorList[bodyIndex].TrackingId = trackingId;

                        // if the current body is tracked, unpause its detector to get VisualGestureBuilderFrameArrived events
                        // if the current body is not tracked, pause its detector so we don't waste resources trying to get invalid gesture results
                        this.gestureDetectorList[bodyIndex].IsPaused = (trackingId == 0);
                        this.gestureDetectorList[bodyIndex].OnGestureDetected += CreateOnGestureHandler(bodyIndex);
                    }
                }
            }
        }
        */

}




