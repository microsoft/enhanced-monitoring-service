#!/usr/bin/env python
#
#Read kvp and display on screen
#

import io

def read_bytes(pool):
    filepath = "/var/lib/hyperv/.kvp_pool_" + str(pool)
    mode = 'r'
    c = None
    try:
        with open(filepath, mode) as F:
            c=F.read()
            F.close()
    except IOError, e:
        print e
    b = bytes(c)
    return b

def bytes_to_str(b, start, end):
    buf = None 
    for i in range(start, end):
        if ord(b[i]) == 0:#Found string end
            buf = bytearray(i - start)
            buf[:] = b[start : i]
            break
    if(buf):
        return buf.decode('utf-8')
    else:
        return None

def read_raw_kvp(pool):
    b = read_bytes(pool)
    key_start = 0
    val_start = 512
    key_len = 512
    val_len = 2048
    step = key_len + val_len
    data={}
    while val_start < len(b):
        key = bytes_to_str(b, key_start, key_start + key_len)
        val = bytes_to_str(b, val_start, val_start + val_len)
        data[key] = val
        key_start += step
        val_start += step
    return data;

def write_bytes(pool, b):
    filepath = "/var/lib/hyperv/.kvp_pool_" + str(pool)
    mode = 'wb'
    c = None
    try:
        with open(filepath, mode) as F:
            c=F.write(b)
            F.close()
    except IOError, e:
        print e
