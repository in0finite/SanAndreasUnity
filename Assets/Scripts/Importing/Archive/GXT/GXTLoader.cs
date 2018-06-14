using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ralph.Crc32C;
using H = GXTHelpers;

// WIP: Inherit?

public class GXTSAIVEntry : IComparable
{
    public int offset;
    public int keyHash = 0;

    public int CompareTo(object obj)
    {
        if (obj == null) return 1;
        GXTSAIVEntry o = obj as GXTSAIVEntry;
        if (o != null)
            return offset.CompareTo(o.offset);
        throw new ArgumentException("Object is not a valid Entry!");
    }
}

public class GXTVC3Entry : IComparable
{
    public int offset;
    public char[] key = new char[8];

    public int CompareTo(object obj)
    {
        if (obj == null) return 1;
        GXTVC3Entry o = obj as GXTVC3Entry;
        if (o != null)
            return offset.CompareTo(o.offset);
        throw new ArgumentException("Object is not a valid Entry!");
    }
}

//C++ TO C# CONVERTER WARNING: The original C++ declaration of the following method implementation was not found:
//ORIGINAL LINE: GXTLoader::GXTLoader(istream* stream, Encoding encoding, bool autoClose) : stream(stream), autoClose(autoClose), encoding(encoding)

//C++ TO C# CONVERTER WARNING: The original C++ declaration of the following method implementation was not found:
//ORIGINAL LINE: GXTLoader::GXTLoader(const File& file, Encoding encoding) : stream(file.openInputStream(ifstream::binary)), autoClose(true), encoding(encoding)

//C++ TO C# CONVERTER WARNING: The original C++ declaration of the following method implementation was not found:
//ORIGINAL LINE: void GXTLoader::init()

//C++ TO C# CONVERTER WARNING: The original C++ declaration of the following method implementation was not found:
//ORIGINAL LINE: bool GXTLoader::nextTableHeader(GXTTableHeader& header)

//C++ TO C# CONVERTER WARNING: The original C++ declaration of the following method implementation was not found:
//ORIGINAL LINE: void GXTLoader::readTableHeaders(GXTTableHeader* headers)

//C++ TO C# CONVERTER WARNING: The original C++ declaration of the following method implementation was not found:
//ORIGINAL LINE: int GXTLoader::getTableOffset(const GXTTableHeader& header) const
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:

//C++ TO C# CONVERTER WARNING: The original C++ declaration of the following method implementation was not found:
//ORIGINAL LINE: GXTTable* GXTLoader::readTableData(const GXTTableHeader& header)

public class GXTTable
{
    /**	\brief An entry iterator.
	 */

    /**	\brief Creates an empty new GXTTable with the given internal encoding.
	 *
	 * 	@param internalEncoding The encoding in which the entry values are stored.
	 */

    //C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
    //	GXTTable(string name, Encoding internalEncoding, bool keepKeyNames = false);
    public GXTTable(string name, Encoding internalEncoding, bool keepKeyNames = false)
    {
        crc32 = new Crc32C();
    }

    ~GXTTable()
    {
        entries.Clear();
    }

    public char[] getValue(int keyHash)
    {
        char[] it = entries[keyHash];
        return it == entries.Last().Value ? null : it;
    }

    public string getValueUTF8(string key)
    {
        return getValueUTF8(crc32.ComputeString(key));
    }

    public string getValueUTF16(string key)
    {
        return getValueUTF16(crc32.ComputeString(key));
    }

    public char[] getValueUTF8(int keyHash)
    {
        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent for pointers to value types:
        //ORIGINAL LINE: sbyte* val = getValue(keyHash);
        char[] val = getValue(keyHash);
        int srcBytes;
        int destBytes;

        if (internalEncoding == Encoding.UTF16)
        {
            srcBytes = (val.Length + 1) * 2;
        }
        else
        {
            srcBytes = val.Length + 1;
        }

        char[] valCpy = new char[srcBytes];
        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
        Array.Copy(valCpy, val, srcBytes);

        destBytes = H.GetSufficientTranscodeBufferSize(srcBytes, internalEncoding, Encoding.UTF8);

        char[] dest = new char[destBytes];

        if (H.Transcode(ref valCpy, srcBytes, ref dest, destBytes, internalEncoding, Encoding.UTF8) < 0)
        {
            dest = null;
            valCpy = null;
            return null;
        }

        valCpy = null;
        return dest;
    }

