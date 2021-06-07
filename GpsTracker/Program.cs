/*
 * MIT License
 *
 * Copyright (c) 2021 John Nahlen
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using NmeaParser;
using System;
using System.IO.Ports;
using System.Threading;
using Timer = System.Timers.Timer;

namespace GpsTracker
{
    internal static class Program
    {
        private const int DifferenceThresholdMeters = 33;
        private static readonly GpsTrackerConfig GpsTrackerConfigInstance = new GpsTrackerConfig();

        private static GpsPosition _lastPosition;
        private static GpsPosition _lastPositionUploaded;

        private static void Main(string[] args)
        {
            Console.WriteLine("GpsTracker EXE 1.0");
            Console.WriteLine("Copyright (c) 2021 John Nahlen");
            Console.WriteLine();

            if (args.Length == 1 && args[0].Equals("--list"))
            {
                foreach (var availablePortName in SerialPort.GetPortNames())
                {
                    Console.WriteLine(availablePortName);
                }

                Environment.Exit(0);
            }

            if (args.Length < 4)
            {
                Console.WriteLine("Arguments: <portName> <deviceName> <uploadUrl> <intervalSeconds>");
                Environment.Exit(1);
            }

            var portName = args[0].Trim();
            var deviceName = args[1].Trim();
            var uploadUrl = args[2].Trim();
            if (!int.TryParse(args[3], out var interval))
            {
                interval = 60;
            }

            GpsTrackerConfigInstance.SerialPortName = portName;
            GpsTrackerConfigInstance.DeviceName = deviceName;
            GpsTrackerConfigInstance.UploadUrl = uploadUrl;
            GpsTrackerConfigInstance.Interval = interval * 1000;

            const int baudRate = 4800;
            var port = new SerialPort(portName, baudRate);

            var device = new SerialPortDevice(port);
            device.MessageReceived += OnNmeaMessageReceived;
            device.OpenAsync();

            var timer = new Timer(GpsTrackerConfigInstance.Interval);
            timer.Elapsed += OnTimedEvent;
            timer.Enabled = true;

            Thread.Sleep(Timeout.Infinite);
        }

        private static bool ShouldUpload(GpsPosition gpsPosition)
        {
            return gpsPosition != null && (_lastPositionUploaded == null ||
                                           (gpsPosition.Altitude != 0 &&
                                            Utils.GetDistance(gpsPosition, _lastPositionUploaded) >=
                                            DifferenceThresholdMeters));
        }

        private static void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            if (ShouldUpload(_lastPosition))
            {
                DoUpload(_lastPosition);
            }
        }

        private static void DoUpload(GpsPosition gpsPosition)
        {
            try
            {
                var url = GpsTrackerConfigInstance.UploadUrl;
                UploadService.Upload(url, new
                {
                    deviceName = GpsTrackerConfigInstance.DeviceName,
                    latitude = gpsPosition.Latitude,
                    longitude = gpsPosition.Longitude,
                    altitude = gpsPosition.Altitude
                });
                _lastPositionUploaded = gpsPosition;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }

        private static void OnNmeaMessageReceived(object sender, NmeaMessageReceivedEventArgs args)
        {
            GpsPosition gpsPosition = null;
            if (args.Message is NmeaParser.Messages.Gga gga)
            {
                // Use double.IsNaN instead of == or != double.NaN
                // https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2242
                if (double.IsNaN(gga.Latitude) || double.IsNaN(gga.Longitude))
                {
                    return;
                }

                gpsPosition = GpsPositionFactory.FromGga(gga);
            }
            else if (args.Message is NmeaParser.Messages.Rmc rmc)
            {
                if (double.IsNaN(rmc.Latitude) || double.IsNaN(rmc.Longitude))
                {
                    return;
                }

                gpsPosition = GpsPositionFactory.FromRmc(rmc);
            }

            if (gpsPosition == null)
            {
                return;
            }

            if (_lastPosition != null && Utils.GetDistance(gpsPosition, _lastPosition) < DifferenceThresholdMeters)
            {
                return;
            }

            Console.WriteLine("{0:MM/dd/yyyy HH:mm:ss zzz} ({1},{2})", DateTime.Now, gpsPosition.Latitude,
                gpsPosition.Longitude);
            _lastPosition = gpsPosition;
        }
    }
}