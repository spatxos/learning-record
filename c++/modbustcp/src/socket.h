#pragma once
#include <stdint.h>

#ifdef _WIN32
#include <winsock2.h>
using socket_t = SOCKET;
#else
using socket_t = int;
#endif

int socket_init();
socket_t socket_connect(const char* ip, int port);
int socket_send(socket_t s, const uint8_t* buf, int len);
int socket_recv(socket_t s, uint8_t* buf, int len);
void socket_close(socket_t s);
