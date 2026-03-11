using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Utility
{
    public class Enc_Dec
    {

        private static object connLock = new object();

        private static bool? _useSqlInstance = null;
        private static Regex _guidRegex = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);

        static string _key = "ABCDEFFEDCBAABCDEFFEDCBAABCDEFFEDCBAABCDEFFEDCBA";
        static string _vector = "ABCDEFFEDCBABCDE";

        #region constructor


        #endregion

        #region Utility Methods

        /// <summary>
        /// Helper function used to determine if we are configured to use SQL Server Named instances.
        /// </summary>
        /// <returns></returns>


        /// <summary>
        /// This method is a helper method to simply check a string array for a given string.
        /// </summary>
        /// <param name="valueToCheck">The string value you are checking for.</param>
        /// <param name="arrayToCheck">The string array to check within.</param>
        /// <returns>The index of the value, if found, otherwise -1 if the value does not exist.</returns>
        public static int InArray(string valueToCheck, string[] arrayToCheck)
        {

            for (int i = 0; i < arrayToCheck.Length; i++)
            {
                if (arrayToCheck[i] == valueToCheck) return i;
            }

            return -1;
        }


        /// <summary>
        /// This method checks a given bit against a bitmask and returns
        /// true/false depending on if it's turned on.
        /// </summary>
        /// <param name="bitToCheck">The bit to check for.</param>
        /// <param name="bitMask">The bitmask to check the bit against.</param>
        /// <returns>True/False depending on if the bit is on or not.</returns>
        public static bool CheckBitMask(long bitToCheck, long bitMask)
        {
            return ((bitMask & bitToCheck) == bitToCheck);
        }


        /// <summary>
        /// Method to return a response from a requested Server by URL.
        /// </summary>
        /// <remarks>This handles the old GetFileServer and XHLHTTP requests.</remarks>
        /// <param name="serverUrl">the URL of the requested server</param>
        /// <returns>string of the response</returns>
        public static string GetServerResponse(string serverUrl)
        {
            try
            {
                HttpWebRequest webRequester;
                Stream requestStream;
                StreamReader streamReader;


                // Do the web request.
                webRequester = (HttpWebRequest)WebRequest.Create(serverUrl);
                webRequester.UserAgent = "Mozilla/5.0+(Windows;+U;+Windows+NT+5.0;+en-US;+rv:1.0.1)+Gecko/20020823+Netscape/7.0";

                // Grab the response stream.
                requestStream = webRequester.GetResponse().GetResponseStream();

                // Throw it into a reader so we can itterate through it.
                streamReader = new StreamReader(requestStream);


                // Return the first line.
                return streamReader.ReadToEnd();
            }
            catch (Exception e)
            {
                //trap exception to the database
                //Debugger.Trap(0,
                //    ref e,
                //    "URL Requested: " + serverUrl,
                //    "",
                //    "");
                return e.Message;
            }

        }


        public static string GetServerResponse(string serverUrl, Hashtable keyValuePairs, bool isFormPost, string userName, string userPwd, string proxyServer, int proxyPort)
        {
            try
            {
                HttpWebRequest webRequester;
                Stream requestStream;
                StreamReader streamReader;

                webRequester = (HttpWebRequest)WebRequest.Create(serverUrl);
                webRequester.UserAgent = "Mozilla/5.0+(Windows;+U;+Windows+NT+5.0;+en-US;+rv:1.0.1)+Gecko/20020823+Netscape/7.0";
                webRequester.Timeout = 15000; // milliseconds

                // form post
                if (isFormPost)
                {
                    webRequester.ContentType = "application/x-www-form-urlencoded";
                    // webRequester.Headers["Content-Disposition"] = "form-data";
                    // form data
                    if ((keyValuePairs != null) && (keyValuePairs.Count > 0))
                    {
                        StringBuilder sb = new StringBuilder();
                        bool isFirst = true;
                        foreach (object key in keyValuePairs.Keys)
                        {
                            if (!isFirst)
                            {
                                sb.Append("&");
                            }
                            sb.Append(key.ToString());
                            sb.Append("=");
                            sb.Append(keyValuePairs[key].ToString());
                            isFirst = false;
                        }
                        webRequester.Method = "POST";
                        ASCIIEncoding ascii = new ASCIIEncoding();
                        byte[] postBuffer = ascii.GetBytes(sb.ToString());
                        webRequester.ContentLength = postBuffer.Length;
                        Stream postData = webRequester.GetRequestStream();
                        postData.Write(postBuffer, 0, postBuffer.Length);
                        postData.Close();
                    }
                }

                requestStream = webRequester.GetResponse().GetResponseStream();
                streamReader = new StreamReader(requestStream);
                return streamReader.ReadToEnd();
            }
            catch
            {
                //Debugger.Trap(0,ref e, "GetServerResponse with form post","",""); 
            }

            return "ERROR";
        }


        public static byte[] Get1x1Pixel()
        {
            ArrayList arrayList = new ArrayList();
            string stringClearPixel = "4749463839610100010080FF00C6353500000021F90401000000002C00000000010001000002024401003B";
            int stoppingPoint = (stringClearPixel.Length / 2);

            for (int index = 0; index < stoppingPoint; index++)
            {
                arrayList.Add(byte.Parse(stringClearPixel.Substring((index * 2), 2),
                    System.Globalization.NumberStyles.HexNumber));
            }

            byte[] byteArray = (byte[])arrayList.ToArray(typeof(byte));

            return byteArray;
        }





        public static double DateDiff(string howtocompare, DateTime startDate, DateTime endDate)
        {
            double diff = 0;
            try
            {
                System.TimeSpan TS = startDate.Subtract(endDate);

                switch (howtocompare.ToLower())
                {
                    //Added for accountsettings/verifypwd.aspx.cs---> soumya 04/29/03
                    //***************************************************************
                    case "m":
                        diff = Convert.ToDouble(TS.TotalMinutes);
                        break;
                    //*********************************************
                    case "h":
                        diff = Convert.ToDouble(TS.TotalHours);
                        break;
                    case "yyyy":
                        diff = Convert.ToDouble(startDate.Year - endDate.Year);
                        break;
                    default:
                        //d -- days
                        diff = Convert.ToDouble(TS.TotalDays);
                        break;
                }
            }
            catch
            {
                //trap exception to the database
                //Debugger.Trap(0,
                //    ref e,
                //    "DateDiff(" + howtocompare + "," + startDate.ToShortDateString() + "," + endDate.ToShortDateString() + ")",
                //    "",
                //    "");
                //diff = -1; 
            }
            return diff;
        }




        public static string EncryptString(string stringToEncrypt)
        {
            if (string.IsNullOrEmpty(stringToEncrypt))
            {
                return "";
            }

            TripleDESCryptoServiceProvider _cryptoProvider = new TripleDESCryptoServiceProvider();
            try
            {
                _cryptoProvider.Key = HexToByte(_key);
                _cryptoProvider.IV = HexToByte(_vector);


                byte[] valBytes = Encoding.Unicode.GetBytes(stringToEncrypt);
                ICryptoTransform transform = _cryptoProvider.CreateEncryptor();
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, transform, CryptoStreamMode.Write);
                cs.Write(valBytes, 0, valBytes.Length);
                cs.FlushFinalBlock();
                byte[] returnBytes = ms.ToArray();
                cs.Close();
                return Convert.ToBase64String(returnBytes);
            }
            catch
            {
                return "";
            }
        }

        public string DecryptString(string stringToDecrypt)
        {
            if (string.IsNullOrEmpty(stringToDecrypt))
            {
                return "";
            }

            TripleDESCryptoServiceProvider _cryptoProvider = new TripleDESCryptoServiceProvider();

            try
            {
                _cryptoProvider.Key = HexToByte(_key);
                _cryptoProvider.IV = HexToByte(_vector);

                byte[] valBytes = Convert.FromBase64String(stringToDecrypt);
                ICryptoTransform transform = _cryptoProvider.CreateDecryptor();
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, transform, CryptoStreamMode.Write);
                cs.Write(valBytes, 0, valBytes.Length);
                cs.FlushFinalBlock();
                byte[] returnBytes = ms.ToArray();
                cs.Close();
                return Encoding.Unicode.GetString(returnBytes);
            }
            catch
            {
                return "";
            }
        }


        /// <summary>
        /// Converts a hexadecimal string to a byte array
        /// </summary>
        /// <param name="hexString">hex value</param>
        /// <returns>byte array</returns>
        private static byte[] HexToByte(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] =
                Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        /// <summary>
        /// Removes Hex Encoded and double encoded escape characters from a string.  
        /// </summary>
        /// <param name="fullText">string that may or may not contain encoded characters</param>
        /// <returns>The same string that was passed in, but with out the encoded characters</returns>
        public static string StripEscapeCharacters(string fullText)
        {
            try
            {
                fullText = System.Text.RegularExpressions.Regex.Replace(fullText, @"\%[0-9A-Za-z][0-9A-Za-z]", "");
                //trim the string to remove the space from the removal of tags
                return fullText.Trim();
            }
            catch
            {
                return fullText;
            }
        }

        /// <summary>
        /// Removes HTML tags from a string.  Removes only the tags, but for script, a, and object; the tags and
        /// the contents within the tags are also removed
        /// </summary>
        /// <param name="fullText">string that may or may not contain HTML tags</param>
        /// <returns>The same string that was passed in, but with out the HTML tags</returns>
        public static string StripHtml(string fullText)
        {
            try
            {
                //stuff text into stringbuilder to replace escape characters with open and close tags
                StringBuilder sbFullText = new StringBuilder();
                sbFullText.Append(fullText);
                sbFullText.Replace("&lt;", "<");
                sbFullText.Replace("&gt;", ">");

                //remove <script tags and everything in between the open and close
                string strNoMoreTags = System.Text.RegularExpressions.Regex.Replace(sbFullText.ToString(), @"<script*.*/script>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                //remove <a tags and everything in between the open and close
                strNoMoreTags = System.Text.RegularExpressions.Regex.Replace(strNoMoreTags, @"<a*.*/a>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                //remove <object tags and everything in between the open and close
                strNoMoreTags = System.Text.RegularExpressions.Regex.Replace(strNoMoreTags, @"<object*.*/object>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                //regular expression to remove all things left in the string starting with < and ending with >
                strNoMoreTags = System.Text.RegularExpressions.Regex.Replace(strNoMoreTags, @"<(.|\n)+?>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                //trim the string to retrieve the space from the removal of tags
                return strNoMoreTags.Trim();
            }
            catch
            {
                return fullText;
            }
        }
        public static void GetFeetInchesFromCMS(decimal heightCMS, ref int heightFT, ref int heightIN)
        {
            double _newheight = (Convert.ToDouble(heightCMS) / (2.54 * 12));
            string _strNewHeight = Convert.ToString(_newheight);
            int DotPos = _strNewHeight.IndexOf(".");
            if (DotPos > 0)
            {
                heightFT = Convert.ToInt32(_strNewHeight.Substring(0, DotPos));
                heightIN = Convert.ToInt32(Math.Round((_newheight - heightFT) * 12));
            }
            else
            {
                heightFT = Convert.ToInt32(_newheight);
                heightIN = 0;
            }

            if (heightIN.Equals(12))
            {
                heightFT = heightFT + 1;
                heightIN = 0;
            }
        }

        public static decimal GetCMsFromFeetInches(int feet, int inches)
        {
            return decimal.Parse((((feet * 12) + inches) * 2.54).ToString());
        }

        /// <summary>
        /// Returns the 100 x 100 directory to store a file based on the last 4 digits of the userID
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="pathDelim"></param>
        /// <returns></returns>
        public static string GetImageDirectoryLocation(int userID, string pathDelim)
        {

            string sUserID = userID.ToString();
            string dir1 = string.Empty;
            string dir2 = string.Empty;

            string temp = Right(sUserID, 4);
            dir2 = Right(temp, 2);
            dir1 = Left(temp, 2);
            return pathDelim + dir1 + pathDelim + dir2;
        }

        public static string Right(string original, int numberCharacters)
        {
            return original.Substring(numberCharacters > original.Length ? 0 : original.Length - numberCharacters);
        }

        public static string Left(string original, int numberCharacters)
        {
            return original.Substring(0, numberCharacters);
        }


        #endregion

        #region LiveID Utility Methods





        #endregion

        #region ASCII Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        public static string Encode_ASCII(string strData)
        {
            /*
             ASCII
            */

            /*                 zero width space               space */
            strData.Replace((char)0x200B, (char)0x0020);

            /*                 ideographic space              space */
            strData.Replace((char)0x3000, (char)0x0020);

            /*                 zero width no-break space      space */
            strData.Replace((char)0xFEFF, (char)0x0020);

            /*                 latin letter retroflex click   exclamation mark */
            strData.Replace((char)0x01C3, (char)0x0021);

            /*                 double exclamation mark        exclamation mark */
            strData.Replace((char)0x203C, (char)0x0021);

            /*                 interrobang                    exclamation mark */
            strData.Replace((char)0x203D, (char)0x0021);

            /*                 heavy exclamation mark ornament exclamation mark */
            strData.Replace((char)0x2762, (char)0x0021);

            /*                 modifier letter double prime   quotation mark */
            strData.Replace((char)0x02BA, (char)0x0022);

            /*!!!              open quotation mark			  quotation mark */
            strData.Replace((char)0x201C, (char)0x0022);

            /*!!!              close quotation mark			  quotation mark */
            strData.Replace((char)0x201D, (char)0x0022);

            /*                 combining double acute accent  quotation mark */
            strData.Replace((char)0x030B, (char)0x0022);

            /*                 combining double vertical line above quotation mark */
            strData.Replace((char)0x030E, (char)0x0022);

            /*                 double prime                   quotation mark */
            strData.Replace((char)0x2033, (char)0x0022);

            /*                 ditto mark                     quotation mark */
            strData.Replace((char)0x3003, (char)0x0022);

            /*                 arabic percent sign            percent sign */
            strData.Replace((char)0x066A, (char)0x0025);

            /*                 per mille sign                 percent sign */
            strData.Replace((char)0x2030, (char)0x0025);

            /*                 per ten thousand sign          percent sign */
            strData.Replace((char)0x2031, (char)0x0025);

            /*!!!              open apostrophe                apostrophe */
            strData.Replace((char)0x2018, (char)0x0027);

            /*!!!              close apostrophe               apostrophe */
            strData.Replace((char)0x2019, (char)0x0027);

            /*                 modifier letter prime          apostrophe */
            strData.Replace((char)0x02B9, (char)0x0027);

            /*                 modifier letter apostrophe     apostrophe */
            strData.Replace((char)0x02BC, (char)0x0027);

            /*                 modifier letter vertical line  apostrophe */
            strData.Replace((char)0x02C8, (char)0x0027);

            /*                 combining acute accent         apostrophe */
            strData.Replace((char)0x0301, (char)0x0027);

            /*                 prime                          apostrophe */
            strData.Replace((char)0x2032, (char)0x0027);

            /*                 arabic five pointed star       asterisk */
            strData.Replace((char)0x066D, (char)0x002A);

            /*                 asterisk operator              asterisk */
            strData.Replace((char)0x2217, (char)0x002A);

            /*                 heavy asterisk                 asterisk */
            strData.Replace((char)0x2731, (char)0x002A);

            /*                 arabic comma                   comma */
            strData.Replace((char)0x060C, (char)0x002C);

            /*                 single low-9 quotation mark    comma */
            strData.Replace((char)0x201A, (char)0x002C);

            /*                 ideographic comma              comma */
            strData.Replace((char)0x3001, (char)0x002C);

            /*                 hyphen                         hyphen-minus */
            strData.Replace((char)0x2010, (char)0x002D);

            /*                 non-breaking hyphen            hyphen-minus */
            strData.Replace((char)0x2011, (char)0x002D);

            /*                 figure dash                    hyphen-minus */
            strData.Replace((char)0x2012, (char)0x002D);

            /*                 en dash                        hyphen-minus */
            strData.Replace((char)0x2013, (char)0x002D);

            /*                 minus sign                     hyphen-minus */
            strData.Replace((char)0x2212, (char)0x002D);

            /*                 arabic full stop               full stop */
            strData.Replace((char)0x06D4, (char)0x002E);

            /*                 ideographic full stop          full stop */
            strData.Replace((char)0x3002, (char)0x002E);

            /*                 latin letter dental click      solidus */
            strData.Replace((char)0x01C0, (char)0x002F);

            /*                 combining long solidus overlay solidus */
            strData.Replace((char)0x0338, (char)0x002F);

            /*                 fraction slash                 solidus */
            strData.Replace((char)0x2044, (char)0x002F);

            /*                 division slash                 solidus */
            strData.Replace((char)0x2215, (char)0x002F);

            /*                 armenian full stop             colon */
            strData.Replace((char)0x0589, (char)0x003A);

            /*                 ratio                          colon */
            strData.Replace((char)0x2236, (char)0x003A);

            /*                 greek question mark            semicolon */
            strData.Replace((char)0x037E, (char)0x003B);

            /*                 arabic semicolon               semicolon */
            strData.Replace((char)0x061B, (char)0x003B);

            /*                 single left-pointing angle quotation mark less-than sign */
            strData.Replace((char)0x2039, (char)0x003C);

            /*                 left-pointing angle bracket    less-than sign */
            strData.Replace((char)0x2329, (char)0x003C);

            /*                 left angle bracket             less-than sign */
            strData.Replace((char)0x3008, (char)0x003C);

            /*                 not equal to                   equals sign */
            strData.Replace((char)0x2260, (char)0x003D);

            /*                 identical to                   equals sign */
            strData.Replace((char)0x2261, (char)0x003D);

            /*                 single right-pointing angle quotation mark greater-than sign */
            strData.Replace((char)0x203A, (char)0x003E);

            /*                 right-pointing angle bracket   greater-than sign */
            strData.Replace((char)0x232A, (char)0x003E);

            /*                 right angle bracket            greater-than sign */
            strData.Replace((char)0x3009, (char)0x003E);

            /*                 greek question mark            question mark */
            strData.Replace((char)0x037E, (char)0x003F);

            /*                 arabic question mark           question mark */
            strData.Replace((char)0x061F, (char)0x003F);

            /*                 interrobang                    question mark */
            strData.Replace((char)0x203D, (char)0x003F);

            /*                 question exclamation mark      question mark */
            strData.Replace((char)0x2048, (char)0x003F);

            /*                 exclamation question mark      question mark */
            strData.Replace((char)0x2049, (char)0x003F);

            /*                 script capital b               latin capital letter b */
            strData.Replace((char)0x212C, (char)0x0042);

            /*                 double-struck capital c        latin capital letter c */
            strData.Replace((char)0x2102, (char)0x0043);

            /*                 black-letter capital c         latin capital letter c */
            strData.Replace((char)0x212D, (char)0x0043);

            /*                 euler constant                 latin capital letter e */
            strData.Replace((char)0x2107, (char)0x0045);

            /*                 script capital e               latin capital letter e */
            strData.Replace((char)0x2130, (char)0x0045);

            /*                 script capital f               latin capital letter f */
            strData.Replace((char)0x2131, (char)0x0046);

            /*                 turned capital f               latin capital letter f */
            strData.Replace((char)0x2132, (char)0x0046);

            /*                 script capital h               latin capital letter h */
            strData.Replace((char)0x210B, (char)0x0048);

            /*                 black-letter capital h         latin capital letter h */
            strData.Replace((char)0x210C, (char)0x0048);

            /*                 double-struck capital h        latin capital letter h */
            strData.Replace((char)0x210D, (char)0x0048);

            /*                 latin capital letter i with dot above latin capital letter i */
            strData.Replace((char)0x0130, (char)0x0049);

            /*                 cyrillic capital letter byelorussian-ukrainian i latin capital letter i */
            strData.Replace((char)0x0406, (char)0x0049);

            /*                 cyrillic letter palochka       latin capital letter i */
            strData.Replace((char)0x04C0, (char)0x0049);

            /*                 script capital i               latin capital letter i */
            strData.Replace((char)0x2110, (char)0x0049);

            /*                 black-letter capital i         latin capital letter i */
            strData.Replace((char)0x2111, (char)0x0049);

            /*                 roman numeral one              latin capital letter i */
            strData.Replace((char)0x2160, (char)0x0049);

            /*                 kelvin sign                    latin capital letter k */
            strData.Replace((char)0x212A, (char)0x004B);

            /*                 script capital l               latin capital letter l */
            strData.Replace((char)0x2112, (char)0x004C);

            /*                 script capital m               latin capital letter m */
            strData.Replace((char)0x2133, (char)0x004D);

            /*                 double-struck capital n        latin capital letter n */
            strData.Replace((char)0x2115, (char)0x004E);

            /*                 double-struck capital p        latin capital letter p */
            strData.Replace((char)0x2119, (char)0x0050);

            /*                 double-struck capital q        latin capital letter q */
            strData.Replace((char)0x211A, (char)0x0051);

            /*                 script capital r               latin capital letter r */
            strData.Replace((char)0x211B, (char)0x0052);

            /*                 black-letter capital r         latin capital letter r */
            strData.Replace((char)0x211C, (char)0x0052);

            /*                 double-struck capital r        latin capital letter r */
            strData.Replace((char)0x211D, (char)0x0052);

            /*                 double-struck capital z        latin capital letter z */
            strData.Replace((char)0x2124, (char)0x005A);

            /*                 black-letter capital z         latin capital letter z */
            strData.Replace((char)0x2128, (char)0x005A);

            /*                 set minus                      reverse solidus */
            strData.Replace((char)0x2216, (char)0x005C);

            /*                 modifier letter up arrowhead   circumflex accent */
            strData.Replace((char)0x02C4, (char)0x005E);

            /*                 modifier letter circumflex accent circumflex accent */
            strData.Replace((char)0x02C6, (char)0x005E);

            /*                 combining circumflex accent    circumflex accent */
            strData.Replace((char)0x0302, (char)0x005E);

            /*                 up arrowhead                   circumflex accent */
            strData.Replace((char)0x2303, (char)0x005E);

            /*                 modifier letter low macron     low line */
            strData.Replace((char)0x02CD, (char)0x005F);

            /*                 combining macron below         low line */
            strData.Replace((char)0x0331, (char)0x005F);

            /*                 combining low line             low line */
            strData.Replace((char)0x0332, (char)0x005F);

            /*                 double low line                low line */
            strData.Replace((char)0x2017, (char)0x005F);

            /*                 modifier letter grave accent   grave accent */
            strData.Replace((char)0x02CB, (char)0x0060);

            /*                 combining grave accent         grave accent */
            strData.Replace((char)0x0300, (char)0x0060);

            /*                 reversed prime                 grave accent */
            strData.Replace((char)0x2035, (char)0x0060);

            /*                 estimated symbol               latin small letter e */
            strData.Replace((char)0x212E, (char)0x0065);

            /*                 script small e                 latin small letter e */
            strData.Replace((char)0x212F, (char)0x0065);

            /*                 latin small letter script g    latin small letter g */
            strData.Replace((char)0x0261, (char)0x0067);

            /*                 script small g                 latin small letter g */
            strData.Replace((char)0x210A, (char)0x0067);

            /*                 cyrillic small letter shha     latin small letter h */
            strData.Replace((char)0x04BB, (char)0x0068);

            /*                 planck constant                latin small letter h */
            strData.Replace((char)0x210E, (char)0x0068);

            /*                 latin small letter dotless i   latin small letter i */
            strData.Replace((char)0x0131, (char)0x0069);

            /*                 script small l                 latin small letter l */
            strData.Replace((char)0x2113, (char)0x006C);

            /*                 superscript latin small letter n latin small letter n */
            strData.Replace((char)0x207F, (char)0x006E);

            /*                 script small o                 latin small letter o */
            strData.Replace((char)0x2134, (char)0x006F);

            /*                 latin small letter z with stroke latin small letter z */
            strData.Replace((char)0x01B6, (char)0x007A);

            /*                 latin letter dental click      vertical line */
            strData.Replace((char)0x01C0, (char)0x007C);

            /*                 divides                        vertical line */
            strData.Replace((char)0x2223, (char)0x007C);

            /*                 light vertical bar             vertical line */
            strData.Replace((char)0x2758, (char)0x007C);

            /*                 small tilde                    tilde */
            strData.Replace((char)0x02DC, (char)0x007E);

            /*                 combining tilde                tilde */
            strData.Replace((char)0x0303, (char)0x007E);

            /*                 tilde operator                 tilde */
            strData.Replace((char)0x223C, (char)0x007E);

            /*                 fullwidth tilde                tilde */
            strData.Replace((char)0xFF5E, (char)0x007E);


            return strData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        public static string Encode_ISO_8859_1(string strData)
        {


            /*
             ISO 8859-1 (aka Latin-1)
            */

            /*                 figure space                   no-break space */
            strData.Replace((char)0x2007, (char)0x00A0);

            /*                 narrow no-break space          no-break space */
            strData.Replace((char)0x202F, (char)0x00A0);

            /*                 zero width no-break space      no-break space */
            strData.Replace((char)0xFEFF, (char)0x00A0);

            /*                 lira sign                      pound sign */
            strData.Replace((char)0x20A4, (char)0x00A3);

            /*!!!              combining diaeresis \w space   diaeresis */
            strData.Replace("\x0020\x0308", "\x00A8");

            /*                 combining diaeresis            diaeresis */
            strData.Replace((char)0x0308, (char)0x00A8);

            /*                 sound recording copyright      copyright sign */
            strData.Replace((char)0x2117, (char)0x00A9);

            /*                 much less-than                 left-pointing double angle quotation mark * */
            strData.Replace((char)0x226A, (char)0x00AB);

            /*                 left double angle bracket      left-pointing double angle quotation mark * */
            strData.Replace((char)0x300A, (char)0x00AB);

            /*                 reversed not sign              not sign */
            strData.Replace((char)0x2310, (char)0x00AC);

            /*                 mongolian todo soft hyphen     soft hyphen */
            strData.Replace((char)0x1806, (char)0x00AD);

            /*                 modifier letter macron         macron */
            strData.Replace((char)0x02C9, (char)0x00AF);

            /*                 combining macron               macron */
            strData.Replace((char)0x0304, (char)0x00AF);

            /*                 combining overline             macron */
            strData.Replace((char)0x0305, (char)0x00AF);

            /*                 ring above                     degree sign */
            strData.Replace((char)0x02DA, (char)0x00B0);

            /*                 combining ring above           degree sign */
            strData.Replace((char)0x030A, (char)0x00B0);

            /*                 superscript zero               degree sign */
            strData.Replace((char)0x2070, (char)0x00B0);

            /*                 ring operator                  degree sign */
            strData.Replace((char)0x2218, (char)0x00B0);

            /*                 minus-or-plus sign             plus-minus sign */
            strData.Replace((char)0x2213, (char)0x00B1);

            /*                 superscript one                superscript two */
            strData.Replace((char)0x00B9, (char)0x00B2);

            /*                 superscript one                superscript three */
            strData.Replace((char)0x00B9, (char)0x00B3);

            /*                 modifier letter prime          acute accent */
            strData.Replace((char)0x02B9, (char)0x00B4);

            /*                 modifier letter acute accent   acute accent */
            strData.Replace((char)0x02CA, (char)0x00B4);

            /*                 combining acute accent         acute accent */
            strData.Replace((char)0x0301, (char)0x00B4);

            /*                 prime                          acute accent */
            strData.Replace((char)0x2032, (char)0x00B4);

            /*                 reversed pilcrow sign          pilcrow sign */
            strData.Replace((char)0x204B, (char)0x00B6);

            /*                 curved stem paragraph sign ornament pilcrow sign */
            strData.Replace((char)0x2761, (char)0x00B6);

            /*                 bullet                         middle dot */
            strData.Replace((char)0x2022, (char)0x00B7);

            /*                 one dot leader                 middle dot */
            strData.Replace((char)0x2024, (char)0x00B7);

            /*                 hyphenation point              middle dot */
            strData.Replace((char)0x2027, (char)0x00B7);

            /*                 bullet operator                middle dot */
            strData.Replace((char)0x2219, (char)0x00B7);

            /*                 dot operator                   middle dot */
            strData.Replace((char)0x22C5, (char)0x00B7);

            /*                 katakana middle dot            middle dot */
            strData.Replace((char)0x30FB, (char)0x00B7);

            /*                 combining cedilla              cedilla */
            strData.Replace((char)0x0327, (char)0x00B8);

            /*                 much greater-than              right-pointing double angle quotation mark * */
            strData.Replace((char)0x226B, (char)0x00BB);

            /*                 right double angle bracket     right-pointing double angle quotation mark * */
            strData.Replace((char)0x300B, (char)0x00BB);

            /*!!!              'A' + grave				      latin capital letter a with grave */
            strData.Replace("\x0041\x0300", "\x00C0");

            /*!!!              'A' + acute			          latin capital letter a with acute */
            strData.Replace("\x0041\x0301", "\x00C1");

            /*!!!              'A' + circumflex		          latin capital letter a with circumflex */
            strData.Replace("\x0041\x0302", "\x00C2");

            /*!!!              'A' + tilde			          latin capital letter a with tilde */
            strData.Replace("\x0041\x0303", "\x00C3");

            /*!!!              'A' + diaeresis		          latin capital letter a with diaeresis */
            strData.Replace("\x0041\x0308", "\x00C4");

            /*!!!              'A' + ring			          latin capital letter a with ring above */
            strData.Replace("\x0041\x030A", "\x00C5");

            /*                 angstrom sign                  latin capital letter a with ring above */
            strData.Replace((char)0x212B, (char)0x00C5);

            /*!!!              'C' + cedilla                  latin capital letter c with cedilla */
            strData.Replace("\x0043\x0327", "\x00C7");

            /*!!!              'E' + grave                  latin capital letter e with grave */
            strData.Replace("\x0045\x0300", "\x00C8");

            /*!!!              'E' + acute                    latin capital letter e with acute */
            strData.Replace("\x0045\x0301", "\x00C9");

            /*!!!              'E' + circumflex               latin capital letter e with circumflex */
            strData.Replace("\x0045\x0302", "\x00CA");

            /*!!!              'E' + diaeresis                latin capital letter e with diaeresis */
            strData.Replace("\x0045\x0308", "\x00CB");

            /*!!!              'I' + grave                    latin capital letter i with grave */
            strData.Replace("\x0049\x0300", "\x00CC");

            /*!!!              'I' + acute                    latin capital letter i with acute */
            strData.Replace("\x0049\x0301", "\x00CD");

            /*!!!              'I' + circumflex               latin capital letter i with circumflex */
            strData.Replace("\x0049\x0302", "\x00CE");

            /*!!!              'I' + diaeresis                latin capital letter i with diaeresis */
            strData.Replace("\x0049\x0308", "\x00CF");

            /*                 latin capital letter d with stroke latin capital letter eth (icelandic) */
            strData.Replace((char)0x0110, (char)0x00D0);

            /*                 latin capital letter african d latin capital letter eth (icelandic) */
            strData.Replace((char)0x0189, (char)0x00D0);

            /*!!!              'N' + tilde                    latin capital letter n with tidle */
            strData.Replace("\x004E\x0303", "\x00D1");

            /*!!!              'O' + grave                    latin capital letter o with grave */
            strData.Replace("\x004F\x0300", "\x00D2");

            /*!!!              'O' + acute                    latin capital letter o with acute */
            strData.Replace("\x004F\x0301", "\x00D3");

            /*!!!              'O' + circumflex               latin capital letter o with circumflex */
            strData.Replace("\x004F\x0302", "\x00D4");

            /*!!!              'O' + tilde                    latin capital letter o with diaeresis */
            strData.Replace("\x004F\x0303", "\x00D5");

            /*!!!              'O' + diaeresis                latin capital letter o with diaeresis */
            strData.Replace("\x004F\x0308", "\x00D6");

            /*                 empty set                      latin capital letter o with stroke */
            strData.Replace((char)0x2205, (char)0x00D8);

            /*!!!              'U' + grave                    latin capital letter u with grave */
            strData.Replace("\x0055\x0300", "\x00D9");

            /*!!!              'U' + acute                    latin capital letter u with acute */
            strData.Replace("\x0055\x0301", "\x00DA");

            /*!!!              'U' + circumflex               latin capital letter u with circumflex */
            strData.Replace("\x0055\x0302", "\x00DB");

            /*!!!              'U' + diaeresis                latin capital letter u with diaeresis */
            strData.Replace("\x0055\x0308", "\x00DC");

            /*!!!              'Y' + acute                    latin capital letter y with acute */
            strData.Replace("\x0059\x0301", "\x00DD");

            /*                 greek small letter beta        latin small letter sharp s (german) */
            strData.Replace((char)0x03B2, (char)0x00DF);

            /*!!!              'a' + grave				      latin small letter a with grave */
            strData.Replace("\x0061\x0300", "\x00E0");

            /*!!!              'a' + acute			          latin small letter a with acute */
            strData.Replace("\x0061\x0301", "\x00E1");

            /*!!!              'a' + circumflex		          latin small letter a with circumflex */
            strData.Replace("\x0061\x0302", "\x00E2");

            /*!!!              'a' + tilde			          latin small letter a with tilde */
            strData.Replace("\x0061\x0303", "\x00E3");

            /*!!!              'a' + diaeresis		          latin small letter a with diaeresis */
            strData.Replace("\x0061\x0308", "\x00E4");

            /*!!!              'a' + ring			          latin small letter a with ring above */
            strData.Replace("\x0061\x030A", "\x00E5");

            /*!!!              'c' + cedilla                  latin small letter c with cedilla */
            strData.Replace("\x0063\x0327", "\x00E7");

            /*!!!              'e' + grave                    latin small letter e with grave */
            strData.Replace("\x0065\x0300", "\x00E8");

            /*!!!              'e' + acute                    latin small letter e with acute */
            strData.Replace("\x0065\x0301", "\x00E9");

            /*!!!              'e' + circumflex               latin small letter e with circumflex */
            strData.Replace("\x0065\x0302", "\x00EA");

            /*!!!              'e' + diaeresis                latin small letter e with diaeresis */
            strData.Replace("\x0065\x0308", "\x00EB");

            /*!!!              'i' + grave                    latin small letter i with grave */
            strData.Replace("\x0069\x0300", "\x00EC");

            /*!!!              'i' + acute                    latin small letter i with acute */
            strData.Replace("\x0069\x0301", "\x00ED");

            /*!!!              'i' + circumflex               latin small letter i with circumflex */
            strData.Replace("\x0069\x0302", "\x00EE");

            /*!!!              'i' + diaeresis                latin small letter i with diaeresis */
            strData.Replace("\x0069\x0308", "\x00EF");

            /*!!!              'n' + tilde                    latin small letter n with tidle */
            strData.Replace("\x006E\x0303", "\x00F1");

            /*!!!              'o' + grave                    latin small letter o with grave */
            strData.Replace("\x006F\x0300", "\x00F2");

            /*!!!              'o' + acute                    latin small letter o with acute */
            strData.Replace("\x006F\x0301", "\x00F3");

            /*!!!              'o' + circumflex               latin small letter o with circumflex */
            strData.Replace("\x006F\x0302", "\x00F4");

            /*!!!              'o' + tilde                    latin small letter o with diaeresis */
            strData.Replace("\x006F\x0303", "\x00F5");

            /*!!!              'o' + diaeresis                latin small letter o with diaeresis */
            strData.Replace("\x006F\x0308", "\x00F6");

            /*!!!              'u' + grave                    latin small letter u with grave */
            strData.Replace("\x0075\x0300", "\x00F9");

            /*!!!              'u' + acute                    latin capital letter u with acute */
            strData.Replace("\x0075\x0301", "\x00FA");

            /*!!!              'u' + circumflex               latin capital letter u with circumflex */
            strData.Replace("\x0075\x0302", "\x00FB");

            /*!!!              'u' + diaeresis                latin capital letter u with diaeresis */
            strData.Replace("\x0075\x0308", "\x00FC");

            /*!!!              'y' + acute                    latin capital letter y with acute */
            strData.Replace("\x0079\x0301", "\x00FD");

            /*!!!              'y' + diaeresis                0latin capital letter y with diaeresis */
            strData.Replace("\x0079\x0308", "\x00FD");

            /*                 latin small ligature oe        latin small letter ae (ash) * */
            strData.Replace((char)0x0153, (char)0x00E6);

            /*                 cyrillic small ligature a ie   latin small letter ae (ash) * */
            strData.Replace((char)0x04D5, (char)0x00E6);

            /*                 greek small letter delta       latin small letter eth (icelandic) */
            strData.Replace((char)0x03B4, (char)0x00F0);

            /*                 partial differential           latin small letter eth (icelandic) */
            strData.Replace((char)0x2202, (char)0x00F0);

            /*                 runic letter thurisaz thurs thorn latin small letter thorn (icelandic) */
            strData.Replace((char)0x16A6, (char)0x00FE);

            /*                 latin capital letter y with diaeresis latin small letter y with diaeresis */
            strData.Replace((char)0x0178, (char)0x00FF);



            return strData;
        }

        #endregion

        #region IsNumeric function
        public static bool IsNumeric(string s)
        {
            Double d;
            bool result = Double.TryParse(s, out d);
            return result;
        }
        #endregion

        #region IsBoolean function
        public static bool IsBoolean(string s)
        {
            bool b;
            bool result = Boolean.TryParse(s, out b);
            return result;
        }
        #endregion

        #region IsMoney function
        public static bool IsMoney(string s)
        {
            decimal d;
            bool result = decimal.TryParse(s, out d);
            return result;
        }
        #endregion

        #region IsDateTime function
        public static bool IsDateTime(string s)
        {
            System.DateTime dt;
            bool result = System.DateTime.TryParse(s, out dt);
            return result;
        }
        #endregion

        #region IsGuid function
        public static bool IsGuid(string s)
        {
            bool isValid = false;
            if (!String.IsNullOrEmpty(s))
                isValid = _guidRegex.IsMatch(s);
            return isValid;
        }
        #endregion

        public static DateTime ParseDateTime(string value)
        {
            DateTime rv = new DateTime(1900, 1, 1);

            if (!String.IsNullOrEmpty(value))
            {
                DateTime parsedValue = new DateTime(1900, 1, 1);
                if (DateTime.TryParse(value.ToString(), out parsedValue))
                    rv = parsedValue;
            }
            return rv;
        }


        #region IsNullOrEmpty
        public static bool IsNullOrEmpty(string s)
        {
            if (s == null || (s != null && s == String.Empty))
                return true;
            else
                return false;
        }
        #endregion

        #region Encrypt Decrypt 256



        #endregion

        #region JSON Parser



        #endregion

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public static string uniqueNumber()
        {
            int randomPart = new Random().Next(0, 1000);
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string unstring = GenerateRandomString(4);

            return $"{timestamp}{randomPart:D3}{unstring}";

        }
        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GenerateUniqueNumber()
        {
            // Use timestamp + random number to make it unique
            string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"); // e.g. 20250917123456789
            string randomPart = new Random().Next(1000, 9999).ToString();     // 4-digit random number

            // Combine and take only 12 digits
            string uniqueNumber = (timestamp + randomPart).Substring(0, 12);
            return uniqueNumber;
        }

        public static string Base64UrlDecode(string base64Url)
        {
            string padded = base64Url
                .Replace('-', '+')
                .Replace('_', '/');

            // Add padding if needed
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }

            var bytes = Convert.FromBase64String(padded);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        public static string Base64UrlEncode(string plainText)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            string base64 = Convert.ToBase64String(bytes);

            // Make URL safe
            string base64Url = base64
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('='); // remove padding

            return base64Url;
        }


        private static readonly byte[] Salt = Encoding.UTF8.GetBytes("MyFixedSalt-ChangeMe!");
        private const int Iterations = 100_000;
        private const int KeySize = 32; // 256 bits
        private const int IVSize = 16;  // 128 bits

        /// <summary>
        /// Encrypts plain text and returns it in the format: GUID + HexData
        /// Example: 3e816d77-8214-41b9-938f-0dc0145d7e8b1301000401883
        /// </summary>
        public static string Encrypt(string plainText, string password = "DevelopedByMdAkbarHossain")
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentException("Plain text cannot be null or empty.", nameof(plainText));

            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));

            byte[] encrypted;
            byte[] iv;

            using (var keyDerivation = new Rfc2898DeriveBytes(password, Salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] key = keyDerivation.GetBytes(KeySize);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.GenerateIV();
                    iv = aes.IV;

                    using (var encryptor = aes.CreateEncryptor())
                    using (var ms = new MemoryStream())
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                        cs.Write(plainBytes, 0, plainBytes.Length);
                        cs.FlushFinalBlock();
                        encrypted = ms.ToArray();
                    }
                }
            }

            // Combine IV and encrypted data
            byte[] combined = new byte[iv.Length + encrypted.Length];
            Array.Copy(iv, 0, combined, 0, iv.Length);
            Array.Copy(encrypted, 0, combined, iv.Length, encrypted.Length);

            // Generate a new GUID and combine with hex data
            string guid = Guid.NewGuid().ToString();
            string hexData = ByteArrayToHexString(combined);

            return guid + hexData;
        }

        /// <summary>
        /// Decrypts data from the format: GUID + HexData
        /// Example: 3e816d77-8214-41b9-938f-0dc0145d7e8b1301000401883
        /// </summary>
        public static string Decrypt(string encryptedData, string password = "DevelopedByMdAkbarHossain")
        {
            if (string.IsNullOrEmpty(encryptedData))
                throw new ArgumentException("Encrypted data cannot be null or empty.", nameof(encryptedData));

            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));

            try
            {
                // Clean input
                string cleanInput = encryptedData.Trim().Replace(" ", "").Replace("\r", "").Replace("\n", "");

                // Extract GUID part (first 36 characters including hyphens)
                if (cleanInput.Length < 36)
                    throw new ArgumentException("Invalid format: too short to contain GUID.");

                string guidPart = cleanInput.Substring(0, 36);
                string hexPart = cleanInput.Substring(36);

                // Validate GUID format
                if (!Guid.TryParse(guidPart, out _))
                    throw new ArgumentException("Invalid GUID format in encrypted data.");

                if (string.IsNullOrEmpty(hexPart))
                    throw new ArgumentException("No hex data found after GUID.");

                // Convert hex to bytes
                byte[] fullCipher = HexStringToByteArray(hexPart);

                if (fullCipher.Length < IVSize)
                    throw new ArgumentException($"Invalid encrypted data. Length {fullCipher.Length} is too short to contain IV (minimum {IVSize} bytes).");

                // Extract IV and cipher data
                byte[] iv = new byte[IVSize];
                byte[] cipher = new byte[fullCipher.Length - IVSize];

                Array.Copy(fullCipher, 0, iv, 0, IVSize);
                Array.Copy(fullCipher, IVSize, cipher, 0, cipher.Length);

                // Decrypt
                using (var keyDerivation = new Rfc2898DeriveBytes(password, Salt, Iterations, HashAlgorithmName.SHA256))
                {
                    byte[] key = keyDerivation.GetBytes(KeySize);

                    using (Aes aes = Aes.Create())
                    {
                        aes.Key = key;
                        aes.IV = iv;
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.PKCS7;

                        try
                        {
                            using (var decryptor = aes.CreateDecryptor())
                            using (var ms = new MemoryStream(cipher))
                            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                            using (var resultStream = new MemoryStream())
                            {
                                cs.CopyTo(resultStream);
                                byte[] decryptedBytes = resultStream.ToArray();
                                return Encoding.UTF8.GetString(decryptedBytes);
                            }
                        }
                        catch (CryptographicException ex)
                        {
                            throw new CryptographicException("Decryption failed. Wrong password or corrupted data.", ex);
                        }
                    }
                }
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (CryptographicException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error during decryption: {ex.Message}", ex);
            }
        }

        // Helper method to convert byte array to hex string
        private static string ByteArrayToHexString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        // Helper method to convert hex string to byte array
        private static byte[] HexStringToByteArray(string hex)
        {
            if (hex.Length % 2 != 0)
                throw new ArgumentException("Hex string must have even length");

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                try
                {
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                }
                catch (FormatException)
                {
                    throw new ArgumentException($"Invalid hex character at position {i}: '{hex.Substring(i, 2)}'");
                }
            }
            return bytes;
        }

        // Method to parse the format and get components
        public static (string guid, string hexData) ParseEncryptedData(string encryptedData)
        {
            if (string.IsNullOrEmpty(encryptedData) || encryptedData.Length < 36)
                throw new ArgumentException("Invalid encrypted data format");

            string guidPart = encryptedData.Substring(0, 36);
            string hexPart = encryptedData.Substring(36);

            return (guidPart, hexPart);
        }


    }
}
