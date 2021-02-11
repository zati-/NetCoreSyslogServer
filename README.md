# NetCore Syslog Server


## Simple Syslog Server in C#


## Watson Syslog Server

This project is based on Watson Syslog Server. 
Moved Watson Syslog Server to libSyslogServer for reference. 
Watson Syslog Server will automatically start using a default configuration listening on UDP/514 and storing log files in the ```logs\``` directory.  If you wish to change this, create a file called ```syslog.json``` with the following structure:
```
{
  "Version": "Watson Syslog Server v1.0.0",
  "UdpPort": 514,
  "DisplayTimestamps": true,
  "LogFileDirectory": "logs\\",
  "LogFilename": "log.txt",
  "LogWriterIntervalSec": 10
}
```

 
## Starting the Server

Build/compile and run the binary.

## Running under Windows Linux and MacOS

This application should work well in all NetCore environments. 
Tested on: 
 - Windows 10 
 - Linux (Ubuntu 20.04 LTS "Focal Fossa" x64)
 - OSX (10.13 "High Sierra")



## Running under Mono

This app should work well in Mono environments.  
It is recommended that when running under Mono, you execute the containing EXE using --server and after using the Mono Ahead-of-Time Compiler (AOT).
```
mono --aot=nrgctx-trampolines=8096,nimt-trampolines=8096,ntrampolines=4048 --server myapp.exe
mono --server myapp.exe
```


## Help or Feedback

Do you need help or have feedback?  
See the "issues" tab. 

## New in v2.0.0

- Dependency on NetCoreServer 
- Support for 10'000 concurrent connections 
- Support for TCP 
- Support for TLS (Note TLS 1.2 REQUIRED, TLS 1 only supported if you modify the source)
- Working Client for TLS (see https://github.com/ststeiger/SyslogNet)

## New in v1.0.0

- Initial release, support for UDP 
