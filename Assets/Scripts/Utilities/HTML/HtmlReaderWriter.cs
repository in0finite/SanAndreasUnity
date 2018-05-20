// Copyright © 2005 by Omar Al Zabir. All rights are reserved.
//
// If you like this code then feel free to go ahead and use it.
// The only thing I ask is that you don't remove or alter my copyright notice.
//
// Your use of this software is entirely at your own risk. I make no claims or
// warrantees about the reliability or fitness of this code for any particular purpose.
// If you make changes or additions to this code please mark your code as being yours.
//
// website http://www.oazabir.com, email OmarAlZabir@gmail.com, msn oazabir@hotmail.com

using System;
using System.Globalization;
using System.IO;
using Sgml;
using System.Xml;
using System.Text;

namespace HtmlCleaner
{
    /// <summary>
    /// This class skips all nodes which has some kind of prefix. This trick does the job
    /// to clean up MS Word/Outlook HTML markups.
    /// </summary>
    public class HtmlReader : Sgml.SgmlReader
    {
        public HtmlReader(TextReader reader) : base()
        {
            base.InputStream = reader;
            base.DocType = "HTML";
        }

        public HtmlReader(string content) : base()
        {
            base.InputStream = new StringReader(content);
            base.DocType = "HTML";
        }

        public override bool Read()
        {
            bool status = base.Read();
            if (status)
            {
                if (base.NodeType == XmlNodeType.Element)
                {
                    // Got a node with prefix. This must be one of those "<o:p>" or something else.
                    // Skip this node entirely. We want prefix less nodes so that the resultant XML
                    // requires not namespace.
                    if (base.Name.IndexOf(':') > 0)
                        base.Skip();
                }
            }
            return status;
        }
    }

    /// <summary>
    /// Extends XmlTextWriter to provide Html writing feature which is not as strict as Xml
    /// writing. For example, Xml Writer encodes content passed to WriteString which encodes special markups like
    /// &nbsp to &amp;bsp. So, WriteString is bypassed by calling WriteRaw.
    /// </summary>
    public class HtmlWriter : XmlTextWriter
    {
        //public StringBuilder builder;

        /// <summary>
        /// If set to true, it will filter the output by using tag and attribute filtering,
        /// space reduce etc
        /// </summary>
        public bool FilterOutput = false;

        /// <summary>
        /// If true, it will reduce consecutive &nbsp; with one instance
        /// </summary>
        public bool ReduceConsecutiveSpace = true;

        /// <summary>
        /// Set the tag names in lower case which are allowed to go to output
        /// </summary>
        public string[] AllowedTags = new string[] { "p", "b", "i", "u", "em", "big", "small",
            "div", "img", "span", "blockquote", "code", "pre", "br", "hr",
            "ul", "ol", "li", "del", "ins", "strong", "a", "font", "dd", "dt"};

        /// <summary>
        /// If any tag found which is not allowed, it is replaced by this tag.
        /// Specify a tag which has least impact on output
        /// </summary>
        public string ReplacementTag = "dd";

        /// <summary>
        /// New lines \r\n are replaced with space which saves space and makes the
        /// output compact
        /// </summary>
        public bool RemoveNewlines = true;

        /// <summary>
        /// Specify which attributes are allowed. Any other attribute will be discarded
        /// </summary>
        public string[] AllowedAttributes = new string[] { "class", "href", "target",
            "border", "src", "align", "width", "height", "color", "size" };

        public HtmlWriter(TextWriter writer) : base(writer)
        {
        }

        public HtmlWriter(StringBuilder builder) : base(new StringWriter(builder))
        {
            //this.builder = builder;
        }

        public HtmlWriter(Stream stream, Encoding enc) : base(stream, enc)
        {
        }

