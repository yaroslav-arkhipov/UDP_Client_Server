
In the corresponding folders are the UDP client and server. The configuration of applications is configured from the config files that are located in folders with the corresponding applications. The source code is attached to the project and is located in the root folder.

- For correct operation, it is necessary that the config files are in the same directory as the application, otherwise the settings will be assigned default values.
- The sending and listening ports of the client and the server must match, for correct operation, in the config-file the application port is in the tags - <port> send or receive port </port>. If there are several tags in the config-file port - there will be the last found value inside the configuration file in the corresponding tags.
- The IP addresses in the config files are enclosed in the <ip> tags in the format - XXXX.XXX.XXX.XXX </ip>. Multicast addresses are configured by adding additional addresses to the config file in the corresponding tags. All addresses recorded in the config file in the <ip> </ip> tags will be processed and messages will be sent to these addresses. The client can also process all IP addresses entered in the config file, but will receive data from the first IP address from which it will receive data.
- <lower_range> integer digit </lower_range> and <upper_range> integer digit </upper_range> these tags are used only in the server config-file for setting the range of random numbers generation.
- to set the delay in the client application, use the tag <delay> integer digit </delay>

The server config file has the following format:
? xml version = "1.0" encoding = "utf-8"?>
server>
  port> 4000 /port>
  ip> 127.0.0.1 /ip>
  lower_range> 10 /lower_range>
  upper_range> 1,000,000 /upper_range>
/server>


The client config file has the following format:
? xml version = "1.0" encoding = "utf-8"?>
client>
  port> 4000 /port>
  ip> 127.0.0.1 </ip>
  delay> 0 /delay>
/client>
