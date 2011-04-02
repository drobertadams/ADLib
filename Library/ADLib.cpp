#include <SPI.h>
#include <Ethernet.h>
#include <ADLib.h>

//#define DEBUG 1

// Function pointer array for each data type
ADLIB_GENERIC_HANDLER_FUN dataTypeHandlers[4];

// Register data handlers
void ADLib_registerErrorHandler(ADLIB_ERROR_HANDLER_FUN f) {
    dataTypeHandlers[Error] = (ADLIB_GENERIC_HANDLER_FUN) f;
}

void ADLib_registerTextHandler(ADLIB_TEXT_HANDLER_FUN f) {
    dataTypeHandlers[Text] = (ADLIB_GENERIC_HANDLER_FUN) f;
}

void ADLib_registerNumericHandler(ADLIB_NUMERIC_HANDLER_FUN f) {
    dataTypeHandlers[Numeric] = (ADLIB_GENERIC_HANDLER_FUN) f;
}

void ADLib_registerNormalizedHandler(ADLIB_NORMALIZED_HANDLER_FUN f) {
    dataTypeHandlers[Normalized] = (ADLIB_GENERIC_HANDLER_FUN) f;
}

//==================================================================================================================================
// Initialize the Ethernet server library
Server ADLib_Server(ADLIB_TCP_PORT);

int useSerial;  // boolean: should we read from the network or from the serial connection?

void ADLib_StartNetwork(byte mac[], byte ip[], byte subnet[], byte gateway[]) {
  
  // Initialize the ethernet device
  Ethernet.begin(mac, ip);

  // Start listening for clients
  ADLib_Server.begin();
  
  useSerial = 0;
}

//==================================================================================================================================
// Initialize serial communication with the client.
void ADLib_StartSerial() {
    useSerial = 1; // we use NULL to indicate the need to use serial communcation in ADLib_ReadNetworkData()
   // Serial.begin(9600);
}

//==================================================================================================================================
void ADLib_Parse(int isBlocking) {

  // Setup buffer to store incoming bytes
  char buffer[ADLIB_BUFFER_SIZE] = {0};

  // Read data from network
  int networkReadStatus = ADLib_ReadData(buffer, isBlocking);

  // If network read had issues, return
  if (networkReadStatus != 0) {
    #ifdef DEBUG
    Serial.println("Issue with network read");
    #endif
    
    return;
  }
  
  #ifdef DEBUG
  Serial.println(buffer);
  #endif

  // Parse bufer

  char *ch;
    
  // First byte should be the device id
  ch = strtok(buffer, ADLIB_DEFAULT_DELIMETER);

  struct ADLib_MessageHeader messageHeader;
  
  messageHeader.deviceID = atoi(ch);
  
  // Verify device ID is within valid range 0 - ADLIB_MAX_DEVICE_ID
  if (messageHeader.deviceID < 0 || messageHeader.deviceID >= ADLIB_MAX_DEVICE_ID)
    return;
  
  ch = strtok(NULL, ADLIB_DEFAULT_DELIMETER);

  messageHeader.dataType = atoi(ch);
  
  #ifdef DEBUG
  Serial.print("DeviceID: ");
  Serial.println(messageHeader.deviceID);
  Serial.print("Data type: ");
  Serial.println(messageHeader.dataType);
  #endif

  switch (messageHeader.dataType) {

    // Numeric data type
    case Numeric: {
  
      struct ADLib_NumericDataType numericDataType;

      ch = strtok(NULL, ADLIB_DEFAULT_DELIMETER);
      numericDataType.minValue = atof(ch);

      ch = strtok(NULL, ADLIB_DEFAULT_DELIMETER);
      numericDataType.maxValue = atof(ch);

      ch = strtok(NULL, ADLIB_DEFAULT_DELIMETER);
      numericDataType.currentValue = atof(ch);

      ch = strtok(NULL, ADLIB_DEFAULT_DELIMETER);
      numericDataType.deltaValue = atof(ch);

      #ifdef DEBUG
      Serial.println("Numeric datatype");
      Serial.print("Min    : ");
      Serial.println(numericDataType.minValue, 5);
      Serial.print("Max    : ");
      Serial.println(numericDataType.maxValue, 5);
      Serial.print("Current: ");
      Serial.println(numericDataType.currentValue, 5);
      Serial.print("Delta  : ");
      Serial.println(numericDataType.deltaValue, 5);
      #endif
      
      // Call numeric datatype handler if configured
      if (dataTypeHandlers[messageHeader.dataType] != NULL)
        (*dataTypeHandlers[messageHeader.dataType])(&numericDataType);
      
      break;
    }

    case Normalized: {

      struct ADLib_NormalizedDataType normalizedDataType;

      ch = strtok(NULL, ADLIB_DEFAULT_DELIMETER);
      normalizedDataType.value = atof(ch);
      
      #ifdef DEBUG
      Serial.println("Normalized datatype: ");
      Serial.print("Value: ");
      Serial.println(normalizedDataType.value, 5);
      #endif

      // Call error or text datatype handler if configured
      if (dataTypeHandlers[messageHeader.dataType] != NULL)
        (*dataTypeHandlers[messageHeader.dataType])(&normalizedDataType);
      
      break;
    }

    case Text:
    case Error: {

      struct ADLib_TextDataType textDataType;

      ch = strtok(NULL, ADLIB_DEFAULT_DELIMETER);

      textDataType.text = ch;
      
      #ifdef DEBUG
      Serial.println("Text datatype: ");
      Serial.print("Value: ");
      Serial.println(textDataType.text);
      #endif
      
      // Call error or text datatype handler if configured
      if (dataTypeHandlers[messageHeader.dataType] != NULL)
        (*dataTypeHandlers[messageHeader.dataType])(&textDataType);
      
      break;
    }
  }    
}

