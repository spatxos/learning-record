#define MODBUS_TCP_EXPORTS
#include "modbus_tcp.h"
#include "socket.h"
#include <map>

struct Context {
    socket_t sock;
    uint8_t slave;
    uint16_t tid;
};

static std::map<int, Context> ctxs;
static int next_handle = 1;

int modbus_tcp_connect(const char* ip, int port, uint8_t slave) {
    socket_init();
    socket_t s = socket_connect(ip, port);
#ifdef _WIN32
    if (s == INVALID_SOCKET) return -1;
#else
    if (s < 0) return -1;
#endif

    ctxs[next_handle] = { s, slave, 1 };
    return next_handle++;
}

int modbus_tcp_read_holding_registers(
    int handle, uint16_t start, uint16_t count, uint16_t* out
) {
    auto& c = ctxs[handle];

    uint8_t req[12] = {
        uint8_t(c.tid >> 8), uint8_t(c.tid),
        0,0, 0,6,
        c.slave, 0x03,
        uint8_t(start >> 8), uint8_t(start),
        uint8_t(count >> 8), uint8_t(count)
    };
    c.tid++;

    socket_send(c.sock, req, 12);

    uint8_t hdr[7];
    socket_recv(c.sock, hdr, 7);

    int len = (hdr[4] << 8) | hdr[5];
    uint8_t pdu[256];
    socket_recv(c.sock, pdu, len);

    for (int i = 0; i < count; i++)
        out[i] = (pdu[3+i*2] << 8) | pdu[4+i*2];

    return count;
}

void modbus_tcp_close(int handle) {
    socket_close(ctxs[handle].sock);
    ctxs.erase(handle);
}
