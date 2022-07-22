import math
import cv2
import numpy as np
from matplotlib import pyplot as plt
import copy


ACTION = "SWAHE"
BIN_NUM = 150
SWAHE_WINDOW_SIZE = 3
CLAHE_WINDOW_SIZE = 32
CUT = 0.5
IMAGE_PATH = "Mona_Lisa_input.png"
INTERPOLATION = True


def combine_blocks_to_image(blocks, blocksInRow):
    image = None

    elInRowCounter = 0
    row = None

    for h in range(0, len(blocks)):

        elInRowCounter += 1

        if elInRowCounter == 1:
            row = blocks[h]

        elif elInRowCounter <= blocksInRow:
            row = np.concatenate((row, blocks[h]), axis=1)

        if elInRowCounter == blocksInRow:
            if image is None:
                image = row
            else:
                image = np.concatenate((image, row), axis=0)
            elInRowCounter = 0

    return image


def cutValues(ns, cut=1.0):
    maxVal = np.max(ns) * cut
    sumCuts = 0

    for i in range(len(ns)):
        if ns[i] >= maxVal:
            tmp = ns[i]
            ns[i] = maxVal
            sumCuts += tmp - ns[i]

    crumbs = sumCuts // len(ns)

    for i in range(len(ns)):
        ns[i] += crumbs

    return ns


def find_nearest(arr, val):
    arr = np.asarray(arr)
    idx = (np.abs(arr - val)).argmin()
    return idx


