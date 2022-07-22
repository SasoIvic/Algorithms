from collections import deque

import cv2
import numpy as np
import copy
from PIL import Image
import time


IMAGE_PATH = "Primer4/indexEVI.tif"
OPERATION = "openR"
S = 9
NAME = "S=" + str(S)
WINDOW_SIZE = S * 2 + 1


def transform(img, operation):
    img = cv2.copyMakeBorder(np.array(img), S, S, S, S, cv2.BORDER_REFLECT)

    if operation == "erosion":
        return np.min(np.lib.stride_tricks.sliding_window_view(img, [WINDOW_SIZE, WINDOW_SIZE]).reshape((
            width, height, WINDOW_SIZE * WINDOW_SIZE
        )), axis=2)
    if operation == "dilation":
        return np.max(np.lib.stride_tricks.sliding_window_view(img, [WINDOW_SIZE, WINDOW_SIZE]).reshape((
            width, height, WINDOW_SIZE * WINDOW_SIZE
        )), axis=2)

    if operation == "erosionR":
        return np.maximum(
            np.min(np.lib.stride_tricks.sliding_window_view(img, [WINDOW_SIZE, WINDOW_SIZE]).reshape((
                width, height, WINDOW_SIZE * WINDOW_SIZE
            )), axis=2),
            image
        )
    if operation == "dilationR":
        return np.minimum(
            np.max(np.lib.stride_tricks.sliding_window_view(img, [WINDOW_SIZE, WINDOW_SIZE]).reshape((
                width, height, WINDOW_SIZE * WINDOW_SIZE
            )), axis=2),
            image
        )


image = np.array(Image.open(IMAGE_PATH).convert('L'))
width, height = image.shape
newImage = None

iterations = 0
start = time.time()

if OPERATION == "dilation":
    newImage = transform(image, "dilation")

elif OPERATION == "erosion":
    newImage = transform(image, "erosion")

elif OPERATION == "open":
    newImage = transform(image, "erosion")
    newImage = transform(newImage, "dilation")

elif OPERATION == "close":
    newImage = transform(image, "dilation")
    newImage = transform(newImage, "erosion")

elif OPERATION == "openR":
    newImage = transform(image, "erosion")
    S = 1
    WINDOW_SIZE = S * 2 + 1

    while True:
        tmp = copy.deepcopy(newImage)
        newImage = transform(newImage, "dilationR")
        iterations += 1

        print(iterations)

        if np.array_equal(newImage, tmp):
            break

elif OPERATION == "closeR":
    newImage = transform(image, "dilation")
    S = 1
    WINDOW_SIZE = S * 2 + 1

    while True:
        tmp = copy.deepcopy(newImage)
        newImage = transform(newImage, "erosionR")
        iterations += 1

        print(iterations)

        if np.array_equal(newImage, tmp):
            break

elif OPERATION == "subtract":
    image1 = np.array(Image.open("Primer3/indexNDWI.tif").convert('L'))
    image2 = np.array(Image.open("Primer3/closeR_S=8_Image.tif").convert('L'))

    newImage = image2 - image1

end = time.time()

im = Image.fromarray(newImage)
im.save(OPERATION + "_" + str(NAME) + '_Image.tif')

print("\n" + OPERATION)
print("Time [s]: ", end-start)
print("Iterations: ", iterations)
