using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace CBR.Core.Helpers
{
	/// <summary>
	/// Serialize/Deserialize object to XML file or stream. Internal, use SerializeHelper
	/// </summary>
	public class XmlHelper
	{
		/// <summary>
		/// Serialize an object to a given file
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="objToSerialize"></param>
		/// <returns></returns>
        static public bool Serialize(string filePath, object objToSerialize)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("XmlHelper.Serialize");
			
			StreamWriter writer = null;
			XmlSerializer xmls = null;

			try
			{
				writer = new StreamWriter(filePath);
				xmls = new XmlSerializer(objToSerialize.GetType());

				xmls.Serialize(writer, objToSerialize);
			}
			catch (Exception err)
			{
				LogHelper.Manage("XmlHelper.Serialize", err);
				return false;
			}
			finally
			{
				xmls = null;
				writer.Close();
				LogHelper.End("XmlHelper.Serialize");
			}
			return true;
		}

		/// <summary>
		/// Deserialize an object from a given file
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="objType"></param>
		/// <returns></returns>
        static public object Deserialize(string filePath, Type objType)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("XmlHelper.Deserialize");

			object objToDeserialize = null;

			XmlTextReader xmlReader = null;
			XmlSerializer xmls = null;

			try
			{
				xmlReader = new XmlTextReader(filePath);
				xmls = new XmlSerializer(objType);

				objToDeserialize = xmls.Deserialize(xmlReader);
			}
			catch (Exception err)
			{
                LogHelper.Manage("XmlHelper:Deserialize", err);
				return null;
			}
			finally
			{
				xmls = null;
				xmlReader.Close();
				LogHelper.End("XmlHelper.Deserialize");
			}

			return objToDeserialize;
		}

		/// <summary>
		/// Serialize an object to a given stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="objToSerialize"></param>
		/// <returns></returns>
        static public bool Serialize(Stream stream, object objToSerialize)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("XmlHelper.Serialize");

			StreamWriter writer = null;
			XmlSerializer xmls = null;

			try
			{
				writer = new StreamWriter(stream);
				xmls = new XmlSerializer(objToSerialize.GetType());

				xmls.Serialize(writer, objToSerialize);
			}
			catch (Exception err)
			{
                LogHelper.Manage("XmlHelper:Serialize", err);
				return false;
			}
			finally
			{
				xmls = null;
				writer.Close();
				LogHelper.End("XmlHelper.Serialize");
			}
			return true;
		}

		/// <summary>
		/// Deserialize an object from a given stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="objType"></param>
		/// <returns></returns>
        static public object Deserialize(Stream stream, Type objType)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("XmlHelper.Deserialize");

			object objToDeserialize = null;

			XmlTextReader xmlReader = null;
			XmlSerializer xmls = null;

			try
			{
				xmlReader = new XmlTextReader(stream);
				xmls = new XmlSerializer(objType);

				objToDeserialize = xmls.Deserialize(xmlReader);
			}
			catch (Exception err)
			{
                LogHelper.Manage("XmlHelper:Deserialize", err);
				return null;
			}
			finally
			{
				xmls = null;
				xmlReader.Close();
				LogHelper.End("XmlHelper.Deserialize");
			}

			return objToDeserialize;
		}
	}
}
