using Dynastream.Fit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeakswareTestOop
{
    class DataPoint 
    {
        public enum Type : int
        {
            DataRaw=0,
            Data1MinuteAverage=1,
            Data5MinuteAverage=2,
            Data10MinuteAverage=3,
            Data15MinuteAverage=4,
            Data20MinuteAverage = 5
        }

        private ushort[] _dataValues = new ushort[6]; 
        public Double TimeOffset { get;  internal set; }

        internal ushort[] DataValues
        {
            get
            {
                return _dataValues;
            }
        }
        public ushort DataValueRaw 
        {
            get
            {
                return _dataValues[(int)DataPoint.Type.DataRaw];
            }
            internal set
            {
                _dataValues[(int)DataPoint.Type.DataRaw] = value;
            }
        }

        public ushort DataValue1MinAvg
        {
            get
            {
                return _dataValues[(int)DataPoint.Type.Data1MinuteAverage];
            }
        }

        public ushort DataValue5MinAvg
        {
            get
            {
                return _dataValues[(int)DataPoint.Type.Data5MinuteAverage];
            }
        }

        public ushort DataValue10MinAvg
        {
            get
            {
                return _dataValues[(int)DataPoint.Type.Data10MinuteAverage];
            }
        }

        public ushort DataValue15MinAvg
        {
            get
            {
                return _dataValues[(int)DataPoint.Type.Data15MinuteAverage];
            }
        }

        public ushort DataValue20MinAvg
        {
            get
            {
                return _dataValues[(int)DataPoint.Type.Data20MinuteAverage];
            }
        }

        public DataPoint(Double timeOffset, ushort dataValue)
        {
            TimeOffset = timeOffset;
            DataValueRaw = dataValue;
        }

        public static bool operator ==(DataPoint dp1, DataPoint dp2)
        {
            if (dp1 != null && dp2 != null)
                return dp1.TimeOffset == dp2.TimeOffset;
            else
                return false;
        }

        public static bool operator !=(DataPoint dp1, DataPoint dp2)
        {
            if (dp1 != null && dp2 != null)
                return dp1.TimeOffset != dp2.TimeOffset;
            else
                return true;
            
        }

        
    }

    class ChannelData : List<DataPoint>  
    {
        private Object lockCollection = new Object();
        
        private int _threadCount;
        public string DataName { get; private set; }
        public int ThreadCount 
        {
            get
            {
                return _threadCount;
            }
        }

        public ChannelData(string dataName)
        {
            DataName = dataName;
            _threadCount = 0;
        }

        new public void Add(double timeOffset, DataPoint datapoint)
        {
            lock (lockCollection)
            {
                base.Add(datapoint);
            }
            CalculateAverages(datapoint);
            return;
        }

        private void CalculateAverages(DataPoint datapoint)
        {

            Task.Factory.StartNew(() => CalculateAveragesFor(datapoint, 60, DataPoint.Type.Data1MinuteAverage));
            Task.Factory.StartNew(() => CalculateAveragesFor(datapoint, 60, DataPoint.Type.Data1MinuteAverage));
            Task.Factory.StartNew(() => CalculateAveragesFor(datapoint, 60 * 5, DataPoint.Type.Data5MinuteAverage));
            Task.Factory.StartNew(() => CalculateAveragesFor(datapoint, 60 * 10, DataPoint.Type.Data10MinuteAverage));
            Task.Factory.StartNew(() => CalculateAveragesFor(datapoint, 60 * 15, DataPoint.Type.Data15MinuteAverage));
            Task.Factory.StartNew(() => CalculateAveragesFor(datapoint, 60 * 20, DataPoint.Type.Data20MinuteAverage));

            //CalculateAveragesFor(datapoint, 60, DataPoint.Type.Data1MinuteAverage);
            //CalculateAveragesFor(datapoint, 60, DataPoint.Type.Data1MinuteAverage);
            //CalculateAveragesFor(datapoint, 60 * 5, DataPoint.Type.Data5MinuteAverage);
            //CalculateAveragesFor(datapoint, 60 * 10, DataPoint.Type.Data10MinuteAverage);
            //CalculateAveragesFor(datapoint, 60 * 15, DataPoint.Type.Data15MinuteAverage);
            //CalculateAveragesFor(datapoint, 60 * 20, DataPoint.Type.Data20MinuteAverage);
        }

        private void CalculateAveragesFor(DataPoint dataPointJustAdded, ushort spanSeconds, DataPoint.Type type)
        {
            List<ushort> dataForAverage = new List<ushort>();

            DataPoint currentDataPoint;
            int currentIndex = this.IndexOf(dataPointJustAdded);
            double currentTimeOffset=0;


            Interlocked.Increment(ref _threadCount);
            
            int iIndex = currentIndex;
            do
            {
                ushort dataValueRaw = 0;
                currentDataPoint = this[iIndex];
                
                
                dataValueRaw = currentDataPoint.DataValueRaw;
                currentTimeOffset = currentDataPoint.TimeOffset;
                
                
                dataForAverage.Add(dataValueRaw);
                iIndex--;
            }
            while (iIndex >= 0 && dataPointJustAdded.TimeOffset - currentTimeOffset < spanSeconds * 1000);

            dataPointJustAdded.DataValues[(int)type] = Convert.ToUInt16( dataForAverage.Average(num => Convert.ToUInt16(num)));

            Interlocked.Decrement(ref _threadCount);
            
        }

        public ushort GetBestAverageData(PeakswareTestOop.DataPoint.Type type)
        {
            DataPoint maxDatapoint = this[0];
            foreach (DataPoint currentDataPoint in this)
            {
                
                if(maxDatapoint.DataValues[(int)type] <  currentDataPoint.DataValues[(int)type] )
                {
                    maxDatapoint = currentDataPoint;
                }
            }

            return maxDatapoint.DataValues[(int)type];
        }
    }

    class FitFile
    {
        public ChannelData _powerData = new ChannelData("Power");
        public ChannelData _heartrateData = new ChannelData("HeartRate");
        public ChannelData _cadenceData = new ChannelData("Cadence");

        public ChannelData PowerData { get { return _powerData; } }
        public ChannelData HeartRate { get { return _heartrateData; } }
        public ChannelData Cadence { get { return _cadenceData; } }

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

                        while (Cadence.ThreadCount > 0 || HeartRate.ThreadCount > 0 || PowerData.ThreadCount > 0) 
                        {
                            Thread.Sleep(10);
                        }
                        
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
            var heartrate = record.GetFieldValue("HeartRate");
            var cadence = record.GetFieldValue("Cadence");

            if (power != null || heartrate != null || cadence != null)
            {
                double timeOffset = GetTimeOffset(record.GetTimestamp().GetDateTime());
                if (power != null)
                {
                    DataPoint dataPoint = new DataPoint(timeOffset, (ushort)power);
                    _powerData.Add(timeOffset, dataPoint);
                }

                if (heartrate != null)
                {
                    DataPoint dataPoint = new DataPoint(timeOffset, Convert.ToUInt16( heartrate.ToString() ));
                    _heartrateData.Add(timeOffset, dataPoint);
                }

                if (cadence != null)
                {
                    DataPoint dataPoint = new DataPoint(timeOffset, Convert.ToUInt16(cadence.ToString()));
                    _cadenceData.Add(timeOffset, dataPoint);
                }
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
