namespace RosMessages{

	/*
		MessageData contains all neccessary information for commands controlling robots
	*/
	public class MessageData{}


	// contains information for controlling the robotiq-3finger adaptive gripper
	public class OutputMessageData : MessageData{
		public uint rACT = 0;
		public uint rMOD = 0;
		public uint rGTO = 0;
		public uint rATR = 0;
		public uint rGLV = 0;
		public uint rICF = 0;
		public uint rICS = 0;
		public uint rPRA = 0;
		public uint rSPA = 0;
		public uint rFRA = 0;
		public uint rPRB = 0;
		public uint rSPB = 0;
		public uint rFRB = 0;
		public uint rPRC = 0;
		public uint rSPC = 0;
		public uint rFRC = 0;
		public uint rPRS = 0;
		public uint rSPS = 0;
		public uint rFRS = 0;
	}

	// contains information about the current state of the robotiq-3finger adaptive gripper
	public class InputMessageData : MessageData{
		public uint gACT = 0; 
		public uint gMOD = 0; 
		public uint gGTO = 0; 
		public uint gIMC = 0; 
		public uint gSTA = 0; 
		public uint gDTA = 0; 
		public uint gDTB = 0; 
		public uint gDTC = 0; 
		public uint gDTS = 0; 
		public uint gFLT = 0; 
		public uint gPRA = 0; 
		public uint gPOA = 0; 
		public uint gCUA = 0; 
		public uint gPRB = 0; 
		public uint gPOB = 0; 
		public uint gCUB = 0; 
		public uint gPRC = 0; 
		public uint gPOC = 0; 
		public uint gCUC = 0; 
		public uint gPRS = 0; 
		public uint gPOS = 0; 
		public uint gCUS = 0;
	}

	public class Stamp{
		public int secs;
		public int nsecs;
	}

	public class Header{
		public Stamp stamp;
		public string frame_id; 
		public ulong seq;
		//public string type;
	}

	public class CompressedImage : MessageData{
		public Header header;
		public string data;	
		public string format;
	}

	public class JointState : MessageData{
		public Header header;
		public string[] name;
		public double[] position;
		public double[] velocity;
		public double[] effort;
		//public string type = "SModel_robot_output";
	}


	public class PointField{
		public const byte INT8 = 1;
		public const byte UINT8 = 2;
		public const byte INT16 = 3;
		public const byte UINT16 = 4;
		public const byte INT32 = 5;
		public const byte UINT32 = 6;
		public const byte FLOAT32 = 7;
		public const byte FLOAT64 = 8;
		public string name;
		public ulong offset;
		public byte datatype;
		public ulong count;
	}

	public class PointCloud2 : MessageData{
		public Header header;
		public uint height;
		public uint width;
		public PointField[] fields;
		public bool is_bigendian;
		public uint point_step;
		public uint row_step;
		public ushort[] data;
		public bool is_dense;
	}

	/*
		Generates proper ROS message objects which can be serialized to JSON strings 
	*/
	public class RosMessage{
		public static int _id = 0;
		public string op;
		public string id;
		public string topic;
		public string type;
	}

	public class RosAdvertise : RosMessage{
		//public string type;
		public string queue_size;
		public string latch;

		public RosAdvertise(string topic, string messageType){
			++RosMessage._id;
			this.op = "advertise";
			this.id = "advertise:"+topic+":"+_id;
			this.type = messageType;
			this.topic = topic;
			this.latch = "false";
			this.queue_size = "100";
		}
	}

	public class RosPublish : RosMessage{
		public MessageData msg;
		public bool latch;

		public RosPublish(string topic, MessageData messageData){
			++RosMessage._id;
			this.op = "publish";
			this.id = "publish:"+topic+":"+_id;
			this.topic = topic;
			this.msg = messageData;
			this.latch = false;
		}
	}

	public class RosSubscribe : RosMessage{
		//public string type;
		public string compression;
		public int throttle_rate;
		public int queue_length;

		public RosSubscribe(string topic, string messageType){
			++RosMessage._id;
			this.op = "subscribe";
			this.id = "subscribe:"+topic+":"+_id;
			this.type = messageType;
			this.topic = topic;
			this.compression = "none";
			this.throttle_rate = 0;
			this.queue_length = 0;
		}
	}

	public class RosUnsubscribe : RosMessage{
		public RosUnsubscribe(string topic){
			++RosMessage._id;
			this.op = "unsubscribe";
			this.id = "unsubscribe:"+topic+":"+_id;
			this.topic = topic;
		}
	}

}