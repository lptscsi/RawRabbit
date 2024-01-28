using MessagePack;
using RawRabbit.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace RawRabbit.Enrichers.MessagePack
{
	internal class MessagePackSerializerWorker : ISerializer
	{
		public string ContentType => "application/x-messagepack";
		private readonly MethodInfo _deserializeType;
		private readonly MethodInfo _serializeType;
		private readonly MessagePackSerializerOptions _options;

		public MessagePackSerializerWorker(MessagePackFormat format)
		{
			if (format == MessagePackFormat.LZ4Compression)
			{
				_options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
			}
			else
			{
				_options = MessagePackSerializerOptions.Standard;
			}

			Type tp = typeof(MessagePackSerializer);

			_deserializeType = tp
				.GetMethod(nameof(MessagePackSerializer.Deserialize), new[] { typeof(Stream), typeof(MessagePackSerializerOptions), typeof(CancellationToken) });
			_serializeType = tp
				.GetMethods()
				.FirstOrDefault(s => s.Name == nameof(MessagePackSerializer.Serialize) && s.ReturnType == typeof(byte[]));
		}

		public byte[] Serialize(object obj)
		{
			if (obj == null)
				throw new ArgumentNullException();

			return (byte[])_serializeType
				.MakeGenericMethod(obj.GetType())
				.Invoke(null, new[] { obj, _options, CancellationToken.None });
		}

		public object Deserialize(Type type, byte[] bytes)
		{
			return _deserializeType.MakeGenericMethod(type)
				.Invoke(null, new object[] { new MemoryStream(bytes), _options, CancellationToken.None });
		}

		public TType Deserialize<TType>(byte[] bytes)
		{
			return MessagePackSerializer.Deserialize<TType>(bytes, _options, CancellationToken.None);
		}
	}
}
