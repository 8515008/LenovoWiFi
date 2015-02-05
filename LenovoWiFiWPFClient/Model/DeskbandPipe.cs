using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lenovo.WiFi.Client.Model
{
    public enum DeskbandCommand { ICS_Loading, ICS_on, ICS_off, ICS_clientconnected }

    public class DeskbandPipe
    {
        private const string PIPENAME = "LenovoWiFi";
        private NamedPipeServerStream PipeSvrStream = null;
        private StreamReader SReader = null;
        private StreamWriter SWriter = null;
        private IDeskbandPipeListener PipeListener = null;

        public DeskbandPipe()
        { 
        }

        public void Dispose()
        {
            if (PipeStream.IsConnected)
            {
                PipeStream.Disconnect();
            }
        }

        protected NamedPipeServerStream PipeStream
        {

            get
            {
                if (null == PipeSvrStream)
                {
                    PipeSvrStream = new NamedPipeServerStream(PIPENAME, PipeDirection.InOut, 1, PipeTransmissionMode.Message);
                }

                return PipeSvrStream;
            }
        }

        protected StreamReader Reader
        {
            get
            {
                if(null == SReader)
                {
                    SReader = new StreamReader(PipeStream, Encoding.Unicode);
                }

                return SReader;
            }
        }

        protected StreamWriter Writer
        {
            get
            {
                if (null == SWriter)
                {
                    SWriter = new StreamWriter(PipeStream, Encoding.Unicode);
                }

                return SWriter;
            }
        }

        public void RegisterListener(IDeskbandPipeListener listener)
        {
            PipeListener = listener;
        }

        public bool Start()
        {
            if (null == PipeListener) return false;

            bool exit = false;

            PipeStream.WaitForConnection();

            while (true)
            {
                try
                {
                    var line = Reader.ReadLine();
                    switch (line)
                    {
                        case "mouseenter":
                            PipeListener.OnMouseEnter();
                            break;
                        case "mouseleave":
                            PipeListener.OnMouseLeave();
                            break;
                        case "lbuttonclick":
                            PipeListener.OnLButtonClick();
                            break;
                        case "rbuttonclick":
                            PipeListener.OnRButtonClick();
                            break;
                        case "exit":
                            PipeListener.OnExit();
                            exit = true;
                            break;
                        default:
                            //log
                            break;
                    }

                    if (exit)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    //log ex.string.
                    return false;
                }
                //finally
                //{
                //    if (PipeStream.IsConnected)
                //    {
                //        PipeStream.Disconnect();
                //    }
                //}
            }

            return true;
        }


        public bool SendCommandToDeskband(DeskbandCommand cmd)
        {

            try
            {
                if( PipeStream.IsConnected )
                {
                    switch(cmd)
                    {
                        case DeskbandCommand.ICS_Loading:
                            Writer.WriteLine("ics_loading");

                            break;
                        case DeskbandCommand.ICS_on:
                            Writer.WriteLine("ics_on");

                            break;
                        case DeskbandCommand.ICS_off:
                            Writer.WriteLine("ics_off");

                            break;
                        case DeskbandCommand.ICS_clientconnected:
                            Writer.WriteLine("ics_clientconnected");

                            break;
                        default:
                            break;
                    }
                }
            }catch(Exception ex)
            {
                return false;
            }

            return true;
        }
    }
}
