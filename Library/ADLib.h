/*
 * ADLib_.h - Ambient Display Library
 *
 *  Created on: Feb 19, 2011
 *      Author: Russ Shearer
 *     License: 
 *
 */
 
#ifndef ADLib_h
#define ADLib_h

/* User modifiable variables */
#define ADLIB_TCP_PORT            2000    // User definded TCP port
#define ADLIB_DEFAULT_DELIMETER   "\v"    // Character delemter used from incoming network message. MUST MATCH SENDER
/* User modifiable variables */

#define ADLIB_BUFFER_SIZE         300     // Size of network read buffer
#define ADLIB_MAX_DEVICE_ID       255     // Max number of devices. A sanity check. Zero is not used.
#define ADLIB_MAX_TEXT_LENGTH     255     // Max size of any text message
#define ADLIB_BLOCKING            1       // Blocking/Non-blocking network read
#define ADLIB_NOT_BLOCKING        0

struct ADLib_MessageHeader {
  int deviceID;
  int dataType;
};

struct ADLib_NumericDataType {
  double minValue;
  double maxValue;
  double currentValue;
  double deltaValue;
};

struct ADLib_NormalizedDataType {
  double value;
};

struct ADLib_TextDataType {
  char *text;
};

// Data types
enum ADLib_DataTypes {
  Error       = 0,
  Text        = 1,
  Numeric     = 2,
  Normalized  = 3
};

// Typedefs to make working with function pointers readable
typedef void (*ADLIB_GENERIC_HANDLER_FUN)(void *);
typedef void (*ADLIB_ERROR_HANDLER_FUN)(struct ADLib_TextDataType *);
typedef void (*ADLIB_TEXT_HANDLER_FUN)(struct ADLib_TextDataType *);
typedef void (*ADLIB_NUMERIC_HANDLER_FUN)(struct ADLib_NumericDataType *);
typedef void (*ADLIB_NORMALIZED_HANDLER_FUN)(struct ADLib_NormalizedDataType *);

// In setup() call either of these to initialize how you want to communicate with the client.
void ADLib_StartNetwork(byte mac[], byte ip[], byte subnet[], byte gateway[]);  // Library function for starting networking
void ADLib_StartSerial();                                                       // Library function for starting serial communication

void ADLib_Parse(int isBlocking);                                               // Library function parsing network stream
void ADLib_registerErrorHandler(ADLIB_ERROR_HANDLER_FUN f);                     // Library function for registering Error data handler
void ADLib_registerTextHandler(ADLIB_TEXT_HANDLER_FUN f);                       // Library function for registering Text data handler
void ADLib_registerNumericHandler(ADLIB_NUMERIC_HANDLER_FUN f);                 // Library function for registering Numeric data handler
void ADLib_registerNormalizedHandler(ADLIB_NORMALIZED_HANDLER_FUN f);           // Library function for registering Normalized data handler

// Private functions
int ADLib_ReadData(char *buffer, int isBlocking);                               // Generic interface for reading data
int ADLib_ReadNetworkData(char *buffer, int isBlocking);                        // Library function for reading data from network
int ADLib_ReadSerialData(char *buffer, int isBlocking);                         // Library function for reading data from serial

#endif /* ADLib__h */