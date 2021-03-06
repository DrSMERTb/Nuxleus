﻿using System;
using System.IO;
using System.Net;
using System.Text;

namespace Xameleon.Function {

    public class HttpWebRequestStream {

        public HttpWebRequestStream () { }

        public static string GetResponse ( String uri ) {
            return new WebClient().DownloadString(uri);
        }

        public static string GetResponse ( String uri, String formValues ) {
            WebRequest myHttpWebRequest = WebRequest.Create(uri);

            myHttpWebRequest.Method = "POST";
            string postData = formValues;
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] byte1 = encoding.GetBytes(postData);

            myHttpWebRequest.ContentType = "application/x-www-form-urlencoded";

            myHttpWebRequest.ContentLength = byte1.Length;

            using (Stream newStream = myHttpWebRequest.GetRequestStream()) {
                newStream.Write(byte1, 0, byte1.Length);
            }
            return GetResponseString(myHttpWebRequest.GetResponse().GetResponseStream());
        }

        public static string GetResponseString ( Stream stream ) {
            using (StreamReader reader = new StreamReader(stream)) {
                return reader.ReadToEnd();
            }
        }

        //public static string GetResponse (String uri, bool returnUri) {
        //    WebRequest myHttpWebRequest = WebRequest.Create(uri);
        //    myHttpWebRequest.Method = "GET";
        //    if (!returnUri) {
        //        return GetResponseString(myHttpWebRequest.GetResponse().GetResponseStream());
        //    } else {
        //        return myHttpWebRequest.GetResponse().ResponseUri.ToString();
        //    }
        //}
    }
}
