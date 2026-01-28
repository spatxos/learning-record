#ifdef _WIN32
#include "socket.h"
#include <ws2tcpip.h>

#pragma comment(lib, "ws2_32.lib")

int socket_init() {
    WSADATA wsa;
    return WSAStartup(MAKEWORD(2,2), &wsa);
}

socket_t socket_connect(const char* ip, int port) {
    SOCKET s = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    sockaddr_in addr{};
    addr.sin_family = AF_INET;
    addr.sin_port = htons(port);
    inet_pton(AF_INET, ip, &addr.sin_addr);

    if (connect(s, (sockaddr*)&addr, sizeof(addr)) != 0)
        return INVALID_SOCKET;

    return s;
}

int socket_send(socket_t s, const uint8_t* buf, int len) {
    return send(s, (const char*)buf, len, 0);
}

int socket_recv(socket_t s, uint8_t* buf, int len) {
    return recv(s, (char*)buf, len, 0);
}

void socket_close(socket_t s) {
    closesocket(s);
}
#endif
