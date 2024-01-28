using RabbitMQ.Client;
using System.Collections.Generic;

namespace RawRabbit.Configuration
{
	/// <summary>
	/// Basic properties used for the publishing setup.
	/// Moved to a separate class because as of new RabbitMq Client Library, it does not allow for creation of BasicProperties directly
	/// all properties correspond to <see cref="IBasicProperties"/> interface
	/// </summary>
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

		/// <summary>
		/// Dictionary of headers
		/// </summary>
		public IDictionary<string, object> Headers { get; } = new Dictionary<string, object>();

		/// <summary>
		/// Updates provided <see cref="IBasicProperties"/ instance from own properties>
		/// </summary>
		/// <param name="basicProperties"></param>
		/// <returns>updated properties</returns>
		public IBasicProperties UpdateBasicProps(IBasicProperties basicProperties)
		{
			BasicPropertiesConfiguration props = this;

			foreach (var prop in props.Headers)
			{
				basicProperties.Headers.TryAdd(prop.Key, prop.Value);
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