    public char[] getValueUTF16(int keyHash)
    {
        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent for pointers to value types:
        //ORIGINAL LINE: sbyte* val = getValue(keyHash);
        char[] val = getValue(keyHash);
        int srcBytes;
        int destBytes;

        if (internalEncoding == Encoding.UTF16)
        {
            // If the internal format is UTF16 already, we'll just make a copy.
            srcBytes = (val.Length + 1) * 2;
        }
        else
        {
            srcBytes = val.Length + 1;
        }

        char[] valCpy = new char[srcBytes];
        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
        Array.Copy(valCpy, val, srcBytes);

        destBytes = H.GetSufficientTranscodeBufferSize(srcBytes, internalEncoding, Encoding.UTF16);

        char[] dest = new char[destBytes];

        if (H.Transcode(ref valCpy, srcBytes, ref dest, destBytes, internalEncoding, Encoding.UTF16) < 0)
        {
            dest = null;
            valCpy = null;
            return null;
        }

        valCpy = null;
        return dest;
    }

    public void setValue(int keyHash, char[] value)
    {
        entries[keyHash] = value;
    }

    public void setValue(string key, char[] value)
    {
        int keyHash = crc32.ComputeString(key);
        setValue(keyHash, value);

        if (keyNames[keyHash] != null)
        {
            keyNames[keyHash] = key;
        }
    }

    public string getKeyName(int keyHash)
    {
        if (keyNames[keyHash] != null)
        {
            return keyNames[keyHash];
        }

        return "";
    }

    public void setKeyName(int keyHash, string name)
    {
        if (keyNames[keyHash] != null)
        {
            keyNames[keyHash] = name;
        }
    }

    private string name;
    private Encoding internalEncoding;
    private Dictionary<int, char[]> entries = new Dictionary<int, char[]>();
    private Dictionary<int, string> keyNames;
    private Crc32C crc32;
}

public class GXTTableHeader
{
    public string name = new string(new char[8]);
    public int offset;
}

public enum Version
{
    SAIV,
    VC3
}

// https://github.com/alemariusnexus/gtatools/blob/master/src/libgtaformats/src/gtaformats/gxt/GXTLoader.cpp
// https://github.com/alemariusnexus/gtatools/blob/master/src/libgtaformats/src/gtaformats/gxt/GXTLoader.h
public partial class GXTLoader
{
    private StreamReader stream;
    private bool autoClose, keepKeyNames;
    private Encoding encoding;
    private Version version;
    private int numTables, currentTable, cpos;

    public GXTLoader(StreamReader stream, Encoding encoding, bool autoClose)
    {
        this.stream = stream;
        this.autoClose = autoClose;
        this.encoding = encoding;
        init();
    }

    public GXTLoader(string file, Encoding encoding)
    {
        this.stream = new StreamReader(File.Open(file, FileMode.Open));
        this.autoClose = true;
        this.encoding = encoding;
        init();
    }

    // Reference: http://gta.wikia.com/wiki/GXT
    public void init()
    {
        // I'm not very sure about this
        using (StreamReader reader = stream)
        {
            // This buffer skip contents
            char[] skipBuf = new char[4];
            char[] tov = new char[4];

            stream.Read(tov, cpos, 4);
            cpos = 4;

            if (string.Compare(tov.ToString(), 0, "TABL", 0, 4) == 0)
            {
                version = Version.VC3;
            }
            else
            {
                version = Version.SAIV;
                stream.Read(skipBuf, cpos, 4); // TABL
                cpos += 4;
            }

            numTables = reader.read32(cpos);

            if (numTables < 0)
            {
                string errmsg = string.Format("Invalid table header size: {0:D}", numTables);
                //C++ TO C# CONVERTER TODO TASK: There is no direct equivalent in C# to the following C++ macro:
                GXTException ex = new GXTException(errmsg);
                errmsg = null;
                throw ex;
            }
            if (numTables % 12 != 0)
            {
                string errmsg = string.Format("Invalid table header size (must be multiple of 12): {0:D}", numTables);
                //C++ TO C# CONVERTER TODO TASK: There is no direct equivalent in C# to the following C++ macro:
                GXTException ex = new GXTException(errmsg);
                errmsg = null;
                throw ex;
            }

            numTables /= 12;
            cpos += 4;

            currentTable = 0;
        }
    }

