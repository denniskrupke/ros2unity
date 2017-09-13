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
using AbstractionLayer;



namespace RosBridge{
	/* TODO:
		- automatic reconnect
		- latest frame buffers (robot state, image, point cloud, ...)
	*/
	class RosBridgeClient {
		private static readonly string ROSBRIDGE_IP = "134.100.13.202";//"192.168.104.159";//
		private static readonly string ROSBRIDGE_PORT = "8080";
//		private static readonly bool DEBUG = false;

		private static WebSocket rosBridgeWebSocket = null;
		private static UTF8Encoding encoder = new UTF8Encoding();

//		private static int millisSinceLastCallOfSomething = Environment.TickCount;
//		private static int countFrames = 0;
//		private static string rosMessageJSONstring = "";
		private int millisSinceLastArmUpdate = Environment.TickCount;
		private static bool verbose;

		private CompressedImage latestImage;
		private JointState latestJointState;
		private Thread threadWorkerCommunication;
		private Thread threadWorkerMessageProcessing;
		private Thread threadWorkerCommandProcessing;
		private Queue<string> rosMessageStrings;
		private Queue<RosMessage> rosCommandQueue;
		private bool processMessageQueue = true;
		private bool processCommandQueue = true;
		private readonly object syncObjMessageQueue = new object();
		private readonly object syncObjCommandQueue = new object();

		private RosPublish rosPublishIncoming;
		private RosMessageConverter rosMessageConverter;

		// parameters for queue processińg frequency
		private static readonly int minSleep = 1;
		private static readonly int maxSleep = 200;
		private static readonly int initialSleep = 20;
		private int inputSleep = initialSleep;
		private int outputSleep = initialSleep;

		private static bool imageStreaming;
		private static bool jointStates;
		private static bool testLatency;
		private static bool pointCloud;

		private StreamWriter streamWriter;
		private List<string> latencyData;
	

