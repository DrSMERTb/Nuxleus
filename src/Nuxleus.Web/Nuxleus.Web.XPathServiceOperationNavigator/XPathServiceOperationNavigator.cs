using System;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using Nuxleus.Extension;
using Nuxleus.Transform;

namespace Nuxleus.Web {

    public class XPathServiceOperationNavigator
    {
        HttpContext m_httpContext;
        Context m_context;
        Transform.Context m_transformContext;
        XPathNavigator m_navigator;
        TransformRequest m_request;
        TransformResponse m_response;
        XsltTransformationManager m_xslTransformationManager;
        Transform.Transform m_transform;
        UTF8Encoding m_encoding;

        public XPathServiceOperationNavigator ( HttpContext httpContext, Context context, Transform.Context transformContext, XPathNavigator navigator, TransformRequest request, TransformResponse response, XsltTransformationManager transformationManager ) {
            m_navigator = navigator;
            m_httpContext = httpContext;
            m_context = context;
            m_transformContext = transformContext;
            m_request = request;
            m_response = response;
            m_xslTransformationManager = transformationManager;
            m_transform = transformationManager.Transform;
            m_encoding = new UTF8Encoding();
        }

        public TransformResponse Process () {

            XmlReader reader = m_navigator.ReadSubtree();

            string xmlStylesheetHref = String.Empty;
            bool processWithEmbeddedPIStylsheet = false;

            do {
                switch (reader.NodeType) {
                    case XmlNodeType.ProcessingInstruction:
                        switch (reader.Name) {
                            case "xml-stylesheet":
                                string piValue = reader.Value;
                                if (piValue.Contains("type=\"text/xsl\"") && piValue.Contains("href=")) {
                                    processWithEmbeddedPIStylsheet = true;
                                    xmlStylesheetHref = piValue.SubstringAfter("href=\"").SubstringBefore("\"");
                                }
                                this.LogInfo("Stylesheet Href = {0}", xmlStylesheetHref);
                                break;
                            default:
                                break;
                        }
                        break;
                    case XmlNodeType.Element:
                        if (reader.IsStartElement()) {
                            switch (reader.Name) {
                                case "service:compile":
                                    if (reader.HasAttributes) {
                                        bool USEMEMCACHE = false;
                                        bool SUCCESS = bool.TryParse(reader.GetAttribute("cache-result"), out USEMEMCACHE);
                                        this.LogInfo(String.Format("Operation succeeded: {0}, Use memcached: {1}", SUCCESS, USEMEMCACHE));
                                    }
                                    break;
                                case "service:session":
                                    this.LogInfo("service:session reached");
                                    break;
                                case "service:operation":
                                case "my:page":
                                    this.LogInfo("service:operation or my:page reached");
                                    Uri baseXsltUri = new Uri(m_httpContext.Request.MapPath(xmlStylesheetHref));
                                    m_xslTransformationManager.BaseXsltUri = baseXsltUri;
                                    string baseXslt = baseXsltUri.GetHashCode().ToString();

                                    if (!m_xslTransformationManager.NamedXsltHashtable.ContainsKey(baseXslt)) {
                                        m_xslTransformationManager.AddTransformer(baseXslt, baseXsltUri);
                                        m_xslTransformationManager.NamedXsltHashtable.Add(baseXslt, baseXsltUri);
                                    }
                                    if (!m_xslTransformationManager.XmlSourceHashtable.ContainsKey(m_context.RequestXmlETag)) {
                                        this.LogInfo(reader.ReadOuterXml());
                                        using (MemoryStream stream = new MemoryStream(m_encoding.GetBytes(reader.ReadOuterXml().ToCharArray()))) {
                                            m_xslTransformationManager.AddXmlSource(m_context.RequestXmlETag.ToString(), (Stream)stream);
                                        }
                                    }

                                    TransformContext transformContext = new TransformContext();
                                    transformContext.Context = m_transformContext;
                                    transformContext.HttpContext = m_httpContext;
                                    transformContext.XsltTransformationManager = m_xslTransformationManager;
                                    transformContext.XsltName = baseXslt;
                                    transformContext.Response = m_response;
                                    m_request.TransformContext = transformContext;
                                    m_response = m_transform.BeginTransformProcess(m_request);
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case XmlNodeType.EndElement:

                        continue;
                    case XmlNodeType.Text:
                    case XmlNodeType.SignificantWhitespace:
                    case XmlNodeType.Whitespace:

                        break;
                    case XmlNodeType.CDATA:

                        break;
                    case XmlNodeType.Comment:
                        break;
                    case XmlNodeType.DocumentType:
                        break;
                    case XmlNodeType.EntityReference:
                        reader.ResolveEntity();
                        continue;

                    case XmlNodeType.XmlDeclaration:
                    case XmlNodeType.EndEntity:
                        continue;
                    default:
                        break;
                    //throw new InvalidOperationException();

                }
            } while (reader.Read());

            return m_response;
        }
    }
}