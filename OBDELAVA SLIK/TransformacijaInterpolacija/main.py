from matplotlib import pyplot as plt
import matplotlib.image as img
import numpy as np
import cv2
import math
from PIL import Image
from scipy.ndimage.interpolation import map_coordinates


def interpolation(img, map, interpolationType = "BilinearInterpolation"):
    matrix = np.zeros(shape=map[0].shape)

    if interpolationType == "NearestNeighbourInterpolation":
        print("Nearest Neighbour Interpolation")
        x = np.rint(map[0]).astype(int)
        y = np.rint(map[1]).astype(int)
        matrix = img[x,y][:,1]

    elif interpolationType == "BilinearInterpolation":
        print("Bilinear Interpolation")

        # Closest smaller point
        x = np.floor(map[0]).astype(int)
        y = np.floor(map[1]).astype(int)

        # Closest bigger point
        x_ = np.ceil(map[0]).astype(int)
        y_ = np.ceil(map[1]).astype(int)

        # x-axis interpolation
        weight = map[0]-x
        left_x = img[x,y][:,1] * (1-weight) + img[x_,y][:,1] * weight
        right_x = img[x_,y_][:,1] * weight + img[x,y_][:,1] * (1-weight)

        # y-axis interpolation
        weight = map[1]-y
        matrix = left_x * (1-weight) + right_x * weight

    return matrix


def defaultMapping(w, h, g):
    # y coordinates
    mappingY = []
    for y in np.arange(0, h, g):
        mappingY.append(y)
    mappingY = np.repeat(mappingY, w/g)

    # x coordinates
    mappingX = []
    for i in np.arange(0, h, g):
        for x in np.arange(0, w, g):
            mappingX.append(x)

    return np.array([mappingX, mappingY])


def drawPointsOnImage(image, matrix):
    plt.imshow(image)
    plt.plot(matrix[0], matrix[1], ',', markerfacecolor='red')
    plt.axis('off')
    plt.show()


def translate(matrix, translateX, translateY):
    for x in range(len(matrix[0])):
        matrix[0][x] = matrix[0][x] + translateX

    for y in range(len(matrix[1])):
        matrix[1][y] = matrix[1][y] + translateY

    return matrix


def rotate(matrix, angle):
    angle = math.radians(angle)
    rot = np.array([[math.cos(angle),math.sin(angle)],[-math.sin(angle),math.cos(angle)]])
    newMatrix = np.dot(rot, matrix)

    return newMatrix


originalImage = cv2.imread("image.jpg")
cv2.imshow('image', originalImage)

# gap nam pove kako daleč narazen se naj izrišejo točke (gap=0.5 pomeni 2 kratno povečavo)
width=120; height=100; gap=0.25
mapping = defaultMapping(width, height, gap)

mapping = translate(mapping, 10, 50)
mapping = rotate(mapping, 5)
drawPointsOnImage(originalImage, mapping)

# out = map_coordinates(originalImage[:,:,1], [mapping[1], mapping[0]], order=1)
out = interpolation(np.asarray(originalImage), [mapping[1], mapping[0]])
out = np.reshape(out, (int(height/gap), int(width/gap)))

outputImage = np.zeros([int(height/gap), int(width/gap), 3])
outputImage[:,:,0] = out
outputImage[:,:,1] = out
outputImage[:,:,2] = out

cv2.imwrite('interpolatedImage.png', outputImage)
cv2.imshow('interpolatedImage', outputImage/255)
cv2.waitKey(0)





