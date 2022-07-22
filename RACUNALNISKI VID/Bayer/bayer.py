import numpy as np

def decimacija(red, green, green2, blue):
    green[::] = (green[::] + green2[::]) / 2
    rgb = np.zeros((red.shape[0], red.shape[1], 3))
    rgb[::, ::, 0] = red
    rgb[::, ::, 1] = green
    rgb[::, ::, 2] = blue
    return np.uint8(rgb)


def interpolacija(slika_bayer, vzorec, red, green1, green2, blue):
    rgb = np.zeros((slika_bayer.shape[0], slika_bayer.shape[1], 3))

    # zrcaljenje slike
    if vzorec == "BGGR":
        slika_bayer = slika_bayer[::-1, ::-1]
    elif vzorec == "GBRG":
        slika_bayer = slika_bayer[::-1]
    elif vzorec == "GRBG":
        slika_bayer = slika_bayer[:, ::-1]

    # RGGB
    # sredina - modra
    # rdeči kanal - 0
    rgb[1:-2:2, 1:-2:2, 0] = (slika_bayer[:-3:2, :-3:2] + slika_bayer[:-3:2, 2:-1:2] + slika_bayer[2:-1:2, :-3:2] + slika_bayer[2:-1:2, 2:-1:2])/4
    # zeleni kanal - 1
    rgb[1:-2:2, 1:-2:2, 1] = (slika_bayer[:-3:2, 1:-2:2] + slika_bayer[1:-2:2, :-3:2] + slika_bayer[1:-2:2, 2:-1:2] + slika_bayer[2:-1:2, 1:-2:2])/4
    # modri kanal - 2
    rgb[1:-2:2, 1:-2:2, 2] = slika_bayer[1:-2:2, 1:-2:2]

    # sredina - zelena2
    # rdeči kanal - 0
    rgb[1:-1:2, 2:-1:2, 0] = (slika_bayer[:-2:2, 2:-1:2] + slika_bayer[2:-1:2, 2:-1:2]) / 2
    # zeleni kanal - 1
    rgb[1:-1:2, 2:-1:2, 1] = slika_bayer[1:-2:2, 2:-1:2]
    # modri kanal - 2
    rgb[1:-1:2, 2:-1:2, 2] = (slika_bayer[1:-2:2, 1:-1:2] + slika_bayer[1:-2:2, 3::2]) / 2

    # sredina - zelena
    # rdeči kanal - 0
    rgb[2:-1:2, 1:-1:2, 0] = (slika_bayer[2::2, :-2:2] + slika_bayer[2::2, 2::2])/2
    # zeleni kanal - 1
    rgb[2:-1:2, 1:-1:2, 1] = slika_bayer[2:-1:2, 1:-1:2]
    # modri kanal - 2
    rgb[2:-1:2, 1:-1:2, 2] = (slika_bayer[1:-2:2, 1:-1:2] + slika_bayer[3::2, 1:-2:2]) / 2

    # sredina - rdeča
    # rdeči kanal - 0
    rgb[2:-1:2, 2:-1:2, 0] = slika_bayer[2:-1:2, 2:-1:2]
    # zeleni kanal - 1
    rgb[2:-1:2, 2:-1:2, 1] = (slika_bayer[2:-1:2, 1:-1:2] + slika_bayer[1:-1:2, 2:-1:2] + slika_bayer[2:-1:2, 3::2] + slika_bayer[3::2, 2:-1:2])/4
    # modri kanal - 2
    rgb[2:-1:2, 2:-1:2, 2] = (slika_bayer[1:-2:2, 1:-2:2] + slika_bayer[1:-2:2, 3::2] + slika_bayer[3::2, 1:-2:2] + slika_bayer[3::2, 3::2])/4

    # robovi rdeča (levo)
    # rdeči kanal - 0
    rgb[2:-1:2, 0, 0] = slika_bayer[2:-1:2, :1:1]
    # zeleni kanal - 1
    rgb[2:-1:2, 0, 1] = (slika_bayer[1:-2:2, :1:1] + slika_bayer[3::2, :1:1] + slika_bayer[2:-1:2, 1:2:1]) / 3
    # modri kanal -2
    rgb[2:-1:2, 0, 2] = (slika_bayer[1:-2:2, 1:2:1] + slika_bayer[3::2, 1:2:1]) / 2

    # robovi zelena (levo)
    # rdeči kanal - 0
    rgb[1:-2:2, 0, 0] = (slika_bayer[:-3:2, 0] + slika_bayer[2:-1:2, 0]) / 2
    # zeleni kanal - 1
    rgb[1:-2:2, 0, 1] = slika_bayer[1:-2:2, 0]
    # modri kanal - 2
    rgb[1:-2:2, 0, 2] = slika_bayer[1:-2:2, 1]

    # robovi rdeča (zgoraj)
    # rdeči kanal - 0
    rgb[0, 2:-1:2, 0] = slika_bayer[:1:1, 2:-1:2]
    # zeleni kanal - 1
    rgb[0, 2:-1:2, 1] = (slika_bayer[:1:1, 1:-2:2] + slika_bayer[:1:1, 3::2] + slika_bayer[1:2:1, 2:-1:2]) / 3
    # modri kanal - 2
    rgb[0, 2:-1:2, 2] = (slika_bayer[1:2:1, 1:-2:2] + slika_bayer[1:2:1, 3::2]) / 2

    # robovi zelena (zgoraj)
    # rdeči kanal - 0
    rgb[0, 1:-2:2, 0] = (slika_bayer[0, :-3:2] + slika_bayer[0, 2:-1:2]) / 2
    # zeleni kanal - 1
    rgb[0, 1:-2:2, 1] = slika_bayer[0, 1:-2:2]
    # modri kanal - 2
    rgb[0, 1:-2:2, 2] = slika_bayer[1, 1:-2:2]

    # robovi modra (desno)
    # rdeči kanal - 0
    rgb[1:-2:2, -1, 0] = (slika_bayer[:-3:2, -2] + slika_bayer[2:-1:2, -2]) / 2
    # zeleni kanal - 1
    rgb[1:-2:2, -1, 1] = (slika_bayer[:-3:2, -1] + slika_bayer[1:-2:2, -2] + slika_bayer[2:-1:2, -1]) / 3
    # modri kanal - 2
    rgb[1:-2:2, -1, 2] = slika_bayer[1:-2:2, -1]

    # robovi zelena (desno)
    # rdeči kanal - 0
    rgb[2:-1:2, -1, 0] = slika_bayer[2:-1:2, -2]
    # zeleni kanal - 1
    rgb[2:-1:2, -1, 1] = slika_bayer[2:-1:2, -1]
    # modri kanal - 2
    rgb[2:-1:2, -1, 2] = (slika_bayer[1:-2:2, -1] + slika_bayer[3::2, -1]) / 2

    # robovi modra (spodaj)
    # rdeči kanal - 0
    rgb[-1, 1:-2:2, 0] = (slika_bayer[-2, :-3:2] + slika_bayer[-2, 2:-1:2]) / 2
    # zeleni kanal - 1
    rgb[-1, 1:-2:2, 1] = (slika_bayer[-1, :-3:2] + slika_bayer[-2, 1:-2:2] + slika_bayer[-1, 2:-1:2]) / 3
    # modri kanal - 2
    rgb[-1, 1:-2:2, 2] = slika_bayer[-1, 1:-2:2]

    # robovi zelena (spodaj)
    # rdeči kanal - 0
    rgb[-1, 2:-1:2, 0] = slika_bayer[-2, 2:-1:2]
    # zeleni kanal - 1
    rgb[-1, 2:-1:2, 1] = slika_bayer[-1, 2:-1:2]
    # modri kanal - 2
    rgb[-1, 2:-1:2, 2] = (slika_bayer[-1, 1:-2:2] + slika_bayer[-1, 3::2]) / 2

    # kot rdeča (zgornji kot)
    # rdeči kanal - 0
    rgb[0, 0, 0] = slika_bayer[0, 0]
    # zeleni kanal - 1
    rgb[0, 0, 1] = (slika_bayer[0, 1] + slika_bayer[1, 0]) / 2
    # modri kanal - 2
    rgb[0, 0, 2] = slika_bayer[1, 1]

    # kot zelena (zgornji kot)
    # rdeči kanal - 0
    rgb[0, -1, 0] = slika_bayer[0, -2]
    # zeleni kanal - 1
    rgb[0, -1, 1] = slika_bayer[0, -1]
    # modri kanal - 2
    rgb[0, -1, 2] = slika_bayer[1, -1]

    # kot zelena (spodnji kot)
    # rdeči kanal - 0
    rgb[-1, 0, 0] = slika_bayer[-2, 0]
    # zeleni kanal - 1
    rgb[-1, 0, 1] = slika_bayer[-1, 0]
    # modri kanal - 2
    rgb[-1, 0, 2] = slika_bayer[-1, 1]

    # kot modra (spodnji kot)
    # rdeči kanal - 0
    rgb[-1, -1, 0] = slika_bayer[-2, -2]
    # zeleni kanal - 1
    rgb[-1, -1, 1] = (slika_bayer[-2, -1] + slika_bayer[-1, -2]) / 2
    # modri kanal - 2
    rgb[-1, -1, 2] = slika_bayer[-1, -1]

    # zrcaljenje nove slike
    if vzorec == "BGGR":
        rgb = rgb[::-1, ::-1]
    elif vzorec == "GBRG":
        rgb = rgb[::-1]
    elif vzorec == "GRBG":
        rgb = rgb[:, ::-1]

    return np.uint8(rgb)


