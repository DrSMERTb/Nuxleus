﻿// Copyright 2011 Max Toro Q.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using System.Xml;
using System.IO;

namespace Nuxleus.Web.Module.SmtpClient {
   
   public sealed class XPathMailBody {

      XmlQualifiedName _Method;

      public XPathItem Content { get; set; }

      public XmlQualifiedName Method {
         get {
            if (_Method != null)
               return _Method;
            return XmlSerializationOptions.Methods.Text;
         }
         set { _Method = value; }
      }

      public void ReadXml(XPathNavigator node) {

         if (node.MoveToFirstAttribute()) {

            do {
               if (String.IsNullOrEmpty(node.NamespaceURI)) {

                  switch (node.LocalName) {
                     case "method":
                        switch (node.Value) {
                           case "xml":
                              this.Method = XmlSerializationOptions.Methods.Xml;
                              break;

                           case "html":
                              this.Method = XmlSerializationOptions.Methods.Html;
                              break;

                           case "xhtml":
                              this.Method = XmlSerializationOptions.Methods.XHtml;
                              break;

                           case "text":
                              this.Method = XmlSerializationOptions.Methods.Text;
                              break;
                        }
                        break;

                     default:
                        break;
                  }
               }
            } while (node.MoveToNextAttribute());

            node.MoveToParent();
         }

         if (node.MoveToFirstChild()) {

            do {
               if (node.NodeType == XPathNodeType.Element || node.NodeType == XPathNodeType.Text) {
                  this.Content = node.Clone();
                  break;
               }

            } while (node.MoveToNext());

            node.MoveToParent();
         }
      }

      public void Serialize(TextWriter output, XPathItemFactory itemFactory) {

         if (this.Content == null)
            return;
         
         XmlSerializationOptions serialization = new XmlSerializationOptions {
            Method = this.Method,
         };

         itemFactory.Serialize(this.Content, output, serialization);
      }
   }
}
