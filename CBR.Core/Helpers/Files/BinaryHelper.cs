using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace CBR.Core.Helpers
{
	/// <summary>
	/// Serialize/Deserialize object to binary file or stream. Internal, use SerializeHelper
	/// </summary>
	internal class BinaryHelper
	{
		/// <summary>
		/// Serialize an object to a given file
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="objToSerialize"></param>
		/// <returns></returns>
		static internal bool Serialize(string filePath, object objToSerialize)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("BinaryHelper.Serialize");

			if (objToSerialize == null)
				return false;

			IFormatter formatter = new BinaryFormatter();
			Stream stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
			try
			{
				formatter.Serialize(stream, objToSerialize);
			}
			catch (Exception err)
			{
				LogHelper.Manage("BinaryHelper:Serialize", err);
				return false;
			}
			finally
			{
				stream.Close();
				LogHelper.End("BinaryHelper.Serialize");
			}
			return true;
		}

		/// <summary>
		/// Deserialize an object from a given file
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		static internal object Deserialize(string filePath)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("BinaryHelper.Deserialize");
			
			// file does not exist, return null
			if (!File.Exists(filePath))
				return null;

			object objToDeserialize = null;

			IFormatter formatter = new BinaryFormatter();
			Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);

			try
			{
				objToDeserialize = formatter.Deserialize(stream);
			}
			catch (Exception err)
			{
				LogHelper.Manage("BinaryHelper.Deserialize", err);
				return null;
			}
			finally
			{
				stream.Close();
				LogHelper.End("BinaryHelper.Deserialize");
			}
			return objToDeserialize;
		}

		/// <summary>
		/// Serialize an object to a given stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="objToSerialize"></param>
		/// <returns></returns>
		static internal bool Serialize(Stream stream, object objToSerialize)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("BinaryHelper.Serialize");

			if (objToSerialize == null)
				return false;

			IFormatter formatter = new BinaryFormatter();
			try
			{
				formatter.Serialize(stream, objToSerialize);
			}
			catch (Exception err)
			{
				LogHelper.Manage("BinaryHelper:Serialize", err);
				return false;
			}
			finally
			{
				LogHelper.End("BinaryHelper.Serialize");
			}
			return true;
		}

		/// <summary>
		/// Deserialize an object from a given stream
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		static internal object Deserialize(Stream stream)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("BinaryHelper.Deserialize");

			object objToDeserialize = null;

			IFormatter formatter = new BinaryFormatter();

			try
			{
				objToDeserialize = formatter.Deserialize(stream);
			}
			catch (Exception err)
			{
				LogHelper.Manage("BinaryHelper:Deserialize", err);
				return null;
			}
			finally
			{
				LogHelper.End("BinaryHelper.Deserialize");
			}
			return objToDeserialize;
		}
	}
}
