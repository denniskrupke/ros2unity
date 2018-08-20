using System.Collections;
using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using WebSocketSharp;
using WebSocketSharp.Net;
using Newtonsoft.Json;
using UnityEngine;

using RosJSON;
using RosMessages;
using RosMessages.moveit_msgs;



namespace RosBridge{
	
	public class RosBridgeClient {
        private static RosBridgeClient instance = null;
		private static string ROSBRIDGE_IP = "134.100.13.202";
		private static string ROSBRIDGE_PORT = "8080";

		private static WebSocket rosBridgeWebSocket = null;
		private static UTF8Encoding encoder = new UTF8Encoding();
        
		private static bool verbose  = false;
        
        // buffering the latest collision objects
        //private CollisionObject latest_collisionObject = null;
        private Dictionary<string, CollisionObject> collisionObjects = new Dictionary<string, CollisionObject>();

        // threads for concurrent message processsing
        private Thread threadWorkerCommunication;
		private Thread threadWorkerMessageProcessing;
		private Thread threadWorkerCommandProcessing;

        // queues for incoming and outgoing messages
        private Queue<string> rosMessageStrings;
		private Queue<RosMessage> rosCommandQueue;

        private bool processMessageQueue = true;
		private bool processCommandQueue = true;

        // locks for managing concurrent access to the queues
        private readonly object syncObjMessageQueue = new object();
		private readonly object syncObjCommandQueue = new object();

		private RosPublish rosPublishIncoming;
		private RosMessageConverter rosMessageConverter;

        /*
		// parameters for queue processińg frequency
		private static readonly int minSleep = 1;
		private static readonly int maxSleep = 200;
		private static readonly int initialSleep = 20;
		private int inputSleep = initialSleep;
		private int outputSleep = initialSleep;
        */

        private static bool collisionObject;
		//private static bool imageStreaming;
		//private static bool jointStates;
		//private static bool testLatency;
		//private static bool pointCloud;

	    // lazy singleton pattern
        public static RosBridgeClient GetInstance(string rosmaster_ip, string rosbridge_port, bool verbose, bool collisionObject)
        {
            if (instance == null) instance = new RosBridgeClient(rosmaster_ip, rosbridge_port, verbose, collisionObject);
            return instance;
        }

        // private constructor...use GetInstance() instead
        private RosBridgeClient(string rosmaster_ip, string rosbridge_port, bool verbose, bool collisionObject){
            RosBridgeClient.ROSBRIDGE_IP = rosmaster_ip;
            RosBridgeClient.ROSBRIDGE_PORT = rosbridge_port;
			RosBridgeClient.verbose = verbose;
            RosBridgeClient.collisionObject = collisionObject;

            //latest_collisionObject = new RosMessages.moveit_msgs.CollisionObject();

            rosMessageStrings = new Queue<string> ();			// Incoming message queue
			rosMessageConverter = new RosMessageConverter ();	// Deserializing of incoming ROSmessages
			rosCommandQueue = new Queue<RosMessage> ();			// Outgoing message queue
		}

		public void Start (){
			// communication thread
			MaybeLog("Creating communication thread...");
			threadWorkerCommunication = new Thread (new ThreadStart (Communicate));
			MaybeLog("Starting communication thread...");
			threadWorkerCommunication.Start ();

			// thread for processing incoming messages
			MaybeLog("Creating message processing thread...");
			threadWorkerMessageProcessing = new Thread (new ThreadStart (ProcessRosMessageQueue));
			MaybeLog("Starting message processing thread...");
			threadWorkerMessageProcessing.Start ();

			// thread for sending messages to the remote robot side
			MaybeLog("Creating command processing thread...");
			threadWorkerCommandProcessing = new Thread (new ThreadStart (ProcessRosCommandQueue));
			MaybeLog("Starting message processing thread...");
			threadWorkerCommandProcessing.Start ();
		}

