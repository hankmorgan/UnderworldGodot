#!/usr/bin/env python3
"""Generates a synthetic .SYS font for the render test.

CI cannot use the shipped fonts, so this fixture carries the three properties
that matter, all in glyph 'A':
  - ink past column 7, which only decodes correctly with a 2-byte row stride
  - an asymmetric row, so MSB-first and LSB-first disagree
  - a declared width larger than the header's maxwidth, which must not be clipped
"""
import struct, pathlib

CHARSIZE, HEIGHT, ROWBYTES, MAXWIDTH, BLANK = 4, 2, 2, 3, 2
COUNT = 128

def main():
    buf = bytearray(12 + COUNT * (CHARSIZE + 1))
    struct.pack_into("<6H", buf, 0, 1, CHARSIZE, BLANK, HEIGHT, ROWBYTES, MAXWIDTH)
    for c in range(COUNT):
        off = 12 + c * (CHARSIZE + 1)
        buf[off + CHARSIZE] = BLANK          # default: blank glyph, advance 2
    off = 12 + ord("A") * (CHARSIZE + 1)
    #        row 0: 1010 0000  0000 1100   -> cols 0,2 and 12,13
    #        row 1: 0000 0001  1000 0000   -> cols 7 and 8
    buf[off + 0], buf[off + 1] = 0xA0, 0x0C
    buf[off + 2], buf[off + 3] = 0x01, 0x80
    buf[off + CHARSIZE] = 13                 # wider than MAXWIDTH 3, within the 16-px cell
    out = pathlib.Path(__file__).with_name("stride_msb_wide.SYS")
    out.write_bytes(bytes(buf))
    print("wrote %s (%d bytes)" % (out, len(buf)))

if __name__ == "__main__":
    main()