        /// <summary>
        /// The reason why we are overriding this method is, we do not want the output to be
        /// encoded for texts inside attribute and inside node elements. For example, all the &nbsp;
        /// gets converted to &amp;nbsp in output. But this does not
        /// apply to HTML. In HTML, we need to have &nbsp; as it is.
        /// </summary>
        /// <param name="text"></param>
        public override void WriteString(string text)
        {
            // Change all non-breaking space to normal space
            text = text.Replace(" ", "&nbsp;");
            /// When you are reading RSS feed and writing Html, this line helps remove
            /// those CDATA tags
            text = text.Replace("<![CDATA[", "");
            text = text.Replace("]]>", "");

            // Do some encoding of our own because we are going to use WriteRaw which won't
            // do any of the necessary encoding
            text = text.Replace("<", "&lt;");
            text = text.Replace(">", "&gt;");
            text = text.Replace("'", "&apos;");
            text = text.Replace("\"", "&quote;");

            if (this.FilterOutput)
            {
                text = text.Trim();

                // We want to replace consecutive spaces to one space in order to save horizontal
                // width
                if (this.ReduceConsecutiveSpace) text = text.Replace("&nbsp;&nbsp;&nbsp;", "&nbsp;");

                if (this.RemoveNewlines) text = text.Replace(Environment.NewLine, " ");

                base.WriteRaw(text);
            }
            else
            {
                base.WriteRaw(text);
            }
        }

        public override void WriteWhitespace(string ws)
        {
            if (!this.FilterOutput) base.WriteWhitespace(ws);
        }

        public override void WriteComment(string text)
        {
            if (!this.FilterOutput) base.WriteComment(text);
        }

        /// <summary>
        /// This method is overriden to filter out tags which are not allowed
        /// </summary>
        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            if (this.FilterOutput)
            {
                bool canWrite = false;
                string tagLocalName = localName.ToLower();
                foreach (string name in this.AllowedTags)
                {
                    if (name == tagLocalName)
                    {
                        canWrite = true;
                        break;
                    }
                }

                if (!canWrite)
                    localName = "dd";
            }

            base.WriteStartElement(prefix, localName, ns);
        }

        /// <summary>
        /// This method is overriden to filter out attributes which are not allowed
        /// </summary>
        public override void WriteAttributes(XmlReader reader, bool defattr)
        {
            if (this.FilterOutput)
            {
                // The following code is copied from implementation of XmlWriter's
                // WriteAttributes method.
                if (reader == null)
                {
                    throw new ArgumentNullException("reader");
                }
                if ((reader.NodeType == XmlNodeType.Element) || (reader.NodeType == XmlNodeType.XmlDeclaration))
                {
                    if (reader.MoveToFirstAttribute())
                    {
                        this.WriteAttributes(reader, defattr);
                        reader.MoveToElement();
                    }
                }
                else
                {
                    if (reader.NodeType != XmlNodeType.Attribute)
                    {
                        throw new XmlException("Xml_InvalidPosition");
                    }
                    do
                    {
                        if (defattr || !reader.IsDefault)
                        {
                            // Check if the attribute is allowed
                            bool canWrite = false;
                            string attributeLocalName = reader.LocalName.ToLower();
                            foreach (string name in this.AllowedAttributes)
                            {
                                if (name == attributeLocalName)
                                {
                                    canWrite = true;
                                    break;
                                }
                            }

                            // If allowed, write the attribute
                            if (canWrite)
                                this.WriteStartAttribute(reader.Prefix, attributeLocalName,
                                    reader.NamespaceURI);

                            while (reader.ReadAttributeValue())
                            {
                                if (reader.NodeType == XmlNodeType.EntityReference)
                                {
                                    if (canWrite) this.WriteEntityRef(reader.Name);
                                    continue;
                                }
                                if (canWrite) this.WriteString(reader.Value);
                            }
                            if (canWrite) this.WriteEndAttribute();
                        }
                    } while (reader.MoveToNextAttribute());
                }
            }
            else
            {
                base.WriteAttributes(reader, defattr);
            }
        }
    }
}