		// connects with the ROSbridge server
	    private void Connect(string uri){
			MaybeLog ("Connect...");
	        try
	        {
	    		rosBridgeWebSocket = new WebSocket(uri);
				rosBridgeWebSocket.OnMessage += ReceiveAndEnqueue;

				int count = 1;
				do{
					MaybeLog("try connecting "+count++);
					rosBridgeWebSocket.Connect();
                    Thread.Sleep(1000);
				}
				while(!rosBridgeWebSocket.IsConnected);        
	        }
	        catch (Exception ex)
	        {
				Debug.Log("Exception: {Connect}"+ex.ToString());
	        }
	        finally{}
	    }

		// closes the websocket connection to the ROSbridge server
	    public void Disconnect(){
	    	try 
	    	{
				rosBridgeWebSocket.Close();
	    	} 
	    	catch (System.Exception e) 
	    	{
				Debug.Log("Exception: {Disconnect}"+e.ToString());
	    	} 
	    	finally {}
	    }

        // checks the websocket connection
		public bool IsConnected(){
			bool connected = (rosBridgeWebSocket == null) ? false : rosBridgeWebSocket.IsConnected;
			return connected;
		}
	
		private void Send(RosMessage message){
			MaybeLog("Send ROSmessage...");
			//Debug.Log (CreateRosMessageString (message));
			byte[] buffer = encoder.GetBytes(CreateRosMessageString(message));
			rosBridgeWebSocket.Send(buffer);
		}



		// TODO: I have to be aware of too many deserializations of messages by filtering the flood of messages. Naive approach is classifying by names...
		private void ReceiveAndEnqueue(object sender, MessageEventArgs e){
			//MaybeLog ("Receive...");
			if(!string.IsNullOrEmpty(e.Data)) {
                /*
				if (e.Data.Contains ("elbow")) { // this tries to find the robotic arm joint values which are published with very high frequency -> scales down to 10 Hz
					if (millisSinceLastArmUpdate + 100 > Environment.TickCount) {
						return;
					}
					millisSinceLastArmUpdate = Environment.TickCount;
				}
                */
                MaybeLog("Try to Enqueue...");
                //Debug.Log("locking...");
                lock (syncObjMessageQueue) {	               
					this.rosMessageStrings.Enqueue (e.Data);
                //Debug.Log(e.Data);
				}
                //Debug.Log("Locking done");

                MaybeLog("" + rosMessageStrings.Count());
            }
	    }


		private void ProcessRosMessageQueue(){			
			while(processMessageQueue){
                //Debug.Log("I am alive");
                Thread.Sleep(5);

                //Debug.Log(this.rosMessageStrings.Count);
                //MaybeLog ("ProcessRosMessageQueue...");
                if (this.rosMessageStrings.Count()>0) {

                    //Debug.Log("ros messages");
                    /*if (this.rosMessageStrings.Count >= 1) {
						inputSleep = Math.Max(minSleep, inputSleep-1);
					} */
                    MaybeLog ("Try to dequeue...");
					//lock (syncObjMessageQueue) {
						//MaybeLog ("Dequeue...");
						DeserializeJSONstring (rosMessageStrings.Dequeue ());
						//rosMessageStrings.TrimExcess ();
					//}
                    Thread.Sleep(2);
                }
                /*
				else {
					inputSleep = Math.Min(maxSleep, inputSleep+1);
				}
                */
				//MaybeLog("" + rosMessageStrings.Count ());
				//Thread.Sleep (inputSleep); //TODO If sleep is to small (depending on the performance of the current machine) no messages can be enqueued :-(
                                           //MaybeLog ("input sleep "+inputSleep);
                
			}
		}


		public void EnqueRosCommand(RosMessage message){
			//lock (syncObjCommandQueue) {
				this.rosCommandQueue.Enqueue (message);
			//}
            MaybeLog("" + rosCommandQueue.Count());
        }


		private void ProcessRosCommandQueue(){
			while (processCommandQueue) {
				if (this.rosCommandQueue.Count > 0) {
                    /*
					if (this.rosCommandQueue.Count >= 1) {
						outputSleep = Math.Max(minSleep, outputSleep-1);
					} 
                    */
					//lock (syncObjCommandQueue) {
						Send (rosCommandQueue.Dequeue ());
					//}
                    Thread.Sleep(2);
                }
                /*
				else {
					outputSleep = Math.Min(maxSleep, outputSleep+1);
				}
                */
                //MaybeLog("commandSleep: " + outputSleep);
                // outputSleep);
			}
		}


