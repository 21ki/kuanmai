using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
namespace KMBit.Util
{
    public class XMLUtil
    {
        public static object DeserializeXML(string xmlFilePath, Type type)
        {
            FileStream xmlStream = null;
            object obj = null;
            try
            {
                xmlStream = new FileStream(xmlFilePath, FileMode.Open, FileAccess.Read);
                XmlSerializer xml = new XmlSerializer(type);
                if (null != xmlStream)
                {
                    xmlStream.Seek(0, SeekOrigin.Begin);

                    obj = xml.Deserialize(xmlStream);
                }
                return obj;
            }
            catch { }
            finally
            {
                if (xmlStream != null)
                {
                    xmlStream.Close();
                }
            }

            return null;
        }

        public static object DeserializeXML(Stream stream, Type type)
        {
            object obj = null;
            try
            {
                XmlSerializer xml = new XmlSerializer(type);
                obj = xml.Deserialize(stream);

                return obj;
            }
            catch { }
            finally
            {
            }

            return null;
        }

        public static T DeserializeXML<T>(Stream stream)
        {
            Type type = typeof(T);

            object o = DeserializeXML(stream, type);

            if (o != null)
            {
                return (T)o;
            }
            else
            {
                return default(T);
            }
        }

        public static T DeserializeXML<T>(string xmlFilePath)
        {
            Type type = typeof(T);

            object o = DeserializeXML(xmlFilePath, type);

            if (o != null)
            {
                return (T)o;
            }
            else
            {
                return default(T);
            }
        }

        public static void SerializeObject(string filename, object obj)
        {
            //Stream fs = null;
            //XmlWriter writer = null;
            StreamWriter sw = null;
            string strSource = "";
            try
            {
                //fs = new FileStream(filename, FileMode.OpenOrCreate);
                //writer = new XmlTextWriter(fs, Encoding.UTF8);

                XmlSerializer s = new XmlSerializer(obj.GetType());
                Stream stream = new MemoryStream();
                s.Serialize(stream, obj);
                stream.Seek(0, SeekOrigin.Begin);
                sw = new StreamWriter(filename, false, Encoding.UTF8);

                using (StreamReader reader = new StreamReader(stream))
                {
                    // Just read to the end.
                    strSource = reader.ReadToEnd();
                    sw.Write(strSource);
                }


                //XmlSerializer serializer =
                //new XmlSerializer(obj.GetType());

                //serializer.Serialize(writer, obj);
                //writer.Close();
            }
            catch
            {

            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                }
                //if (writer != null)
                //{
                //    writer.Close();
                //}

                //if (fs != null)
                //{
                //    fs.Close();
                //}
            }
        }

        public static string SerializeObject(object obj)
        {
            //Stream fs = null;
            //XmlWriter writer = null;

            string strSource = "";
            try
            {
                //fs = new FileStream(filename, FileMode.OpenOrCreate);
                //writer = new XmlTextWriter(fs, Encoding.UTF8);

                XmlSerializer s = new XmlSerializer(obj.GetType());
                Stream stream = new MemoryStream();
                s.Serialize(stream, obj);
                stream.Seek(0, SeekOrigin.Begin);


                using (StreamReader reader = new StreamReader(stream))
                {
                    strSource = reader.ReadToEnd();
                }


            }
            catch (Exception ex)
            {
                }

            return strSource;
        }
    }
}
