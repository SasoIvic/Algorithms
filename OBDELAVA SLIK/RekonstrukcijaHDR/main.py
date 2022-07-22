import math
import os
import cv2
import numpy as np
from PIL import Image
from PIL.ExifTags import TAGS
from matplotlib import pyplot

FILE = "primer2"
USE_LIBRARY = False
NUM_PTS = 200
SHOW_RES = True
LAMBDA_SMOOTHNESS = 1


def getExifField(exif, field):
    for (key, val) in exif.items():
        if TAGS.get(key) == field:
            return val


def readImages():
    imgs = []
    ets = []
    for root, dirs, files in os.walk(FILE):
        for name in files:
            image = Image.open(FILE + "/" + name)
            img = pyplot.imread(FILE + "/" + name)

            # Slike pretvorimo v uint8 format
            if img.dtype == np.float32:
                img = np.uint8(img * 255)

            if img.dtype != np.uint8:
                raise Exception('wrong image data type')

            ets.append(getExifField(image._getexif(), 'ExposureTime'))
            imgs.append(img)

    return np.array(imgs), np.array(ets, dtype=np.float32)


def selectPts(im_list, pts=100):
    """ Izbere naključne točke v sliki in vrne seznam točk. """
    H, W = im_list[0].shape[:2]
    ptsH = np.random.randint(0, H, size=(pts,))
    ptsW = np.random.randint(0, W, size=(pts,))

    pts_list = []
    for im in im_list:
        im_pts_val = im[ptsH, ptsW]
        pts_list.append(im_pts_val)

    return pts_list


def estimateCameraInvSensFunction(pixelValues, exposureTimes, smoothing, weighting):
    """ Oceni inverzno funkcijo senzitivnosti kamere iz izbranih vrednosti slike in časov izpostavitve.

    pixel_values - numpy.ndarray, NxM
        N - število slik.
        M - število izbranih točk.

        Vsak stolpec predstavlja vrednosti iste točke v različnih slikah.
        Vsaka vrstica predstavlja vrednosti različnih točk iz iste slike.

    exposure_times - numpy.ndarray, Nx1
        Časi izpostavitve za posamezne slike v vrsticah pixel_values.
    """

    if pixelValues.ndim != 2:
        raise Exception('Pricakujem 2D podatke - NxM.')

    N, M = pixelValues.shape
    Z = pixelValues
    addArraySize = 0

    if smoothing:
        addArraySize += 256-4

    Ag = np.zeros((N * M + addArraySize + 1, 256))  # matrika vrednosti slike
    AE = np.zeros((N * M + addArraySize + 1, M))  # matrika tock slike
    t = np.zeros((N * M + addArraySize + 1, 1))  # vektor casov zajemanja

    # uporaba uteži (Welch-ovo okno)
    if weighting:
        for n in range(0, N):  # za vsako sliko
            for m in range(0, M):  # preko vseh izbranih tock
                w = 1 - pow((Z[n, m] - 256/2)/(256/2), 2)
                Ag[n * M + m, Z[n, m]] = 1 * w
                AE[n * M + m, m] = -1 * w
                t[n * M + m] = np.log(exposureTimes[n]) * w
    else:
        for n in range(0, N):  # za vsako sliko
            for m in range(0, M):  # preko vseh izbranih tock
                Ag[n*M+m, Z[n, m]] = 1
                AE[n*M+m, m] = -1
                t[n*M+m] = np.log(exposureTimes[n])

    # dodatna izboljšava (vrednost 128 naj bo enaka 0)
    Ag[N*M, 128] = 1
    t[N*M] = 0
    k = N*M+1

    # glajenje funkcije odziva
    if smoothing:
        # Central difference: f'''(x) = -0.5*f(x-2) + 1*f(x-1) - 0*f(x) - 1*f(x+1) + 0.5*f(x+2)
        for i in range(2, 254):
            w = 1 - pow((i - 256/2)/(256/2), 2)
            Ag[k, i-2] = -0.5 * LAMBDA_SMOOTHNESS * w
            Ag[k, i-1] = 1 * LAMBDA_SMOOTHNESS * w
            Ag[k, i] = 0
            Ag[k, i+1] = -1 * LAMBDA_SMOOTHNESS * w
            Ag[k, i+2] = 0.5 * LAMBDA_SMOOTHNESS * w
            k += 1

    # sestavimo podmatriki A_g (vrednosti slike) in A_E (tocke slike)
    A = np.hstack((Ag, AE))

    pyplot.imshow(A)
    pyplot.savefig("matrixA.png", bbox_inches="tight", pad_inches=0, dpi=2000)

    # resim sistem enacb z lstsq - least squares solver/solution
    X = np.linalg.lstsq(A, t, rcond=None)[0]

    # izluscim samo koeficiente inverza senzitivne funkcije kamere
    g = X[:256].ravel()

    return g


def convertImgToLogIntensity(imgList, time, invSensFunc):
    return [invSensFunc[im] - np.log(t) for im, t in zip(imgList, time)]


