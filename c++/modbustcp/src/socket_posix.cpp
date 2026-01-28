#ifndef _WIN32
#include "socket.h"
#include <arpa/inet.h>
#include <unistd.h>
#include <sys/socket.h>

int socket_init() { return 0; }

socket_t socket_connect(const char* ip, int port) {
    int s = socket(AF_INET, SOCK_STREAM, 0);
    sockaddr_in addr{};
    addr.sin_family = AF_INET;
    addr.sin_port = htons(port);
    inet_pton(AF_INET, ip, &addr.sin_addr);

    if (connect(s, (sockaddr*)&addr, sizeof(addr)) != 0)
        return -1;

    return s;
}

int socket_send(socket_t s, const uint8_t* buf, int len) {
    return send(s, buf, len, 0);
}

int socket_recv(socket_t s, uint8_t* buf, int len) {
    return recv(s, buf, len, 0);
}

void socket_close(socket_t s) {
    close(s);
}
#endif
