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
using System.Collections.ObjectModel;
using System.Xml.XPath;
using System.Net.Mail;
using System.IO;

namespace Nuxleus.Web.Module.SmtpClient {

   public sealed class XPathMailMessage {

      public XPathMailAddress From { get; set; }
      public Collection<XPathMailAddress> To { get; private set; }
      public Collection<XPathMailAddress> CC { get; private set; }
      public Collection<XPathMailAddress> Bcc { get; private set; }
      public Collection<XPathMailAddress> ReplyTo { get; private set; }
      public XPathMailAddress Sender { get; set; }
      public string Subject { get; set; }
      public XPathMailBody Body { get; set; }

      public XPathMailMessage() {

         this.To = new Collection<XPathMailAddress>();
         this.CC = new Collection<XPathMailAddress>();
         this.Bcc = new Collection<XPathMailAddress>();
         this.ReplyTo = new Collection<XPathMailAddress>();
      }

      public void ReadXml(XPathNavigator node) {

         bool movedToDocEl = false;

         if (node.NodeType == XPathNodeType.Root)
            movedToDocEl = node.MoveToChild(XPathNodeType.Element);

         if (node.NamespaceURI == XPathSmtpClient.Namespace
            && node.LocalName == "message") {

            if (node.MoveToFirstAttribute()) {

               do {
                  if (String.IsNullOrEmpty(node.NamespaceURI)) {

                  }
               } while (node.MoveToNextAttribute());

               node.MoveToParent();
            }

            if (node.MoveToChild(XPathNodeType.Element)) {

               do {
                  if (node.NamespaceURI == XPathSmtpClient.Namespace) {

                     switch (node.LocalName) {
                        case "from":
                           this.From = new XPathMailAddress();
                           this.From.ReadXml(node);
                           break;

                        case "reply-to": {
                              XPathMailAddress address = new XPathMailAddress();
                              address.ReadXml(node);

                              this.ReplyTo.Add(address);
                           }
                           break;

                        case "sender":
                           this.Sender = new XPathMailAddress();
                           this.Sender.ReadXml(node);
                           break;

                        case "to": {
                              XPathMailAddress address = new XPathMailAddress();
                              address.ReadXml(node);

                              this.To.Add(address);
                           }
                           break;

                        case "cc": {
                              XPathMailAddress address = new XPathMailAddress();
                              address.ReadXml(node);

                              this.CC.Add(address);
                           }
                           break;

                        case "bcc": {
                              XPathMailAddress address = new XPathMailAddress();
                              address.ReadXml(node);

                              this.Bcc.Add(address);
                           }
                           break;

                        case "subject":
                           this.Subject = node.Value;
                           break;

                        case "body":
                           this.Body = new XPathMailBody();
                           this.Body.ReadXml(node);
                           break;

                        default:
                           break;
                     }
                  }

               } while (node.MoveToNext(XPathNodeType.Element));

               node.MoveToParent();
            }
         }

         if (movedToDocEl)
            node.MoveToParent();
      }

      public MailMessage ToMailMessage(XPathItemFactory itemFactory) {

         var mailMessage = new MailMessage();

         if (this.From != null)
            mailMessage.From = this.From.ToMailAddress();

         for (int i = 0; i < this.To.Count; i++) 
            mailMessage.To.Add(this.To[i].ToMailAddress());

         for (int i = 0; i < this.CC.Count; i++) 
            mailMessage.CC.Add(this.CC[i].ToMailAddress());

         for (int i = 0; i < this.Bcc.Count; i++) 
            mailMessage.Bcc.Add(this.Bcc[i].ToMailAddress());

         for (int i = 0; i < this.ReplyTo.Count; i++)
            mailMessage.ReplyToList.Add(this.ReplyTo[i].ToMailAddress());

         if (this.Sender != null)
            mailMessage.Sender = this.Sender.ToMailAddress();

         if (this.Subject != null)
            mailMessage.Subject = this.Subject;

         if (this.Body != null) {
            
            using (var writer = new StringWriter()) {
               this.Body.Serialize(writer, itemFactory);

               mailMessage.Body = writer.ToString();
            }

            mailMessage.IsBodyHtml = 
               this.Body.Method == XmlSerializationOptions.Methods.Html
               || this.Body.Method == XmlSerializationOptions.Methods.XHtml;
         }

         return mailMessage;
      }
   }
}
