# UW1 test fixtures

The `SOUNDS.DAT` and `UW.AD` files from an Ultima Underworld 1 install belong
here but are **not committed** — they're copyright-protected game data.

Tests that require the fixtures skip with a clear message when they're absent.
To run those tests, copy the files from your own UW1 install:

```sh
cp /path/to/UW1/SOUND/SOUNDS.DAT ./SOUNDS.DAT
cp /path/to/UW1/SOUND/UW.AD ./UW.AD
```

The tools under `tools/` (SfxWav, psmitty_reference) locate these via their
own config or symlinks — see their respective READMEs.
