using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using System;

using RosMessages;

namespace RosJSON
{
	public abstract class JsonCreationConverter<T> : JsonConverter
	{
	    /// <summary>
	    /// Create an instance of objectType, based properties in the JSON object
	    /// </summary>
	    /// <param name="objectType">type of object expected</param>
	    /// <param name="jObject">
	    /// contents of JSON object that will be deserialized
	    /// </param>
	    /// <returns></returns>
	    protected abstract T Create(Type objectType, JObject jObject);

	    public override bool CanConvert(Type objectType)
	    {
	        return typeof(T).IsAssignableFrom(objectType);
	    }

	    public override object ReadJson(JsonReader reader, 
	                                    Type objectType, 
	                                     object existingValue, 
	                                     JsonSerializer serializer)
	    {
	        // Load JObject from stream
	        JObject jObject = JObject.Load(reader);

	        // Create target object based on JObject
	        T target = Create(objectType, jObject);

	        // Populate the object properties
	        serializer.Populate(jObject.CreateReader(), target);

	        return target;
	    }

	    public override void WriteJson(JsonWriter writer, 
	                                   object value,
	                                   JsonSerializer serializer)
	    {
	        throw new NotImplementedException();
	    }
	}


	public class RosMessageConverter : JsonCreationConverter<RosMessage>
	{
	    protected override RosMessage Create(Type objectType, JObject jObject)
	    {
	        if (jObject["op"].ToString() == "advertise")
	        {
	            return new RosAdvertise(jObject["topic"].ToString(), jObject["type"].ToString());
	        }
	        else if (jObject["op"].ToString() == "subscribe")
	        {
	            return new RosSubscribe(jObject["topic"].ToString(), jObject["type"].ToString());
	        }
	        else if (jObject["op"].ToString() == "unsubscribe")
	        {
	            return new RosUnsubscribe(jObject["topic"].ToString());
	        }
	        else if (jObject["op"].ToString() == "publish")
	        {
	            MessageData msg = JsonConvert.DeserializeObject<MessageData>(jObject["msg"].ToString(), new MessageDataConverter(jObject["topic"].ToString()));
	            return new RosPublish(jObject["topic"].ToString(), msg);
	        }
	        else return new RosMessage(); // empty dummy
	    }
	}
	

	public class MessageDataConverter : JsonCreationConverter<MessageData>
	{
	    string topic = "";
	    public MessageDataConverter(string topic){
	        this.topic = topic;
	    }

	    protected override MessageData Create(Type objectType, JObject jObject)
	    {
			switch (topic) {
			case "/SModelRobotInput": {
				return new InputMessageData ();
			} break;
			case "/SModelRobotOutput": {
				return new OutputMessageData();
			} break;
			case "/camera/rgb/image_rect_color/compressed": {
				return new CompressedImage();
			} break;
			case "/joint_states": {
				return new JointState();
			} break;
			case "/camera/depth/points": {
				return new PointCloud2();
			} break;
            case "/pr2_phantom/collision_object": {
                return new RosMessages.moveit_msgs.CollisionObject();
            } break;
                default:  return new MessageData(); // no data inside :-(
			}
	    }

	}
}