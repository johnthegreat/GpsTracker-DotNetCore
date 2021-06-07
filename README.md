# GpsTracker

A C# / .NET Core port of https://github.com/johnthegreat/GpsTracker

## Notes

- Windows Security seems to (I believe) falsely report the generated binary as `Trojan:Win32/Wacatac.B!ml`. *Use at your own risk.*
- When running on Linux, serial port access seems to require `sudo`.
- Tested in both Visual Studio 2019 and JetBrains Rider.

## Dependencies

- Newtonsoft.Json
- SharpGIS.NmeaParser

## License

MIT License (See `LICENSE.txt`)