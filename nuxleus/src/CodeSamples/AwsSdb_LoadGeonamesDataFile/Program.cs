/* -*- Mode: Java; c-basic-offset: 2 -*- */
/*
 * This software code is made available "AS IS" without warranties of any
 * kind.  You may copy, display, modify and redistribute the software
 * code either by itself or as incorporated into your code; provided that
 * you do not remove any proprietary notices.  Your use of this software
 * code is at your own risk and you waive any claim against Amazon
 * Web Services LLC or its affiliates with respect to your use of
 * this software code.
 * 
 * @copyright 2007 Amazon Web Services LLC or its affiliates.
 *            All rights reserved.
 */
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Nuxleus.Extension.Aws;
using Nuxleus.Extension.Aws.Sdb;

class BasicSample
{

    public static void Main (string[] args)
    {
        bool m_dryRun = false;

        if (args.Length > 0)
        {
            if (args[0] == "dryrun")
            {
                m_dryRun = true;
            }
        }

        string awsAccessKey =
          System.Environment.GetEnvironmentVariable("SDB_ACCESS_KEY");
        string awsSecretKey =
          System.Environment.GetEnvironmentVariable("SDB_SECRET_KEY");

        string domainName = "geonames";

        // Create a new instance of the SDB class
        HttpQueryConnection connection = new HttpQueryConnection(awsAccessKey, awsSecretKey, "http://sdb.amazonaws.com/");
        Sdb sdb = new Sdb(connection);

        System.Console.WriteLine();
        System.Console.WriteLine("Step 1: Creating the domain.");

        try
        {
            sdb.CreateDomain(domainName);

        }
        catch (SdbException ex)
        {
            handleException(ex);
        }

        Domain domain = sdb.GetDomain(domainName);

        System.Console.WriteLine();
        System.Console.WriteLine("Step 2: Loading the GeoNames Data File.");

        using (StreamReader csvReader = new StreamReader("./allCountries.txt"))
        {
            string inputLine = "";

            while ((inputLine = csvReader.ReadLine()) != null)
            {

                string[] inputArray = inputLine.Split(new char[] { '\u0009' });

                System.Console.WriteLine(String.Format("Loading Item: {0}, with Place Name: {1}", (string)inputArray.GetValue(0), (string)inputArray.GetValue(1)));
                System.Console.WriteLine(String.Format("Array Length: {0}", inputArray.Length));

                Item item = domain.GetItem((string)inputArray.GetValue(0));

                string[] geoNamesTitle = new string[] { 
                    "names", 
                    "asciiname", 
                    "alternatenames", 
                    "latitude", 
                    "longitude", 
                    "feature_class", 
                    "feature_code",
                    "country_code",
                    "cc2",
                    "admin1_code",
                    "admin2_code",
                    "admin3_code",
                    "admin4_code",
                    "population",
                    "elevation",
                    "gtopo30",
                    "timezone",
                    "modification_date"
                    };

                string[] geoNames = new string[] { };

                geoNames[0] = (string)inputArray.GetValue(0);
                geoNames[1] = (string)inputArray.GetValue(1);
                geoNames[2] = (string)inputArray.GetValue(2);
                geoNames[3] = (string)inputArray.GetValue(3);
                geoNames[4] = (string)inputArray.GetValue(4);
                geoNames[4] = (string)inputArray.GetValue(5);
                geoNames[6] = (string)inputArray.GetValue(6);
                geoNames[7] = (string)inputArray.GetValue(7);
                geoNames[8] = (string)inputArray.GetValue(8);
                geoNames[9] = (string)inputArray.GetValue(9);
                geoNames[10] = (string)inputArray.GetValue(10);
                geoNames[11] = (string)inputArray.GetValue(11);
                geoNames[12] = (string)inputArray.GetValue(12);
                geoNames[13] = (string)inputArray.GetValue(13);
                geoNames[14] = (string)inputArray.GetValue(14);
                geoNames[15] = (string)inputArray.GetValue(15);
                geoNames[16] = (string)inputArray.GetValue(16);
                geoNames[17] = (string)inputArray.GetValue(17);

                IEnumerator attributeArray = geoNames.GetEnumerator();

                ArrayList attributes = new ArrayList();

                int count = 0;

                while (attributeArray.MoveNext())
                {
                    string current = (string)attributeArray.Current;
                    string title = (string)geoNamesTitle.GetValue(count);

                    if (current.Length > 0)
                    {
                        if (current.Contains(","))
                        {
                            IEnumerator csvEnumerator = current.Split(new char[] { ',' }).GetEnumerator();
                            while (csvEnumerator.MoveNext())
                            {
                                attributes.Add(new Nuxleus.Extension.Aws.Sdb.Attribute(title, (string)csvEnumerator.Current));
                            }
                        }
                        else
                        {
                            attributes.Add(new Nuxleus.Extension.Aws.Sdb.Attribute(title, current));
                        }
                    }
                    count++;
                }


                try
                {
                    if (!m_dryRun)
                    {
                        item.PutAttributes(attributes);
                    }
                    else
                    {
                        printAttributes(item);
                    }
                }
                catch (SdbException ex)
                {
                    handleException(ex);
                }
            }
        }
    }

    static void handleException (SdbException ex)
    {
        System.Console.WriteLine("Failure: {0}: {1} ({2})", ex.ErrorCode, ex.Message, ex.RequestId);
    }

    static void printAttributes (Item item)
    {
        System.Console.WriteLine();
        System.Console.WriteLine("Attributes for '{0}':", item.Name);
        try
        {
            GetAttributesResponse getAttributesResponse = item.GetAttributes();

            foreach (Nuxleus.Extension.Aws.Sdb.Attribute attribute in getAttributesResponse.Attributes())
            {
                System.Console.WriteLine("{0} => {1}.", attribute.Name,
                             attribute.Value);
            }
        }
        catch (SdbException ex)
        {
            handleException(ex);
        }
    }
}