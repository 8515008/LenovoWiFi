using System;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace Lenovo.WiFi.Client.Model
{
    internal class DeskbandPipeServer : IDisposable
    {
        private const string PipeName = "LenovoWiFi";

        private readonly NamedPipeServerStream _pipeServerStream;

        internal DeskbandPipeServer()
        {
            _pipeServerStream = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message);
        }

        public void Dispose()
        {
            _pipeServerStream.Dispose();
        }

        internal event EventHandler DeskbandMouseEnter;
        internal event EventHandler DeskbandMouseLeave;
        internal event EventHandler DeskbandLeftButtonClick;
        internal event EventHandler DeskbandRightButtonClick;
        internal event EventHandler DeskbandExit;
        internal event EventHandler HookFinished;

        internal void Start()
        {
            _pipeServerStream.WaitForConnection();

            using (var reader = new StreamReader(_pipeServerStream, Encoding.Unicode))
            {
                while (true)
                {
                    try
                    {
                        var line = reader.ReadLine();
                        var exit = false;

                        switch (line)
                        {
                            case "mouseenter":
                                OnDeskbandMouseEnter();
                                break;
                            case "mouseleave":
                                OnDeskbandMouseLeave();
                                break;
                            case "lbuttonclick":
                                OnDeskbandLeftButtonClick();
                                break;
                            case "rbuttonclick":
                                OnDeskbandRightButtonClick();
                                break;
                            case "exit":
                                OnDeskbandExit();
                                exit = true;
                                break;
                            case "handshake":
                                OnHookFinished();
                                break;
                            default:
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
                    }
                }
            }
        }

        internal void Send(string message)
        {
            if (!_pipeServerStream.IsConnected)
            {
                return;
            }

            using (var writer = new StreamWriter(_pipeServerStream, Encoding.Unicode, 1024, true))
            {
                try
                {
                    writer.WriteLine(message);
                }
                catch (Exception e)
                {
                    
                }
            }
        }

        internal void Stop()
        {
            if (_pipeServerStream.IsConnected)
            {
                _pipeServerStream.Disconnect();
            }
        }

        private void OnDeskbandMouseEnter()
        {
            if (DeskbandMouseEnter != null)
            {
                DeskbandMouseEnter(this, EventArgs.Empty);
            }
        }

        private void OnDeskbandMouseLeave()
        {
            if (DeskbandMouseLeave != null)
            {
                DeskbandMouseLeave(this, EventArgs.Empty);
            }
        }

        private void OnDeskbandLeftButtonClick()
        {
            if (DeskbandLeftButtonClick != null)
            {
                DeskbandLeftButtonClick(this, EventArgs.Empty);
            }
        }

        private void OnDeskbandRightButtonClick()
        {
            if (DeskbandRightButtonClick != null)
            {
                DeskbandRightButtonClick(this, EventArgs.Empty);
            }
        }

        private void OnDeskbandExit()
        {
            if (DeskbandExit != null)
            {
                DeskbandExit(this, EventArgs.Empty);
            }
        }

        private void OnHookFinished()
        {
            if (HookFinished != null)
            {
                HookFinished(this, EventArgs.Empty);
            }
        }
    }
}
