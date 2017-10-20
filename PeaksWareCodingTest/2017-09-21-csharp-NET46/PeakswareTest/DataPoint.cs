using Dynastream.Fit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakswareTestOop
{
    class DataPoint
    {
        public Double TimeOffset { get;  internal set; }
        public ushort DataValue { get; internal set; }
        public ushort DataValue1MinAvg { get; internal set; }
        public ushort DataValue5MinAvg { get; internal set; }
        public ushort DataValue10MinAvg { get; internal set; }
        public ushort DataValue15MinAvg { get; internal set; }
        public ushort DataValue20MinAvg { get; internal set; }

        public DataPoint(Double timeOffset, ushort dataValue)
        {
            TimeOffset = timeOffset;
            DataValue = dataValue;
        }
    }

    class ChannelData : Dictionary<double, DataPoint>  
    {
        public string DataName { get; private set; }

        public ChannelData(string dataName)
        {
            DataName = dataName;
        }

        new public void Add(double timeOffset, DataPoint datapoint)
        {
            base.Add(timeOffset, datapoint);
            return;
        }
    }

    class FitFile
    {
        public ChannelData _powerData = new ChannelData("Power");

        public ChannelData PowerData { get { return _powerData; } }

        public string FileName { get; private set; }
        private static System.DateTime? _start;

        public FitFile(string fileName)
        {
            FileName = fileName;
        }

        public void LoadDataAsync()
        {

            if (System.IO.File.Exists(FileName))
            {
                using (var fitSource = new FileStream(FileName, FileMode.Open))
                {

                    Decode decodeDemo = new Decode();
                    MesgBroadcaster mesgBroadcaster = new MesgBroadcaster();

                    // Connect the Broadcaster to our event source (in this case the Decoder)
                    decodeDemo.MesgEvent += mesgBroadcaster.OnMesg;

                    // Subscribe to message events of interest by connecting to the Broadcaster
                    mesgBroadcaster.RecordMesgEvent += new MesgEventHandler(OnRecordMesg);
                    mesgBroadcaster.FileIdMesgEvent += new MesgEventHandler(OnFileIDMesg);

                    bool status = decodeDemo.IsFIT(fitSource) && decodeDemo.CheckIntegrity(fitSource);

                    // Process the file
                    if (status)
                    {
                        decodeDemo.Read(fitSource);
                    }
                    else
                    {
                        throw new Exception(string.Format("Integrity check failed. Filename: {0}", FileName));

                    }
                }
            }
            else
            {
                throw new Exception(string.Format("File does not exist. Filename: {0}", FileName));
            }

        }

        public void OnRecordMesg(object sender, MesgEventArgs e)
        {
            var record = (RecordMesg)e.mesg;

            var power = record.GetFieldValue("Power");

            if (power != null)
            {
                double timeOffset = GetTimeOffset(record.GetTimestamp().GetDateTime());
                DataPoint dataPoint = new DataPoint(timeOffset, (ushort)power);
                _powerData.Add(timeOffset,dataPoint);
            }
        }

        public double GetTimeOffset(System.DateTime date)
        {
            if (_start == null)
                return 0;

            return date.Subtract(_start.Value).TotalMilliseconds;
        }

        public void OnFileIDMesg(object sender, MesgEventArgs e)
        {
            FileIdMesg myFileId = (FileIdMesg)e.mesg;

            _start = myFileId.GetTimeCreated().GetDateTime();
        }

    }
}
