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

using System;

namespace GpsTracker
{
    public static class GpsPositionFactory
    {
        public static GpsPosition FromGga(NmeaParser.Messages.Gga gga)
        {
            var gpsPosition = new GpsPosition
            {
                Latitude = Math.Round(gga.Latitude, 5),
                Longitude = Math.Round(gga.Longitude, 5),
                Altitude = gga.Altitude
            };
            return gpsPosition;
        }

        public static GpsPosition FromRmc(NmeaParser.Messages.Rmc rmc)
        {
            var gpsPosition = new GpsPosition
            {
                Latitude = Math.Round(rmc.Latitude, 5),
                Longitude = Math.Round(rmc.Longitude, 5)
            };
            // Altitude not available
            return gpsPosition;
        }
    }
}