def interpolate(image, ns, br):
    nsWidth = int(np.floor(image.shape[1]/CLAHE_WINDOW_SIZE) - 1) if image.shape[1] % CLAHE_WINDOW_SIZE == 0 else int(np.floor(image.shape[1]/CLAHE_WINDOW_SIZE))
    nsHeight = int(np.floor(image.shape[0]/CLAHE_WINDOW_SIZE) - 1) if image.shape[0] % CLAHE_WINDOW_SIZE == 0 else int(np.floor(image.shape[0]/CLAHE_WINDOW_SIZE))
    ns = np.reshape(np.array(ns), (nsHeight + 1, nsWidth + 1, len(br)))

    for y in range(image.shape[0]):
        for x in range(image.shape[1]):
            # region Red region
            if y < CLAHE_WINDOW_SIZE//2:
                if x < CLAHE_WINDOW_SIZE//2:
                    # print("red region 1")
                    image[y][x] = ns[0][0][find_nearest(br, image[y][x])]
                    continue
                if x >= image.shape[1] - CLAHE_WINDOW_SIZE//2:
                    # print("red region 2")
                    image[y][x] = ns[0][nsWidth][find_nearest(br, image[y][x])]
                    continue
            if y >= image.shape[0] - CLAHE_WINDOW_SIZE//2:
                if x < CLAHE_WINDOW_SIZE//2:
                    # print("red region 3")
                    image[y][x] = ns[nsHeight][0][find_nearest(br, image[y][x])]
                    continue
                if x >= image.shape[1] - CLAHE_WINDOW_SIZE//2:
                    # print("red region 4")
                    image[y][x] = ns[nsHeight][nsWidth][find_nearest(br, image[y][x])]
                    continue

            # endregion

            if x % CLAHE_WINDOW_SIZE == 0:
                correction1w = np.ceil(CLAHE_WINDOW_SIZE/2) * -1
                correction2w = np.floor(CLAHE_WINDOW_SIZE/2)
            elif (x/CLAHE_WINDOW_SIZE) - (x//CLAHE_WINDOW_SIZE) <= 0.5 and x > CLAHE_WINDOW_SIZE//2:
                correction1w = np.ceil(CLAHE_WINDOW_SIZE/2) * -1
                correction2w = np.ceil(CLAHE_WINDOW_SIZE/2) * -1
            else:
                correction1w = np.floor(CLAHE_WINDOW_SIZE/2)
                correction2w = np.floor(CLAHE_WINDOW_SIZE/2)

            if y % CLAHE_WINDOW_SIZE == 0:
                correction1h = np.ceil(CLAHE_WINDOW_SIZE/2) * -1
                correction2h = np.floor(CLAHE_WINDOW_SIZE/2)
            elif (y/CLAHE_WINDOW_SIZE) - (y//CLAHE_WINDOW_SIZE) <= 0.5 and y > CLAHE_WINDOW_SIZE//2:
                correction1h = np.ceil(CLAHE_WINDOW_SIZE/2) * -1
                correction2h = np.ceil(CLAHE_WINDOW_SIZE/2) * -1
            else:
                correction1h = np.floor(CLAHE_WINDOW_SIZE/2)
                correction2h = np.floor(CLAHE_WINDOW_SIZE/2)

            # region Green region
            if y < CLAHE_WINDOW_SIZE//2 and (CLAHE_WINDOW_SIZE//2 <= x <= image.shape[1] - CLAHE_WINDOW_SIZE//2):
                # print("green region - upper")
                leftX = int(np.floor((x/CLAHE_WINDOW_SIZE)) * CLAHE_WINDOW_SIZE + correction1w)
                rightX = int(np.ceil(x/CLAHE_WINDOW_SIZE) * CLAHE_WINDOW_SIZE + correction2w)

                if rightX >= image.shape[1]:
                    rightX = image.shape[1] - 1

                # print("\t ", leftX, " ", rightX)
                weight = math.fabs(x-leftX) / CLAHE_WINDOW_SIZE
                leftValue = ns[0][leftX//CLAHE_WINDOW_SIZE][find_nearest(br, image[y][x])]
                rightValue = ns[0][rightX//CLAHE_WINDOW_SIZE][find_nearest(br, image[y][x])]
                image[y][x] = int(leftValue * (1-weight) + rightValue * weight)
                continue

            if y >= image.shape[0] - CLAHE_WINDOW_SIZE//2 and (CLAHE_WINDOW_SIZE//2 <= x <= image.shape[1] - CLAHE_WINDOW_SIZE//2):
                # print("green region - lower")
                leftX = int(np.floor(x/CLAHE_WINDOW_SIZE) * CLAHE_WINDOW_SIZE + correction1w)
                rightX = int(np.ceil(x/CLAHE_WINDOW_SIZE) * CLAHE_WINDOW_SIZE + correction2w)

                if rightX >= image.shape[1]:
                    rightX = image.shape[1] - 1

                # print("\t ", leftX, " ", rightX)
                weight = math.fabs(x-leftX) / CLAHE_WINDOW_SIZE
                leftValue = ns[nsHeight][leftX//CLAHE_WINDOW_SIZE][find_nearest(br, image[y][x])]
                rightValue = ns[nsHeight][rightX//CLAHE_WINDOW_SIZE][find_nearest(br, image[y][x])]
                image[y][x] = int(leftValue * (1-weight) + rightValue * weight)
                continue

            if x < CLAHE_WINDOW_SIZE//2 and (CLAHE_WINDOW_SIZE//2 <= y <= image.shape[0] - CLAHE_WINDOW_SIZE//2):
                # print("green region - left")
                upperY = int(np.floor(y/CLAHE_WINDOW_SIZE) * CLAHE_WINDOW_SIZE + correction1h)
                lowerY = int(np.ceil(y/CLAHE_WINDOW_SIZE) * CLAHE_WINDOW_SIZE + correction2h)

                if lowerY >= image.shape[0]:
                    lowerY = image.shape[0] - 1

                # print("\t ", upperY, " ", lowerY)
                weight = math.fabs(y-upperY) / CLAHE_WINDOW_SIZE
                upperValue = ns[upperY//CLAHE_WINDOW_SIZE][0][find_nearest(br, image[y][x])]
                lowerValue = ns[lowerY//CLAHE_WINDOW_SIZE][0][find_nearest(br, image[y][x])]
                image[y][x] = int(upperValue * (1-weight) + lowerValue * weight)
                continue

            if x >= image.shape[1] - CLAHE_WINDOW_SIZE//2 and (CLAHE_WINDOW_SIZE//2 <= y <= image.shape[0] - CLAHE_WINDOW_SIZE//2):
                # print("green region - right")
                upperY = int(np.floor(y/CLAHE_WINDOW_SIZE) * CLAHE_WINDOW_SIZE + correction1h)
                lowerY = int(np.ceil(y/CLAHE_WINDOW_SIZE) * CLAHE_WINDOW_SIZE + correction2h)

                if lowerY >= image.shape[0]:
                    lowerY = image.shape[0] - 1

                # print("\t ", upperY, " ", lowerY)
                weight = math.fabs(y-upperY) / CLAHE_WINDOW_SIZE
                upperValue = ns[upperY//CLAHE_WINDOW_SIZE][nsWidth][find_nearest(br, image[y][x])]
                lowerValue = ns[lowerY//CLAHE_WINDOW_SIZE][nsWidth][find_nearest(br, image[y][x])]
                image[y][x] = int(upperValue * (1-weight) + lowerValue * weight)
                continue

            # endregion

            # region Blue region
            # print("blue region")

            leftX = int(np.floor(x/CLAHE_WINDOW_SIZE) * CLAHE_WINDOW_SIZE + correction1w)
            rightX = int(np.ceil(x/CLAHE_WINDOW_SIZE) * CLAHE_WINDOW_SIZE + correction2w)

            if rightX >= image.shape[1]:
                rightX = image.shape[1] - 1

            upperY = int(np.floor(y/CLAHE_WINDOW_SIZE) * CLAHE_WINDOW_SIZE + correction1h)
            lowerY = int(np.ceil(y/CLAHE_WINDOW_SIZE) * CLAHE_WINDOW_SIZE + correction2h)

            if lowerY >= image.shape[0]:
                lowerY = image.shape[0] - 1

            # x-axis interpolation
            weight = math.fabs(x-leftX) / CLAHE_WINDOW_SIZE

            leftValueUpper = ns[upperY//CLAHE_WINDOW_SIZE][leftX//CLAHE_WINDOW_SIZE][find_nearest(br, image[y][x])]
            rightValueUpper = ns[upperY//CLAHE_WINDOW_SIZE][rightX//CLAHE_WINDOW_SIZE][find_nearest(br, image[y][x])]

            leftValueLower = ns[lowerY//CLAHE_WINDOW_SIZE][leftX//CLAHE_WINDOW_SIZE][find_nearest(br, image[y][x])]
            rightValueLower = ns[lowerY//CLAHE_WINDOW_SIZE][rightX//CLAHE_WINDOW_SIZE][find_nearest(br, image[y][x])]

            upperValInterp = leftValueUpper * (1-weight) + rightValueUpper * weight
            lowerValInterp = leftValueLower * (1-weight) + rightValueLower * weight

            # y-axis interpolation
            weight = math.fabs(y-upperY) / CLAHE_WINDOW_SIZE
            image[y][x] = np.round((upperValInterp * (1-weight) + lowerValInterp * weight))

            # endregion

    return image


def histogramEqualization(lum, clahe=False):
    hist, br = np.histogram(lum, bins=BIN_NUM, range=(0, 255))
    br = br[:-1]

    if CUT != 1:
        hist = cutValues(hist, CUT)

    # Cumulative sum
    cs = hist.cumsum()

    # Normalized sum
    n = cs.max() - cs.min()
    m = lum.max()
    csMin = cs.min()
    ns = (((cs-csMin) / n) * m).astype('uint8')

    # Create new intensity matrix
    if not clahe:
        for i in range(lum.shape[0]):
            for j in range(lum.shape[1]):
                index = find_nearest(br, lum[i][j])
                lum[i][j] = ns[index]

    return lum, ns, br


def simpleImageToHistogram(lum):
    plt.figure(1)
    plt.hist(lum.flat, bins=BIN_NUM, range=(0, 255))
    plt.title("Histogram before equalization")

    # Histogram equalization (basic)
    equ, ns, br = histogramEqualization(lum)
    plt.figure(2)
    plt.hist(equ.flat, bins=BIN_NUM, range=(0, BIN_NUM))
    plt.title("Histogram after equalization")

    ns = cutValues(ns, CUT)
    plt.figure(3)
    # plt.hist(ns, bins=BIN_NUM, range=(0, BIN_NUM))
    plt.bar(list(range(0, BIN_NUM)), ns)
    plt.title("Histogram normalized cumulative sum")
    plt.show()

    return equ


def SWAHE(lum):
    window = None
    calculatedCenterPoints = []
    lum = lum.astype('int32')
    lum = np.pad(lum, pad_width=int(SWAHE_WINDOW_SIZE/2), mode='constant', constant_values=-1)
    width, height = lum.shape

    # Window sliding through image
    for i in range(width-SWAHE_WINDOW_SIZE+1):
        for j in range(height-SWAHE_WINDOW_SIZE+1):
            if width-i-SWAHE_WINDOW_SIZE > 0 and height-SWAHE_WINDOW_SIZE-j > 0:
                window = lum[i:SWAHE_WINDOW_SIZE+i, j:SWAHE_WINDOW_SIZE+j]

            if not width-i-SWAHE_WINDOW_SIZE > 0 and not height-SWAHE_WINDOW_SIZE-j > 0:
                window = lum[i:, j:]
            else:
                if not width-SWAHE_WINDOW_SIZE-i > 0:
                    window = lum[i:, j:SWAHE_WINDOW_SIZE+j]
                if not height-SWAHE_WINDOW_SIZE-j > 0:
                    window = lum[i:SWAHE_WINDOW_SIZE+i, j:]

            dpcp = copy.deepcopy(window)
            equ = histogramEqualization(dpcp)[0]
            calculatedCenterPoints.append(equ[int(SWAHE_WINDOW_SIZE/2)][int(SWAHE_WINDOW_SIZE/2)])

    return calculatedCenterPoints


def CLAHE(lum):
    normalizedSums = []
    equalizedBlocks = []

    blocks = [lum[x:x+CLAHE_WINDOW_SIZE, y:y+CLAHE_WINDOW_SIZE] for x in range(0, lum.shape[0], CLAHE_WINDOW_SIZE) for y in range(0, lum.shape[1], CLAHE_WINDOW_SIZE)]

    for block in blocks:
        equ, ns, binRanges = histogramEqualization(block, INTERPOLATION)
        equalizedBlocks.append(equ)
        normalizedSums.append(ns)

    if INTERPOLATION:
        image = interpolate(lum, normalizedSums, binRanges)
    else:
        image = combine_blocks_to_image(equalizedBlocks, math.ceil(lum.shape[1]/CLAHE_WINDOW_SIZE))

    return image


# -------------------- main ----------------------- #
img = cv2.imread(IMAGE_PATH, 1)

# Converting image to LAB, to get the luminance channel
lab_img = cv2.cvtColor(img, cv2.COLOR_BGR2LAB)
luminance, a, b = cv2.split(lab_img)

if ACTION == "SimpleHistogramEqualization" or ACTION == "":
    equalized = simpleImageToHistogram(luminance)

    updated_lab_img = cv2.merge((equalized, a, b))
    histEq_img = cv2.cvtColor(updated_lab_img, cv2.COLOR_LAB2BGR)

    cv2.imshow("Original image", img)
    cv2.imshow("Histogram equalization", histEq_img)
    cv2.waitKey(0)

if ACTION == "SWAHE" or ACTION == "":
    equalized = SWAHE(luminance)
    equalized = np.array(equalized).reshape(luminance.shape)
    equalized = equalized.astype('uint8')

    updated_lab_img = cv2.merge((equalized, a, b))
    SWAHE_img = cv2.cvtColor(updated_lab_img, cv2.COLOR_LAB2BGR)

    #cv2.imshow("Original image", img)
    #cv2.imshow("SWAHE", SWAHE_img)
    plt.imshow(SWAHE_img)
    plt.show()
    #cv2.waitKey(0)

if ACTION == "CLAHE" or ACTION == "":
    equalized = CLAHE(luminance)

    updated_lab_img = cv2.merge((equalized, a, b))
    CLAHE_img = cv2.cvtColor(updated_lab_img, cv2.COLOR_LAB2BGR)

    cv2.imshow("Original image", img)
    cv2.imshow("CLAHE", CLAHE_img)
    cv2.waitKey(0)
