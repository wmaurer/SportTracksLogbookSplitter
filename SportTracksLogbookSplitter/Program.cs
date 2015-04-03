namespace SportTracksLogbookSplitter
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Xml;

    internal class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 4) {
                Console.WriteLine("Usage: SportTracksLogbookSplitter logbookname date outputlogbook1 outputlogbook2");
                return;
            }
            var logbookName = args[0];
            var breakpoint = DateTime.ParseExact(args[1], "yyyy-MM-dd", CultureInfo.CurrentCulture);
            var outputLogbook1 = args[2];
            var outputLogbook2= args[3];

            var reachedEquipment = false;
            var firstFileReferenceIds = new List<string>();
            using (var xmlReader = XmlReader.Create(logbookName)) {
                using (var xmlWriter1 = XmlWriter.Create(outputLogbook1)) {
                    using (var xmlWriter2 = XmlWriter.Create(outputLogbook2)) {
                        while (xmlReader.Read()) {
                            if (xmlReader.Name == "Equipment" && xmlReader.NodeType == XmlNodeType.Element) {
                                reachedEquipment = true;
                            }
                            if (xmlReader.Name == "Activity" && xmlReader.NodeType == XmlNodeType.Element) {
                                if (!reachedEquipment) {
                                    var startTime = xmlReader.GetAttribute("startTime");
                                    var referenceId = xmlReader.GetAttribute("referenceId");
                                    XmlWriter currentXmlWriter;
                                    if (DateTime.Parse(startTime) < breakpoint) {
                                        currentXmlWriter = xmlWriter1;
                                        firstFileReferenceIds.Add(referenceId);
                                    }
                                    else {
                                        currentXmlWriter = xmlWriter2;
                                    }
                                    WriteShallowNode(xmlReader, currentXmlWriter);
                                    while (xmlReader.Read()) {
                                        WriteShallowNode(xmlReader, currentXmlWriter);
                                        if (xmlReader.Name == "Activity" && xmlReader.NodeType == XmlNodeType.EndElement) {
                                            break;
                                        }
                                    }
                                }
                                else {
                                    var referenceId = xmlReader.GetAttribute("referenceId");
                                    WriteShallowNode(xmlReader, firstFileReferenceIds.Contains(referenceId) ? xmlWriter1 : xmlWriter2);
                                }
                            }
                            else {
                                WriteShallowNode(xmlReader, xmlWriter1);
                                WriteShallowNode(xmlReader, xmlWriter2);
                            }
                        }
                    }
                }
            }
        }

        private static void WriteShallowNode(XmlReader reader, XmlWriter writer)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            if (writer == null) throw new ArgumentNullException("writer");

            switch (reader.NodeType) {
                case XmlNodeType.Element:
                    writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                    writer.WriteAttributes(reader, true);
                    if (reader.IsEmptyElement) {
                        writer.WriteEndElement();
                    }
                    break;
                case XmlNodeType.Text:
                    writer.WriteString(reader.Value);
                    break;
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    writer.WriteWhitespace(reader.Value);
                    break;
                case XmlNodeType.CDATA:
                    writer.WriteCData(reader.Value);
                    break;
                case XmlNodeType.EntityReference:
                    writer.WriteEntityRef(reader.Name);
                    break;
                case XmlNodeType.XmlDeclaration:
                case XmlNodeType.ProcessingInstruction:
                    writer.WriteProcessingInstruction(reader.Name, reader.Value);
                    break;
                case XmlNodeType.DocumentType:
                    writer.WriteDocType(reader.Name, reader.GetAttribute("PUBLIC"), reader.GetAttribute("SYSTEM"), reader.Value);
                    break;
                case XmlNodeType.Comment:
                    writer.WriteComment(reader.Value);
                    break;
                case XmlNodeType.EndElement:
                    writer.WriteFullEndElement();
                    break;
            }
        }
    }
}
