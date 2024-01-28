using RabbitMQ.Client;
using RawRabbit.Pipe;
using System.Collections.Generic;

namespace RawRabbit.Configuration
{
	public class BasicPropertiesConfiguration
	{
		public string Type { get; set; }
		public string MessageId { get; set; }
		public byte? DeliveryMode { get; set; }
		public string CorrelationId { get; set; }
		public string ContentType { get; set; }
		public string ContentEncoding { get; set; }
		public string UserId { get; set; }
		public string ReplyTo { get; set; }
		public PublicationAddress ReplyToAddress { get; set; }

		/// <summary>
		/// Expiration in Milliseconds
		/// </summary>
		public string Expiration { get; set; }
	
		public Dictionary<string, object> Headers { get; } = new Dictionary<string, object>();

		public IBasicProperties UpdateBasicProps(IBasicProperties basicProperties)
		{
			BasicPropertiesConfiguration props = this;

			foreach (var prop in props.Headers)
			{
				DictionaryExtensions.TryAdd(basicProperties.Headers, prop.Key, prop.Value);
			}
			if (!string.IsNullOrEmpty(props.Type))
			{
				basicProperties.Type = props.Type;
			}
			if (!string.IsNullOrEmpty(props.MessageId))
			{
				basicProperties.MessageId = props.MessageId;
			}
			if (props.DeliveryMode.HasValue)
			{
				basicProperties.DeliveryMode = props.DeliveryMode.Value;
			}
			if (!string.IsNullOrEmpty(props.ContentType))
			{
				basicProperties.ContentType = props.ContentType;
			}
			if (!string.IsNullOrEmpty(props.ContentEncoding))
			{
				basicProperties.ContentEncoding = props.ContentEncoding;
			}
			if (!string.IsNullOrEmpty(props.UserId))
			{
				basicProperties.UserId = props.UserId;
			}
			if (!string.IsNullOrEmpty(props.CorrelationId))
			{
				basicProperties.CorrelationId = props.CorrelationId;
			}
			if (!string.IsNullOrEmpty(props.ReplyTo))
			{
				basicProperties.ReplyTo = props.ReplyTo;
			}
			if (props.ReplyToAddress != null)
			{
				basicProperties.ReplyToAddress = props.ReplyToAddress;
			}
			if (!string.IsNullOrEmpty(props.Expiration))
			{
				basicProperties.Expiration = props.Expiration;
			}

			return basicProperties;
		}
	}
}