    public bool nextTableHeader(GXTTableHeader header)
    {
        if (currentTable >= numTables)
        {
            return false;
        }

        int headerOffset = currentTable * 12 + 8; // +8 = TABL and block size

        if (version == Version.SAIV)
        {
            headerOffset += 4; // GXT version
        }

        if (cpos != headerOffset)
        {
            stream.seekg(headerOffset - cpos, istream.cur);
        }

        stream.Read((string)header, 12);

        /*#if !GTAFORMATS_LITTLE_ENDIAN
                header.offset = FromLittleEndian32(header.offset);
        #endif*/

        //H.GetLittleEndianIntegerFromByteArray

        cpos += 12;
        currentTable++;

        if (header.offset < 0)
        {
            string errmsg = string.Format("Invalid table offset: {0:D}", header.offset);
            //C++ TO C# CONVERTER TODO TASK: There is no direct equivalent in C# to the following C++ macro:
            GXTException ex = new GXTException(errmsg);
            errmsg = null;
            throw ex;
        }

        return true;
    }

    public void readTableHeaders(GXTTableHeader[] headers)
    {
        int readTableCount = numTables - currentTable;
        for (int i = 0; i < readTableCount; i++)
        {
            nextTableHeader(headers[i]);
        }
    }

    public int getTableOffset(GXTTableHeader header)
    {
        int offset = header.offset;

        if (string.Compare(header.name, 0, "MAIN\0", 0, 5) != 0)
        {
            offset += 8; // The table name again
        }

        return offset;
    }

