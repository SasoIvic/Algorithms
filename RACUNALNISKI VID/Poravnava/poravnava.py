import numpy as np
import skimage.measure as measure
from scipy import ndimage


class ResiProcrustesRansac:
    def estimate(self, data):
        a = data[:, :2]  # x y
        b = data[:, 2:]  # x' y'
        self.R, self.T = resi_procrustes(a, b)

    def residuals(self, data):
        a = data[:, :2]
        b = data[:, 2:]
        c = a.dot(self.R) + self.T
        return (np.sum((b - c) ** 2, 1)) ** 0.5


def resi_procrustes(tocke_A, tocke_B):
    A_avg = np.mean(tocke_A, 0)
    B_avg = np.mean(tocke_B, 0)

    A_tilda = tocke_A - A_avg
    B_tilda = tocke_B - B_avg

    K = np.dot(A_tilda.transpose(), B_tilda)
    U, S, Vh = np.linalg.svd(K)
    Delta = np.diag(np.array(([1, np.linalg.det(np.dot(Vh.transpose(), U.transpose()))])))

    R = np.dot(U, np.dot(Delta, Vh))  # rotacija
    T = (B_avg - np.dot(A_avg, R)).reshape(1, 2)  # translacija

    return R, T


def getTocke(sivinska_slika, thresh):
    sobel_dx = ndimage.sobel(sivinska_slika, 1)
    sobel_dy = ndimage.sobel(sivinska_slika, 0)
    sobel_val = (sobel_dy ** 2 + sobel_dx ** 2) ** 0.5

    binary = sobel_val > thresh
    binary = np.where(binary == True)

    tocke = np.array(binary).transpose()
    return tocke


def shrani_barve(tocke, slika, original):

    n = tocke.shape[0]
    barve = np.zeros(shape=(n, 5))

    for i in range(0, n):
        x = original[i, 0]
        x_nov = tocke[i, 0]
        barve[i, 0] = x_nov

        y = original[i, 1]
        y_nov = tocke[i, 1]
        barve[i, 1] = y_nov

        barve[i, 2] = slika[x, y, 0]  # R
        barve[i, 3] = slika[x, y, 1]  # G
        barve[i, 4] = slika[x, y, 2]  # B

    return barve


def oceni_transformacijo_ICP(slika_A, slika_B, barve=False, ransac=False, max_itt=10):
    # gledamo samo prve 3 kanale in izračunamo sivinsko sliko
    sivinska_A = np.mean(slika_A[:, :, :3], 2)
    sivinska_B = np.mean(slika_B[:, :, :3], 2)

    tocke_A = getTocke(sivinska_A, 3)
    tocke_B = getTocke(sivinska_B, 3)

    original_A = tocke_A
    barve_B = np.zeros(shape=(tocke_B.shape[0], 5))

    if barve:
        barve_B = shrani_barve(tocke_B, slika_B, tocke_B)

        arr_barve = np.ndarray((tocke_A.shape[0], 3))
        arr_barve[:, :] = slika_B[tocke_A[:, 0], tocke_A[:, 1]]
        tocke_A = np.concatenate((tocke_A, arr_barve), axis=1)

        arr_barve = np.ndarray((tocke_B.shape[0], 3))
        arr_barve[:, :] = slika_A[tocke_B[:, 0], tocke_B[:, 1]]
        tocke_B = np.concatenate((tocke_B, arr_barve), axis=1)

    # ItterativeClosestPoint Algorithm
    D = np.zeros((tocke_A.shape[0], tocke_B.shape[0]))
    R = np.identity(2)  # rotacija
    T = np.zeros((1, 2))  # translacija

    for it in range(max_itt):
        for i in range(D.shape[0]):
            dx = tocke_A[i, 0] - tocke_B[:, 0]
            dy = tocke_A[i, 1] - tocke_B[:, 1]

            if barve:
                barve_A = shrani_barve(tocke_A, slika_A, original_A)
                r = (barve_A[i, 2] - barve_B[:, 2])
                g = (barve_A[i, 3] - barve_B[:, 3])
                b = (barve_A[i, 4] - barve_B[:, 4])
                D[i, :] = (dx / slika_A.shape[1]) ** 2 + (dy / slika_A.shape[0]) ** 2 + (r / 255) ** 2 + (g / 255) ** 2 + (b / 255) ** 2
            else:
                D[i, :] = dx ** 2 + dy ** 2

        minD = np.argmin(D, axis=1)
        tocke_B2 = tocke_B[minD]  # nove tocke (tocke iz b, ki so najblizje tockam iz a)

        if ransac:
            if barve:
                tmp = np.concatenate((tocke_A[:, 0:2], tocke_B2[:, 0:2]), axis=1)
            else:
                tmp = np.concatenate((tocke_A, tocke_B2), axis=1)

            model, inlines = measure.ransac(tmp, ResiProcrustesRansac, min_samples=10, residual_threshold=4, max_trials=1000)
            rotation = model.R
            translation = model.T
        else:
            rotation, translation = resi_procrustes(tocke_A[:, 0:2], tocke_B2[:, 0:2])

        if barve:
            tocke_A[:, 0:2] = tocke_A[:, 0:2].dot(rotation) + translation
        else:
            tocke_A = tocke_A.dot(rotation) + translation

        R = R.dot(rotation)
        T = T.dot(rotation) + translation

    return R.transpose(), np.flip(T)


def transformiraj_sliko(slika, rotacija, translacija):
    rotacija = np.flip(np.flip(rotacija, axis=0), axis=1)
    T = np.zeros([3, 3])
    T[0:2, 0:2] = rotacija
    T[0:2, 2] = translacija
    T[2, 2] = 1

    transformirana_slika = slika.copy()
    for i in [0, 1, 2]:
        transformirana_slika[::, ::, i] = ndimage.affine_transform(slika[::, ::, i], T)

    return transformirana_slika