def bayer_v_rgb(slika_bayer, vzorec, interpoliraj=False):
    red = []
    green = []
    green2 = []
    blue = []

    # da ne pride do overlowa spremenimo v uint16
    slika_bayer = slika_bayer.astype(np.uint16)

    if vzorec == "RGGB":
        red = slika_bayer[::2, ::2]
        green = slika_bayer[::2, 1::2]
        green2 = slika_bayer[1::2, ::2]
        blue = slika_bayer[1::2, 1::2]
    elif vzorec == "GBRG":
        red = slika_bayer[1::2, ::2]
        green = slika_bayer[::2, ::2]
        green2 = slika_bayer[1::2, 1::2]
        blue = slika_bayer[::2, 1::2]
    elif vzorec == "BGGR":
        red = slika_bayer[1::2, 1::2]
        green = slika_bayer[::2, 1::2]
        green2 = slika_bayer[1::2, ::2]
        blue = slika_bayer[::2, ::2]
    elif vzorec == "GRBG":
        red = slika_bayer[::2, 1::2]
        green = slika_bayer[::2, ::2]
        green2 = slika_bayer[1::2, 1::2]
        blue = slika_bayer[1::2, ::2]

    if not interpoliraj:
        return decimacija(red, green, green2, blue)
    else:
        return interpolacija(slika_bayer, vzorec, red, green, green2, blue)
