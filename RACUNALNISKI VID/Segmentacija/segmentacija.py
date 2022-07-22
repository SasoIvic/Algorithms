import numpy as np
import skimage.measure as measure
from scipy import ndimage
import skimage.draw as draw
import math as math


def poisci_konture(slika, min_povrsina):
    # ojačaj robove
    img_guss = ndimage.gaussian_filter(slika, 2)
    img_guss = slika - img_guss

    # binarna slika robov
    img_bin = img_guss > 10

    # poišči konture (robove)
    contours = measure.find_contours(img_bin, 0.5)

    # algoritem vezalk (ehranimo eno mejo)
    filtered_contours = []
    for contour in contours:
        zg = 0
        sp = 0

        for i in range(len(contour) - 1):
            zg += contour[i][1] * contour[i + 1][0]
            sp += contour[i][0] * contour[i + 1][1]

        surface = 0.5 * (zg - sp)

        if surface > min_povrsina:
            filtered_contours.append(contour)

    return filtered_contours


def analiziraj_konture(konture):
    # dobi tocke znotraj konture
    contours_val = []
    for contour in konture:
        a, b = draw.polygon(contour[:, 0], contour[:, 1])
        contour_val = np.array([a, b]).T
        contours_val += [contour_val]

    res = np.zeros([len(konture), 5], float)
    index = 0

    for contour in contours_val:
        # center (težišče)
        x = res[index][0] = surov_moment(1, 0, contour) / surov_moment(0, 0, contour)
        y = res[index][1] = surov_moment(0, 1, contour) / surov_moment(0, 0, contour)

        # osi
        u00 = surov_moment(0, 0, contour)
        u01 = 0
        u10 = 0
        u11 = surov_moment(1, 1, contour) - x * surov_moment(0, 1, contour)
        u20 = surov_moment(2, 0, contour) - x * surov_moment(1, 0, contour)
        u02 = surov_moment(0, 2, contour) - y * surov_moment(0, 1, contour)
        u21 = surov_moment(2, 1, contour) - 2 * x * surov_moment(1, 1, contour) - y * surov_moment(2, 0, contour) + 2 * x * x * surov_moment(0, 1, contour)
        u12 = surov_moment(1, 2, contour) - 2 * y * surov_moment(1, 1, contour) - x * surov_moment(0, 2, contour) + 2 * y * y * surov_moment(1, 0, contour)
        u30 = surov_moment(3, 0, contour) - 3 * x * surov_moment(2, 0, contour) + 2 * x * x * surov_moment(1, 0, contour)
        u03 = surov_moment(0, 3, contour) - 3 * y * surov_moment(0, 2, contour) + 2 * y * y * surov_moment(0, 1, contour)

        # osi
        os_x = ((u20 + u02) / 2) + (math.sqrt(4 * (u11 ** 2) + (u20 - u02) ** 2) / 2)
        os_y = ((u20 + u02) / 2) - (math.sqrt(4 * (u11 ** 2) + (u20 - u02) ** 2) / 2)

        res[index][2] = (100 * os_x) / (os_x + os_y)
        res[index][3] = (100 * os_y) / (os_x + os_y)

        u_11 = u11 / u00
        u_20 = u20 / u00
        u_02 = u02 / u00

        # kot (orientacija)
        theta = 0.5 * math.atan((2 * u_11) / (u_20 - u_02))

        if u_02 > u_20:
            theta -= math.pi / 2

        # asimetričnost
        a = math.cos(theta * -1)
        b = math.sin(theta * -1) * -1

        u_30 = a ** 3 * u30 + 3 * a ** 2 * b * u21 + 3 * a * b ** 2 * u12 + b ** 3 * u03

        if u_30 < 0:
            theta -= math.pi

        res[index][4] = math.degrees(theta) + 270

        if res[index][4] < 0:
            res[index][4] += 360

        index += 1

    return res


def surov_moment(i, j, contour):
    moment = 0
    for point in contour:
        moment += point[0] ** i * point[1] ** j
    return moment
