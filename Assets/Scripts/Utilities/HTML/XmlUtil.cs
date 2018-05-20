using System;
using System.Text;
using System.Xml;

public static class XmlUtil
{
    public static string SanitizeXML(string textIn)
    {
        StringBuilder textOut = new StringBuilder(); // Used to hold the output.
        char current; // Used to reference the current character.

        if (textIn == null || textIn == string.Empty) return string.Empty; // vacancy test.
        for (int i = 0; i < textIn.Length; i++)
        {
            current = textIn[i];

            if ((current == 0x9 || current == 0xA || current == 0xD) ||
                ((current >= 0x20) && (current <= 0xD7FF)) ||
                ((current >= 0xE000) && (current <= 0xFFFD)) ||
                ((current >= 0x10000) && (current <= 0x10FFFF)))
            {
                textOut.Append(current);
            }
        }
        return textOut.ToString().Replace("><", ">\n<");
    }

    public static string XmlBeautifier(this string xmlText, string indentChars = "", bool indentOnAttributes = false, Encoding enc = null)
    {
        if (string.IsNullOrEmpty(xmlText) || string.IsNullOrEmpty(xmlText))
            throw new ArgumentNullException(paramName: "xmlText");
        else
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(SanitizeXML(xmlText));
            return XmlBeautifier(xmlDoc, indentChars, indentOnAttributes, enc);
        }
    }

    public static string XmlBeautifier(XmlDocument xmlDoc, string indentChars = "", bool indentOnAttributes = false, Encoding enc = null)
    {
        global::System.Text.StringBuilder sb = new global::System.Text.StringBuilder();
        XmlWriterSettings settings = new XmlWriterSettings()
        {
            Indent = true,
            CheckCharacters = true,
            OmitXmlDeclaration = false,
            ConformanceLevel = ConformanceLevel.Auto,
            NewLineHandling = NewLineHandling.Replace,
            NewLineChars = Environment.NewLine,
            NewLineOnAttributes = indentOnAttributes,
            IndentChars = !string.IsNullOrEmpty(indentChars) ? indentChars : Convert.ToChar(9).ToString(),
            Encoding = enc != null ? enc : Encoding.Default,
            CloseOutput = true
        };

        using (XmlWriter writer = XmlWriter.Create(sb, settings))
        {
            xmlDoc.WriteContentTo(writer);
            writer.Flush();
        }

        return sb.ToString();
    }
}