def show_list_of_images(img_list, normalize=False, R=None, C=None, title=None, subtitles=None):
    """
    Prikaz serije slik v enem oknu.

    img_list - list, serija slik
    normalize - bool
        Podane slike se nromalizirajo, tako da se poišče minimalna in
        maksimalna vrednost preko vseh slik. Ti dve se nato uporabita
        za normalizacijo vsake slike: img=(img-min)/(max-min)
    R, C - int
        Število vrstic (R) in stolpcev (C) za prikaz z pyplot.subplots. V kolikor
        nista podana, sta izračunana tako, da se slike optimalno
        porazdelijo v oknu.
    title - str
        Naslov za celotno okno - figure.suptitle
    subtitles - list of str
        Naslov za vsako sliko, prikazani z axes.set_title.

    Vrne
        fig, ax_list
        Referenco na figure in axes, kot jih vrne pyplot.subplots.
        Tabela ax_list je preoblikovana - linearizirana.
    """
    if normalize:
        v_min = np.min(img_list)
        v_max = np.max(img_list)
    else:
        v_min = 0
        if img_list[0].dtype == np.uint8:
            v_max = 255.
        else:
            v_max = 1

    if R is None or C is None:
        R = int(len(img_list) ** 0.5)
        C = int(np.ceil(len(img_list) / R))

    fig, ax_list = pyplot.subplots(R, C)
    ax_list = ax_list.reshape(-1)

    for n, img in enumerate(img_list):
        img_norm = (img - v_min) / (v_max - v_min)

        ax_list[n].imshow(img_norm)
        ax_list[n].axis('off')
        if subtitles is not None:
            ax_list[n].set_title(subtitles[n])

    for n in range(n + 1, len(ax_list)):
        ax_list[n].axis('off')

    if title:
        fig.suptitle(title)

    return fig, ax_list


def calculateWelch():
    welchWeights = np.arange(pow(2, 8), dtype=float)
    welchWeights = 1 - pow((welchWeights - 256/2)/(256/2), 2)
    return welchWeights


def reconstructionWithWeights(images, imgLogIntList):
    recImg = np.zeros(shape=imgLogIntList[0].shape, dtype=np.float64)
    recWeightSum = np.zeros(shape=imgLogIntList[0].shape, dtype=np.float64)

    weights = calculateWelch()

    for i in range(len(images)):
        recImg += weights[images[i]] * imgLogIntList[i]
        recWeightSum += weights[images[i]]

    # No zero division
    recWeightSum[recWeightSum == 0] = 1

    recImg /= recWeightSum
    recImg = np.exp(recImg)
    recImg /= recImg.max()

    return recImg


def main():
    images, exposureTimes = readImages()

    if USE_LIBRARY:
        # Align input images
        alignMTB = cv2.createAlignMTB()
        alignMTB.process(images, images)

        # Obtain Camera Response Function (CRF)
        calibrateDebevec = cv2.createCalibrateDebevec()
        responseDebevec = calibrateDebevec.process(images, exposureTimes)

        # Merge images into an HDR linear image
        mergeDebevec = cv2.createMergeDebevec()
        hdrDebevec = mergeDebevec.process(images, exposureTimes, responseDebevec)

        # Save HDR image
        cv2.imwrite("HDR_library.hdr", hdrDebevec)

    else:
        pointsList = np.array(selectPts(images, NUM_PTS))
        pointsList = pointsList.reshape(len(images), NUM_PTS * 3)

        # dobimo funkcijo g (inverz fukcije obcutljivosti kamere --> opisuje pretvorbo intenzitete v vrednost piksla)
        camInvSens = estimateCameraInvSensFunction(pointsList, exposureTimes, smoothing=True, weighting=True)

        # z g() lahko rekonstruiramo Ei ... dobimo log(Ei) --> svetlost točk v sceni
        imgLogIntList = convertImgToLogIntensity(images, exposureTimes, camInvSens)
        imgIntList = [np.exp(img) for img in imgLogIntList]  # Ei

        # rekonstrukcija z utežmi
        imgHDR = reconstructionWithWeights(images, imgLogIntList)

        if SHOW_RES:
            img_labels = [f'exp. {t * 1e3:.0f} ms' for t in exposureTimes]

            show_list_of_images([imgHDR, np.minimum(imgHDR * 8, 1), np.minimum(imgHDR * 16, 1), np.minimum(imgHDR * 32, 1)], title='reconst. hdr slika\n$E_{i,rec}$', subtitles=[f'img*{p}' for p in [1, 8, 16, 32]])
            show_list_of_images(imgIntList, normalize=True, title='slike intenzitete\n$E_i$', subtitles=img_labels)
            show_list_of_images(imgLogIntList, normalize=True, title='slike intenzitete\n$log(E_i)$', subtitles=img_labels)

            pyplot.figure()
            pyplot.plot(camInvSens)
            pyplot.xlabel('vrednost piksla \n$Z_{ij}$')
            pyplot.ylabel('intenziteta svetlobe \n$E_i \Delta t_j$')
            pyplot.title('inverz senzitivnosti kamere \n$g(v)$')
            pyplot.show()

            show_list_of_images(images, title='original', subtitles=img_labels)

        # save images as 16 bit png
        imgHDR = np.clip(imgHDR*pow(2, 16), 0, pow(2, 16)).astype('uint16')
        cv2.imwrite('HDR_myProgram.png', cv2.cvtColor(imgHDR, cv2.COLOR_RGB2BGR))
        cv2.imwrite("HDR_myProgram.hdr", cv2.cvtColor(imgHDR, cv2.COLOR_RGB2BGR))


if __name__ == "__main__":
    main()