		// starts the communication: connects and subscribes to topics
	    private void Communicate(){	
			MaybeLog ("--- inside communicate ---");
			MaybeLog ("Try to connect with ROSbridge...");
			Connect("ws://"+ROSBRIDGE_IP+":"+ROSBRIDGE_PORT);
			Thread.Sleep(2000);
			MaybeLog ("...connect done!");

            // Code should only be reachable if connect() was succesful
			if (rosBridgeWebSocket.IsConnected) {
                /*
				if(RosBridgeClient.imageStreaming) {
					//MaybeLog ("Subscribing to /camera/rgb/image_rect_color/compressed");
					//Send(new RosSubscribe("/camera/rgb/image_rect_color/compressed", "sensor_msgs/CompressedImage"));
                    MaybeLog("Subscribing to /usb_cam/image_raw/compressed");
                    //Send(new RosSubscribe("/usb_cam/image_raw/compressed", "sensor_msgs/CompressedImage"));
					Send(new RosSubscribe("/camera/rgb/image_rect_color/compressed", "sensor_msgs/CompressedImage"));

                }
				if (RosBridgeClient.jointStates) {
					MaybeLog ("Subscribing to /joint_states");
					Send (new RosSubscribe ("/joint_states", "sensor_msgs/JointState"));
				}
				if (RosBridgeClient.pointCloud) {
					MaybeLog ("Subscribing to /camera/depth/points");
					Send (new RosSubscribe ("/camera/depth/points", "sensor_msgs/PointCloud2"));
				}
                */
                if (RosBridgeClient.collisionObject) {
                    MaybeLog("Subscribing to /pr2_phantom/collision_object");
                    Send(new RosSubscribe("/pr2_phantom/collision_object", "moveit_msgs/CollisionObject",0,1));
                }
                                
            } else {
				//Connect("ws://"+ROSBRIDGE_IP+":"+ROSBRIDGE_PORT);
				//Communicate ();
			}
		}

	
		// serializes a RosMessage to a string
		private static string CreateRosMessageString(RosMessage msg){
			//if (msg.topic == "/joint_states") {
			//	return JsonConvert.SerializeObject((RosPublish)msg);
			//}
			//else 
			//Debug.Log(JsonConvert.SerializeObject(msg));
				return JsonConvert.SerializeObject(msg);
		}
			

		// switchable log function, logs only if 'verbose' is set to true
		private static void MaybeLog(string logstring){
			if (verbose) {
				Debug.Log (logstring);
			}
		}

		//--------------------------------------------------------------------------------------------------------------
		// TODO: extending this for more messages
//		{"topic": "/camera/rgb/image_rect_color/compressed", 
//			"msg": {
//				"header": {
//					"stamp": {
//						"secs": 1479891554, 
//						"nsecs": 542033426}, 
//					"frame_id": "camera_rgb_optical_frame", 
//					"seq": 10393}, 
//				"data": "/9j/4AAQSkZJRgABAQAAAQABAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODw}}


		private void DeserializeJSONstring(string message){
			//Debug.Log ("Try to deserialize: " + message);
			rosPublishIncoming = (RosPublish)JsonConvert.DeserializeObject<RosMessage>(message, rosMessageConverter);

            if (rosPublishIncoming.topic.Equals("/pr2_phantom/collision_object"))
            {
                CollisionObject co = (CollisionObject) rosPublishIncoming.msg;
                if (this.collisionObjects.ContainsKey(co.id)) //object exists -> update the old one
                {
                    this.collisionObjects[co.id] = co;
                }
                else this.collisionObjects.Add(co.id, co); // new object -> add to dictionary
            }
		}

        public Dictionary<string,CollisionObject> GetCollisionObjects()
        {
            return this.collisionObjects;
        }

      
    }
}