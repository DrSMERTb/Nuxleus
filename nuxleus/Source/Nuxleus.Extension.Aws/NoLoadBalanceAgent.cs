﻿using System.Collections.Generic;
using System.Text;
using Nuxleus.Extension.Aws.SimpleDb;
using VVMF.SOA.Common;
using Nuxleus.Asynchronous;
using System.Net;
using System.Configuration;
using System.Threading;
using System.Runtime.Remoting;
using System.Collections;
using System.IO;
using Nuxleus.Extension.Aws.SimpleDb.Model;
using System.Xml.Linq;
using Nuxleus.Extension;
using System.Web;
using Nuxleus.Extension;
using System.Web.Configuration;
using System.Linq;
using System.Security.Permissions;

namespace Nuxleus.Extension.Aws {


    //[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
    public struct NoLoadBalanceAgent {

        static LoggerScope logger = new LoggerScope();
        static ExceptionHandlerScope exShield = new ExceptionHandlerScope();
        static ProfilerScope profiler = new ProfilerScope();

        public void Initialize() {
            ServicePointManager.DefaultConnectionLimit = int.Parse(ConfigurationManager.AppSettings["DefaultConnectionLimit"]);

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            HttpRuntimeSection configSection = (HttpRuntimeSection)config.GetSection("system.web/httpRuntime");
            this.LogInfo("ServicePointManager Default Connection Limit: {0}", ServicePointManager.DefaultConnectionLimit);
            this.LogInfo("system.web/httpRuntime/minFreeThreads: {0}", configSection.MinFreeThreads);
            this.LogInfo("system.web/httpRuntime/minLocalRequestFreeThreads: {0}", configSection.MinLocalRequestFreeThreads);

            //EnableDnsRoundRobin is not implemented on Mono
            //ServicePointManager.EnableDnsRoundRobin = true;

            //ServicePointManager.Expect100Continue = true;
            int minWorkerThreads = int.Parse(ConfigurationManager.AppSettings["MinimumWorkerThreads"]);
            int minAsyncIOThreads = int.Parse(ConfigurationManager.AppSettings["MinimumAsyncIOThreds"]);
            int maxWorkerThreads = int.Parse(ConfigurationManager.AppSettings["MaximumWorkerThreads"]);
            int maxAsyncIOThreads = int.Parse(ConfigurationManager.AppSettings["MaximumAsyncIOThreads"]);
            ThreadPool.SetMaxThreads(maxWorkerThreads, maxAsyncIOThreads);
            ThreadPool.SetMinThreads(minWorkerThreads, minAsyncIOThreads);
        }

        public void Invoke(string fileName) {
            Scope scope = new Scope();
            scope += profiler.Scope;
            scope += logger.Scope;
            scope += exShield.Scope;

            logger.Message = "Processing SOAP requests";

            string[] attributeNames = new string[] {
                "geonamesid:0",
                "names:1",
                "alternatenames:3",
                "latitude:4",
                "longitude:5",
                "feature_class:6",
                "feature_code:7",
                "country_code:8",
                "cc2:9",
                "admin1_code:10",
                "admin2_code:11",
                "admin3_code:12",
                "admin4_code:12",
                "population:14",
                "elevation:15",
                "gtopo30:16",
                "timezone:17",
                "modification_date:18",
            };

            int taskCount = 0;
            int ellapsedTime = 0;

            scope.Begin = () => {
                var tasks = from line in ReadLinesFromFile(fileName)
                            let inputArray = line.Split(new char[] { '\u0009' })
                            let attributes = CreateAttributeList(attributeNames, inputArray)
                            select CreatePutAttributesTask("foobar", "geonamesid", attributes).AsIAsync();
                taskCount = tasks.Count();
                InvokeParallelOperation(tasks).ExecuteAndWait();
            };
            ellapsedTime = profiler.EllapsedTime.Milliseconds;
            this.LogInfo("Ellapsed Time: {0}", ellapsedTime);
        }

        private static IEnumerable<IAsync> InvokeParallelOperation(IEnumerable<IEnumerable<IAsync>> tasks) {
            yield return Async.Parallel(tasks.ToArray());
        }

        private static IEnumerable<Attribute> CreateAttributeList(string[] attributeNames, string[] attributeValues) {
            foreach (string attributeName in attributeNames) {
                string title = attributeName.SubstringBefore(":");
                int pos = int.Parse(attributeName.SubstringAfter(":"));
                string attributeValue = attributeValues[pos];
                if (attributeValue.Length > 0) {
                    if (attributeValue.Contains(",")) {
                        IEnumerator csvEnumerator = attributeValue.Split(new char[] { ',' }).GetEnumerator();
                        while (csvEnumerator.MoveNext()) {
                            yield return new Attribute(attributeName, (string)csvEnumerator.Current);
                        }
                    } else {
                        yield return new Attribute(title, attributeValue);
                    }
                }
            }
        }

        private static PutAttributes CreatePutAttributesTask(string domainName, string matchItemName, IEnumerable<Attribute> attributes) {
            return new PutAttributes {
                DomainName = domainName,
                ItemName = attributes.First(a => a.Name == matchItemName).Value,
                Attribute = attributes.ToList(),
            };
        }


        // From Don Box @ http://www.pluralsight.com/community/blogs/dbox/archive/2007/10/09/48719.aspx
        // LINQ-compatible streaming I/O helper
        public static IEnumerable<string> ReadLinesFromFile(string filename) {
            using (StreamReader reader = new StreamReader(filename)) {
                while (true) {
                    string s = reader.ReadLine();
                    if (s == null)
                        break;
                    yield return s;
                }
            }
        }
    }
}