    public GXTTable readTableData(GXTTableHeader header)
    {
        using (StreamReader reader = stream)
        {
            char[] skipBuf = new char[8];
            int offset = getTableOffset(header);
            stream.seekg(offset - cpos, istream.cur);
            cpos = offset;

            stream.Read(skipBuf, cpos, 4); // TKEY
            cpos += 4;

            int tkeySize = reader.read32(cpos);
            cpos += 4;

            if (version == Version.SAIV)
            {
                int numEntries = tkeySize / 8;
                GXTTable table = new GXTTable(header.name, encoding == Encoding.None ? Encoding.GXT8 : encoding, keepKeyNames);
                GXTSAIVEntry[] entries = new GXTSAIVEntry[numEntries];

                // 2 * numEntries? Why?
                for (int i = 0; i < numEntries; ++i)
                {
                    entries[i].offset = reader.read32(cpos);
                    cpos += 8; // Why 8?
                }

                /*for (int32_t i = 0 ; i < numEntries ; i++) {
                    stream->read((char*) &entries[i], 8);
                }*/

                cpos += 8 * numEntries;

                stream.Read(skipBuf, cpos, 8); // "TDAT" + TDAT size
                cpos += 8;

                new QSort<GXTSAIVEntry>(entries).Sort();
                int tdatRead = 0;

                for (int i = 0; i < numEntries; i++)
                {
                    int skip = entries[i].offset - tdatRead; // Why? UInt
                    char[] tmpSkipBuf = new char[skip];
                    stream.Read(tmpSkipBuf, tdatRead, skip);
                    tmpSkipBuf = null;
                    tdatRead += skip;

                    int step = 64;
                    char[] text = null;
                    bool finished = false;
                    int strLen = 0;

                    for (int j = 0; !finished; j++)
                    {
                        char[] tmp = new char[(j + 1) * step];

                        if (text.Length > 0)
                        {
                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                            Array.Copy(tmp, text, j * step);
                            //text = null;
                        }

                        //text = tmp;

                        for (int k = 0; k < step; k++)
                        {
                            char[] buf = new char[1];
                            int idx = j * step + k;
                            stream.Read(buf, strLen, 1);
                            // text.Substring(idx) ??

                            strLen++;
                            if (text[idx] == '\0')
                            {
                                finished = true;
                                break;
                            }
                        }
                    }

                    if (encoding != Encoding.None && encoding != Encoding.GXT8)
                    {
                        char[] old = text;
                        int textSize = H.GetSufficientTranscodeBufferSize(strLen, Encoding.GXT8, encoding);
                        text = new char[textSize];
                        int res = H.Transcode(ref old, strLen, ref text, textSize, Encoding.GXT8, encoding);

                        if (res < 0)
                        {
                            string errmsg = string.Format("Error transcoding GXT string {0:D}: {1:D}\n", entries[i].keyHash, res);
                            //C++ TO C# CONVERTER TODO TASK: There is no direct equivalent in C# to the following C++ macro:
                            GXTException ex = new GXTException(errmsg);
                            errmsg = null;
                            throw ex;
                        }

                        old = null;
                    }

                    table.setValue(entries[i].keyHash, text);
                    tdatRead += strLen;
                }

                cpos += tdatRead;

                entries = null;

                return table;
            }
            else
            {
                int numEntries = tkeySize / 12;
                GXTTable table = new GXTTable(header.name, encoding == Encoding.None ? Encoding.GXT16 : encoding, keepKeyNames);
                GXTVC3Entry[] entries = Arrays.InitializeWithDefaultInstances<GXTVC3Entry>(numEntries);

                stream.Read((string)entries, 12 * numEntries);

                /*#if !GTAFORMATS_LITTLE_ENDIAN
                                for (int i = 0; i < numEntries; i++)
                                {
                                    entries[i].offset = SwapEndianness32(entries[i].offset);
                                }
                #endif*/

                /*for (int32_t i = 0 ; i < numEntries ; i++) {
                    stream->read((char*) &entries[i], 12);
                }*/

                cpos += 12 * numEntries;

                new QSort<GXTVC3Entry>(entries).Sort();

                stream.Read(skipBuf, cpos, 4); // TDAT
                cpos += 4;

                int tdatSize = reader.read32(cpos);
                cpos += 4;

                int tdatRead = 0;

                for (int i = 0; i < numEntries; i++)
                {
                    int skip = entries[i].offset - tdatRead;
                    char[] tmpSkipBuf = new char[skip];
                    stream.Read(tmpSkipBuf, tdatRead, skip);
                    tmpSkipBuf = null;
                    tdatRead += skip;

                    int maxLen;

                    if (i < numEntries - 1)
                    {
                        maxLen = entries[i + 1].offset - entries[i].offset;
                    }
                    else
                    {
                        maxLen = tdatSize - tdatRead;
                    }

                    char[] text = null;

                    text = new char[maxLen];
                    stream.Read(text, tdatRead, maxLen);
                    tdatRead += maxLen;

                    if (encoding != Encoding.None && encoding != Encoding.GXT16)
                    {
                        char[] old = text;
                        int textSize = H.GetSufficientTranscodeBufferSize(maxLen, Encoding.GXT16, encoding);
                        text = new char[textSize];
                        H.Transcode(ref old, maxLen, ref text, textSize, Encoding.GXT16, encoding);
                        old = null;
                    }

                    table.setValue(entries[i].key, text);
                }

                cpos += tdatRead;

                entries = null;

                return table;
            }
        }
    }

    /*public static int EntrySortComparator(object e1, object e2)
    {
        return ((GXTSAIVEntry)e1).offset - ((GXTSAIVEntry)e2).offset;
    }*/
}

//----------------------------------------------------------------------------------------
//	Copyright © 2006 - 2016 Tangible Software Solutions Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class provides the ability to initialize array elements with the default
//	constructions for the array type.
//----------------------------------------------------------------------------------------
internal static class Arrays
{
    internal static T[] InitializeWithDefaultInstances<T>(int length) where T : new()
    {
        T[] array = new T[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = new T();
        }
        return array;
    }
}

public class GXTException : Exception
{
    public GXTException(string message, Exception nestedException = null) : base(message, nestedException)
    {
    }
}