		public RosBridgeClient(bool verbose, bool imageStreaming, bool jointStates, bool testLatency, bool pointCloud){
			RosBridgeClient.verbose = verbose;
			RosBridgeClient.imageStreaming = imageStreaming;
			RosBridgeClient.jointStates = jointStates;
			RosBridgeClient.testLatency = testLatency;

			latestImage = new CompressedImage ();				// Buffer for latest incoming image message
			latestJointState = new JointState ();				// Buffer for latest incoming jointState	
			rosMessageStrings = new Queue<string> ();			// Incoming message queue
			rosMessageConverter = new RosMessageConverter ();	// Deserializing of incoming ROSmessages
			rosCommandQueue = new Queue<RosMessage> ();			// Outgoing message queue

			streamWriter = new StreamWriter("latencyData.txt");
			latencyData = new List<string> ();
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



		// TODO: I have to be aware of too many deserializations of messages by filtering the flood of messages. Naive approach isk classifying by names...
		private void ReceiveAndEnqueue(object sender, MessageEventArgs e){
			//MaybeLog ("Receive...");
			if(!string.IsNullOrEmpty(e.Data)) {
				if (e.Data.Contains ("elbow")) { // this tries to find the robotic arm joint values which are published with very high frequency -> scales down to 10 Hz
					if (millisSinceLastArmUpdate + 100 > Environment.TickCount) {
						return;
					}
					millisSinceLastArmUpdate = Environment.TickCount;
				}
				lock (syncObjMessageQueue) {
					//MaybeLog ("Enqueue...");               
					this.rosMessageStrings.Enqueue (e.Data);
					//MaybeLog("" + rosMessageStrings.Count ());
                    Debug.Log("" + rosMessageStrings.Count());
				}
			}
	    }


		private void ProcessRosMessageQueue(){			
			while(processMessageQueue){
				//MaybeLog ("ProcessRosMessageQueue...");
				if (this.rosMessageStrings.Count()>0) {
					if (this.rosMessageStrings.Count >= 1) {
						inputSleep = Math.Max(minSleep, inputSleep-1);
					} 
					//MaybeLog ("Try to dequeue...");
					lock (syncObjMessageQueue) {
						//MaybeLog ("Dequeue...");
						DeserializeJSONstring (rosMessageStrings.Dequeue ());
						//rosMessageStrings.TrimExcess ();
					}
				}
				else {
					inputSleep = Math.Min(maxSleep, inputSleep+1);
				}
				//MaybeLog("" + rosMessageStrings.Count ());
				Thread.Sleep (inputSleep); //TODO If sleep is to small (depending on the performance of the current machine) no messages can be enqueued :-(
				//MaybeLog ("input sleep "+inputSleep);
			}
		}


		public void EnqueRosCommand(RosMessage message){
			lock (syncObjCommandQueue) {
				this.rosCommandQueue.Enqueue (message);
				MaybeLog("" + rosCommandQueue.Count ());
			}
		}


		private void ProcessRosCommandQueue(){
			while (processCommandQueue) {
				if (this.rosCommandQueue.Count > 0) {
					if (this.rosCommandQueue.Count >= 1) {
						outputSleep = Math.Max(minSleep, outputSleep-1);
					} 
					lock (syncObjCommandQueue) {
						Send (rosCommandQueue.Dequeue ());
					}
				}
				else {
					outputSleep = Math.Min(maxSleep, outputSleep+1);
				}
				//MaybeLog("commandSleep: " + outputSleep);
				Thread.Sleep (outputSleep);

			}
		}


		// starts the communication: connects and subscribes to topics
	    private void Communicate(){	
			MaybeLog ("--- inside communicate ---");
			MaybeLog ("Try to connect with ROSbridge...");
			Connect("ws://"+ROSBRIDGE_IP+":"+ROSBRIDGE_PORT);
			Thread.Sleep(2000);
			MaybeLog ("...connect done!");

			if (rosBridgeWebSocket.IsConnected) {
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
			Debug.Log(JsonConvert.SerializeObject(msg));
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
			Debug.Log ("Try to deserialize: " + message);
			rosPublishIncoming = (RosPublish)JsonConvert.DeserializeObject<RosMessage>(message, rosMessageConverter);
			//Debug.Log (rosPublish.topic);
			if (rosPublishIncoming.topic.Equals ("/joint_states")) {
				//Debug.Log ("received joint states");
				latestJointState = (JointState)rosPublishIncoming.msg;
			
				// TODO latency evaluation
				//if(testLatency){
				//	if(latestJointState.header.frame_id == "Latency Test"){
						// TODO storing this information to a file
						DateTime currentTime = DateTime.Now;
						int secs = currentTime.Second;
						int nsecs = currentTime.Millisecond;
						//Debug.Log(""+latestJointState.header.frame_id+" "+latestJointState.header.seq+" "+latestJointState.header.stamp.secs+":"+latestJointState.header.stamp.nsecs );
						//Debug.Log ("received: " + secs + ":" + nsecs);
						string dataLine = "" + (latestJointState.header.stamp.secs * 1000 + latestJointState.header.stamp.nsecs) + "," + (secs * 1000 + nsecs) + "," + ((secs * 1000 + nsecs) - (latestJointState.header.stamp.secs * 1000 + latestJointState.header.stamp.nsecs));
						//Debug.Log ("dataline " + dataLine);
						latencyData.Add(dataLine);
						//streamWriter.WriteLine ("" + latestJointState.header.stamp.secs * 1000 + latestJointState.header.stamp.nsecs + "," + secs * 1000 + nsecs + "," + ((secs * 1000 + nsecs) - (latestJointState.header.stamp.secs * 1000 + latestJointState.header.stamp.nsecs)));
						//streamWriter.Close ();
				//	}
				//}
			}
            // else if (rosPublishIncoming.topic.Equals("/camera/rgb/image_rect_color/compressed")) { 	
			else if (rosPublishIncoming.topic.Equals ("/usb_cam/image_raw/compressed")) {				
				latestImage = (CompressedImage)rosPublishIncoming.msg;
			}
		}

		public void WriteLatencyDataFile(){
			Debug.Log ("Writing data file of size "+latencyData.Count());
			foreach (var item in latencyData) {
				streamWriter.WriteLine (item);
				Debug.Log (item);
			}
			streamWriter.Flush ();
			streamWriter.Close ();
		}


		public CompressedImage GetLatestImage(){
			return latestImage;
		}


		public JointState GetLatestJoinState (){
			return latestJointState;
		}

		//--------------------------------------------------------------------------------------------------------------

		//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
/*
		// DEPRECATED
		private static void testNewstonsoftDeserialization(){
			RosSubscribe subscribe = new RosSubscribe("/camera/rgb/image_rect_color/compressed", "sensor_msgs/CompressedImage");
			Console.WriteLine(""+subscribe);
			Console.WriteLine();
			string serializedMessage = createRosMessageString(subscribe);
			//serializedMessage = "{\"op\":\"publish\",\"id\":\"publish:/SModelRobotOutput:12\",\"topic\":\"/SModelRobotOutput\",\"msg\":{\"rACT\":1,\"rMOD\":0,\"rGTO\":1,\"rATR\":0,\"rGLV\":0,\"rICF\":0,\"rICS\":0,\"rPRA\":255,\"rSPA\":255,\"rFRA\":150,\"rPRB\":0,\"rSPB\":0,\"rFRB\":0,\"rPRC\":0,\"rSPC\":0,\"rFRC\":0,\"rPRS\":0,\"rSPS\":0,\"rFRS\":0},\"latch\":false}";		
			serializedMessage = "{\"topic\": \"/camera/rgb/image_rect_color/compressed\", \"msg\": {\"header\": {\"stamp\": {\"secs\": 1447862289, \"nsecs\": 606011860}, \"frame_id\": \"camera_rgb_optical_frame\", \"seq\": 76394}, \"data\": \"/9j/4AAQSkZJRgABAQAAAQABAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAHgAoADASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD5UooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigD//Z\", \"format\": \"rgb8; jpeg compressed bgr8\"}, \"op\": \"publish\"}";
			Console.WriteLine("serializedMessage: "+serializedMessage);
			Console.WriteLine();
			byte[] buffer = encoder.GetBytes(serializedMessage);
			Console.WriteLine("buffer: "+buffer);
			Console.WriteLine();
			string reencodedString = encoder.GetString(buffer);
			Console.WriteLine("reencodedString: "+reencodedString);
			Console.WriteLine();
			//RosSubscribe subscribeII = JsonConvert.DeserializeObject<RosSubscribe>(reencodedString);
			//RosPublish subscribeII = JsonConvert.DeserializeObject<RosPublish>(reencodedString);
			RosPublish subscribeII = (RosPublish)JsonConvert.DeserializeObject<RosMessage>(reencodedString, new RosMessageConverter());

			Console.WriteLine(""+subscribeII);
			// Console.WriteLine(""+subscribeII.type);
			// Console.WriteLine(""+subscribeII.compression);
			// Console.WriteLine(""+subscribeII.topic);
			// Console.WriteLine(""+subscribeII.compression);
			// Console.WriteLine(""+subscribeII.throttle_rate);
			// Console.WriteLine(""+subscribeII.queue_length);
			Console.WriteLine(""+subscribeII.op);
			Console.WriteLine(""+subscribeII.id);
			Console.WriteLine(""+subscribeII.topic);
			Console.WriteLine(""+subscribeII.msg);
			Console.WriteLine(""+subscribeII.latch);

			//OutputMessageData data = (OutputMessageData) subscribeII.msg;
			CompressedImage data = (CompressedImage) subscribeII.msg;
			Console.WriteLine(""+data.header);
			Console.WriteLine(""+data.data);
			//Console.WriteLine(""+data.rMOD);
		}

		// DEPRECATED
		private String parseMessageBuffer(byte[] buffer){
			String message = encoder.GetString(buffer);
			if(message.Contains("{")){
				int begin = message.IndexOf("{");
				int end = message.Length;
				rosMessageJSONstring = message.Substring(begin, end);
			}
			else if (message.Contains("}")){
				int begin = 0;
				int end = message.LastIndexOf("}")+1;
				rosMessageJSONstring += message.Substring(begin, end);
				if(DEBUG) {
					checkFPS();    		
					logStatus(buffer);
				}
			}
			else {
				rosMessageJSONstring += message;
			}
			return rosMessageJSONstring;
		}

		// DEPRECATED
		private String parseMessage(string message){
			if(message.Contains("{")){
				int begin = message.IndexOf("{");
				int end = message.Length;
				rosMessageJSONstring = message.Substring(begin, end);
			}
			else if (message.Contains("}")){
				int begin = 0;
				int end = message.LastIndexOf("}")+1;
				rosMessageJSONstring += message.Substring(begin, end);
				if(DEBUG) {
					checkFPS();    		
					logStatus(message);
				}
			}
			else {
				rosMessageJSONstring += message;
			}
			return rosMessageJSONstring;
		}

		//DEPRECATED
		private void checkFPS(){
			if(millisSinceLastCallOfSomething+1000 < Environment.TickCount){
				Debug.Log("FPS: "+(countFrames + 1));
				millisSinceLastCallOfSomething = Environment.TickCount;
				countFrames = 0;
			}
			else{
				countFrames++;
			}
		}

		// DEPRECATED
		private static RosMessage convertToRosMessage(string msg){
			maybeLog ("Try to convert to JSON object..");

			RosPublish rosMessage = null;
			if(!String.IsNullOrEmpty(msg)){
				try 
				{
					//rosMessage = JsonConvert.DeserializeObject<RosPublish>(msg);
					msg = "{\"op\":\"publish\",\"id\":\"publish:/SModelRobotOutput:12\",\"topic\":\"/SModelRobotOutput\",\"msg\":{\"rACT\":1,\"rMOD\":0,\"rGTO\":1,\"rATR\":0,\"rGLV\":0,\"rICF\":0,\"rICS\":0,\"rPRA\":255,\"rSPA\":255,\"rFRA\":150,\"rPRB\":0,\"rSPB\":0,\"rFRB\":0,\"rPRC\":0,\"rSPC\":0,\"rFRC\":0,\"rPRS\":0,\"rSPS\":0,\"rFRS\":0},\"latch\":false}";		
					rosMessage = JsonConvert.DeserializeObject<RosPublish>(msg);
					maybeLog("...conversion done");

				} catch (System.Exception e) 
				{
					Debug.Log("Exception: {Deserialize}"+e.ToString());
				} finally{}			
			}
			else{
				maybeLog ("...was empty");
			}
			return rosMessage;
		}
*/

	}
}