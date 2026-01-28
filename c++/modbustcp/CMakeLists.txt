cmake_minimum_required(VERSION 3.15)
project(modbus_tcp LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)

add_library(modbus_tcp SHARED
    src/modbus_tcp.cpp
    src/socket_win.cpp
    src/socket_posix.cpp
)

target_include_directories(modbus_tcp PUBLIC include)

if (WIN32)
    target_link_libraries(modbus_tcp ws2_32)
endif()
