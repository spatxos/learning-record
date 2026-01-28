#pragma once
#include <stdint.h>

#if defined(_WIN32)
  #ifdef MODBUS_TCP_EXPORTS
    #define MODBUS_API __declspec(dllexport)
  #else
    #define MODBUS_API __declspec(dllimport)
  #endif
#else
  #define MODBUS_API __attribute__((visibility("default")))
#endif

#ifdef __cplusplus
extern "C" {
#endif

MODBUS_API int modbus_tcp_connect(
    const char* ip,
    int port,
    uint8_t slave_id
);

MODBUS_API int modbus_tcp_read_holding_registers(
    int handle,
    uint16_t start_addr,
    uint16_t count,
    uint16_t* out
);

MODBUS_API void modbus_tcp_close(int handle);

#ifdef __cplusplus
}
#endif