//==================================================================================================================================
// Library function for reading data from either the network or the serial line.
// Returns 0 success.
int ADLib_ReadData(char *buffer, int isBlocking) {
    if (useSerial)
        return ADLib_ReadSerialData(buffer, isBlocking);
    else
        return ADLib_ReadNetworkData(buffer, isBlocking);
}

//==================================================================================================================================
// Library function for reading data from network
// Returns 0 success.
int ADLib_ReadNetworkData(char *buffer, int isBlocking) {

  #ifdef DEBUG
  Serial.println("In ReadNetworkData.");
  #endif
      
  int bufferCounter = 0; // Initialize buffer counter
  boolean clientNotConnected = true;

  do {

    // Listen for incoming clients
    Client client = ADLib_Server.available();
  
    if (client) {
      #ifdef DEBUG
      Serial.println("Client connected.");
      #endif
      
      clientNotConnected = false;

      // Read the incoming message
      while (client.connected()) {
        if (client.available()) {

          // Read the bytes incoming from the client
          char c = client.read();

          buffer[bufferCounter++] = c;

          if (bufferCounter >= ADLIB_BUFFER_SIZE - 1) {
            // Close the connection:
            client.stop();
            
            #ifdef DEBUG
            Serial.print("Buffer full at ");
            Serial.print(bufferCounter);
            Serial.println(" bytes. Connection closed");
            #endif

            return -1;
          }
        }
      }

      if (bufferCounter < ADLIB_BUFFER_SIZE) {

        #ifdef DEBUG
        Serial.print("Buffer at ");
        Serial.print(bufferCounter);
        Serial.println(" bytes.");
        #endif
            
        // Close the connection:
        client.stop();

        delay(1);
        
        return 0;
      }
    }
  } while(isBlocking);
}

//==================================================================================================================================
// Library function for reading data from serial
// Returns 0 success.
int ADLib_ReadSerialData(char *buffer, int isBlocking) {
  #ifdef DEBUG
  Serial.println("In ReadSerialData.");
  #endif
      
  int bufferCounter = 0; // Initialize buffer counter

  do {

      if (Serial.available()) {
          // Read the bytes incoming from the client
          char c = Serial.read();
          
          // Read until we get a \0. Serial.available() isn't accurate enough. The arduino can poll
          // the serial line faster than it can receive data. Consequently, available() can return
          // false in the middle of a message. Therefore, we keep reading until the end of the message
          // is received.
          while ( c != 0 ) {

              buffer[bufferCounter++] = c;

              if (bufferCounter >= ADLIB_BUFFER_SIZE - 1) {
            
                #ifdef DEBUG
                Serial.print("Buffer full at ");
                Serial.print(bufferCounter);
                Serial.println(" bytes.");
                #endif

                return -1;
              }
              
              do {
                  c = Serial.read();
              } while (c == -1);
          }
      }

      if (bufferCounter > 0 && bufferCounter < ADLIB_BUFFER_SIZE) {

        #ifdef DEBUG
        Serial.print("Buffer at ");
        Serial.print(bufferCounter);
        Serial.println(" bytes.");
        #endif

        delay(1);
        
        return 0;
      }
  } while(isBlocking);
}
