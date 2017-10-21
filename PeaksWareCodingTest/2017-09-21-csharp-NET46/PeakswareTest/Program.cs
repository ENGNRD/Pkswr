using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynastream.Fit;
using System.IO;
using System.Threading;

namespace PeakswareTest
{
    class Program
    {
        private static Dictionary<double, ushort> _powerChannel;
        private static System.DateTime? _start;

        static void Main(string[] args)
        {
            string strFilename = "..\\..\\files\\2012-05-31-11-17-12.fit";
            System.DateTime start = System.DateTime.Now;
            Console.WriteLine("TrainingPeaks C# Code Test");
            Console.WriteLine("input file {0}", strFilename);

            //if (args.Length == 1)
            //{
            //    strFilename = args[0];
                
            //}
            //Console.WriteLine("input file {0}", strFilename);

            PeakswareTestOop.FitFile fitFile = new PeakswareTestOop.FitFile(strFilename);
            fitFile.LoadDataAsync();

            // Attempt to open .FIT file
            //using (var fitSource = new FileStream(args[0], FileMode.Open))
            //{
            //    Console.WriteLine("Opening {0}", args[0]);

            //    _powerChannel = new Dictionary<double, ushort>();

            //    Decode decodeDemo = new Decode();
            //    MesgBroadcaster mesgBroadcaster = new MesgBroadcaster();

            //    // Connect the Broadcaster to our event source (in this case the Decoder)
            //    decodeDemo.MesgEvent += mesgBroadcaster.OnMesg;

            //    // Subscribe to message events of interest by connecting to the Broadcaster
            //    mesgBroadcaster.RecordMesgEvent += new MesgEventHandler(OnRecordMesg);
            //    mesgBroadcaster.FileIdMesgEvent += new MesgEventHandler(OnFileIDMesg);

            //    bool status = decodeDemo.IsFIT(fitSource) && decodeDemo.CheckIntegrity(fitSource);
                
            //    // Process the file
            //    if (status)
            //    {
            //        Console.WriteLine("Decoding...");
            //        decodeDemo.Read(fitSource);

            //        Console.WriteLine("Decoded FIT file {0}", args[0]);
            //    }
            //    else
            //    {
            //        Console.WriteLine("Integrity Check Failed {0}", args[0]);
            //        decodeDemo.Read(fitSource);
            //    }
            //}

            

            foreach (PeakswareTestOop.DataPoint dataPoint in fitFile.PowerData)
            {
                Console.WriteLine("{0,7} {1,4} {2,4} {3,4} {4,4} {5,4} {6,4}", dataPoint.TimeOffset, dataPoint.DataValueRaw, dataPoint.DataValue1MinAvg, dataPoint.DataValue5MinAvg, dataPoint.DataValue10MinAvg, dataPoint.DataValue15MinAvg, dataPoint.DataValue20MinAvg);
            }

            Console.WriteLine("Data1MinuteAverage {0} ", fitFile.PowerData.GetBestAverageData(PeakswareTestOop.DataPoint.Type.Data1MinuteAverage));
            Console.WriteLine("Data5MinuteAverage {0} ", fitFile.PowerData.GetBestAverageData(PeakswareTestOop.DataPoint.Type.Data5MinuteAverage));
            Console.WriteLine("Data10MinuteAverage {0} ", fitFile.PowerData.GetBestAverageData(PeakswareTestOop.DataPoint.Type.Data10MinuteAverage));
            Console.WriteLine("Data15MinuteAverage {0} ", fitFile.PowerData.GetBestAverageData(PeakswareTestOop.DataPoint.Type.Data15MinuteAverage));
            Console.WriteLine("Data20MinuteAverage {0} ", fitFile.PowerData.GetBestAverageData(PeakswareTestOop.DataPoint.Type.Data20MinuteAverage));


            foreach (PeakswareTestOop.DataPoint dataPoint in fitFile.HeartRate)
            {
                Console.WriteLine("{0,7} {1,4} {2,4} {3,4} {4,4} {5,4} {6,4}", dataPoint.TimeOffset, dataPoint.DataValueRaw, dataPoint.DataValue1MinAvg, dataPoint.DataValue5MinAvg, dataPoint.DataValue10MinAvg, dataPoint.DataValue15MinAvg, dataPoint.DataValue20MinAvg);
            }

            Console.WriteLine("Data1MinuteAverage {0} ", fitFile.HeartRate.GetBestAverageData(PeakswareTestOop.DataPoint.Type.Data1MinuteAverage));
            Console.WriteLine("Data5MinuteAverage {0} ", fitFile.HeartRate.GetBestAverageData(PeakswareTestOop.DataPoint.Type.Data5MinuteAverage));
            Console.WriteLine("Data10MinuteAverage {0} ", fitFile.HeartRate.GetBestAverageData(PeakswareTestOop.DataPoint.Type.Data10MinuteAverage));
            Console.WriteLine("Data15MinuteAverage {0} ", fitFile.HeartRate.GetBestAverageData(PeakswareTestOop.DataPoint.Type.Data15MinuteAverage));
            Console.WriteLine("Data20MinuteAverage {0} ", fitFile.HeartRate.GetBestAverageData(PeakswareTestOop.DataPoint.Type.Data20MinuteAverage));

            foreach (PeakswareTestOop.DataPoint dataPoint in fitFile.Cadence)
            {
                Console.WriteLine("{0,7} {1,4} {2,4} {3,4} {4,4} {5,4} {6,4}", dataPoint.TimeOffset, dataPoint.DataValueRaw, dataPoint.DataValue1MinAvg, dataPoint.DataValue5MinAvg, dataPoint.DataValue10MinAvg, dataPoint.DataValue15MinAvg, dataPoint.DataValue20MinAvg);
            }

            Console.WriteLine("Data1MinuteAverage {0} ", fitFile.Cadence.GetBestAverageData(PeakswareTestOop.DataPoint.Type.Data1MinuteAverage));
            Console.WriteLine("Data5MinuteAverage {0} ", fitFile.Cadence.GetBestAverageData(PeakswareTestOop.DataPoint.Type.Data5MinuteAverage));
            Console.WriteLine("Data10MinuteAverage {0} ", fitFile.Cadence.GetBestAverageData(PeakswareTestOop.DataPoint.Type.Data10MinuteAverage));
            Console.WriteLine("Data15MinuteAverage {0} ", fitFile.Cadence.GetBestAverageData(PeakswareTestOop.DataPoint.Type.Data15MinuteAverage));
            Console.WriteLine("Data20MinuteAverage {0} ", fitFile.Cadence.GetBestAverageData(PeakswareTestOop.DataPoint.Type.Data20MinuteAverage));



            System.DateTime end = System.DateTime.Now;
            Console.WriteLine("Done in {0} Seconds!", (end-start).TotalSeconds);
            Console.ReadLine();
        }

        static void OnRecordMesg(object sender, MesgEventArgs e)
        {
            var record = (RecordMesg)e.mesg;

            var power = record.GetFieldValue("Power");

            if (power != null)
            {
                _powerChannel.Add(GetTimeOffset (record.GetTimestamp().GetDateTime()), (ushort)power);
            }
        }

        static double GetTimeOffset(System.DateTime date)
        {
            if (_start == null)
                return 0;
            
            return date.Subtract(_start.Value).TotalMilliseconds;
        }

        static void OnFileIDMesg(object sender, MesgEventArgs e)
        {
            FileIdMesg myFileId = (FileIdMesg)e.mesg;

            _start = myFileId.GetTimeCreated().GetDateTime();
        }
    }
}
