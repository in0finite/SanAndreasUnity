using HtmlCleaner;
using System.IO;
using System.Text;
using System.Xml;

public static class XmlExtensions
{
    public static string ToXml(this string html)
    {
        StringBuilder builder = new StringBuilder(html);
        using (StringWriter stringWriter = new StringWriter(builder))
        {
            using (HtmlWriter writer = new HtmlWriter(stringWriter))
            {
                // This produces UTF16 XML
                writer.Indentation = 4;
                writer.IndentChar = '\t';
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("Root");
                writer.WriteAttributeString("myattrib", "123");
                writer.WriteEndElement();
                writer.WriteEndDocument();

                return builder.ToString();
            }
        }
    }

    public static string ToUTF8Xml()
    {
        string result;
        MemoryStream stream = new MemoryStream(); // The writer closes this for us
        using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8))
        {
            writer.Indentation = 4;
            writer.IndentChar = '\t';
            writer.Formatting = Formatting.Indented;
            writer.WriteStartDocument();
            writer.WriteStartElement("Root");
            writer.WriteAttributeString("myattrib", "123");
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();

            // Make sure you flush or you only get half the text
            writer.Flush();

            // Use a StreamReader to get the byte order correct
            StreamReader reader = new StreamReader(stream, Encoding.UTF8, true);
            stream.Seek(0, SeekOrigin.Begin);
            result = reader.ReadToEnd();

            // #2 - doesn't write the byte order reliably
            //result = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
        }

        // Make sure you use File.WriteAllText("myfile", xml, Encoding.UTF8);
        // or equivalent to write your file back.
        return result;
    }
}