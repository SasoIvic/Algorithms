from collections import deque
import PIL
import matplotlib.pyplot as plt
import cv2
import numpy as np
from PIL import Image, ImageTk
import time
from tkinter import *
from scipy.ndimage import gaussian_filter, distance_transform_edt
from skimage.filters import threshold_otsu
from skimage.measure import label
from skimage.morphology import binary_erosion
from skimage.segmentation import watershed, find_boundaries

IMAGE_PATH = "Primeri/primer3.jpg"
AUTOMATIC_MARKERS = False
SEARCH_LIGHT_PARTICLES = False
USE_LIBRARY = False


def get_x_and_y(event):
    global lasx, lasy
    lasx, lasy = event.x, event.y


def draw_smth(event):
    global lasx, lasy
    canvas.create_line((lasx, lasy, event.x, event.y), fill='red', width=2)
    lasx, lasy = event.x, event.y
    markersArray.append((lasx, lasy))


def createCanvas(image, width, height):
    app = Tk()
    app.geometry(str(width) + "x" + str(height))

    global canvas
    canvas = Canvas(app, bg='black')
    canvas.pack(anchor='nw', fill='both', expand=1)

    image = ImageTk.PhotoImage(image)
    canvas.create_image(0, 0, image=image, anchor='nw')

    canvas.bind("<Button-1>", get_x_and_y)
    canvas.bind("<B1-Motion>", draw_smth)

    app.mainloop()


class Watershed:
    MASK = -2
    WSHD = 0
    INIT = -1
    INQE = -3


def get_neighbours(x, y, image):
    neighbours = []
    # Save the neighbourhood pixel's values
    for i in range(-1, 2):
        for j in range(-1, 2):
            if (i == 0 and j == 0) or (x + i < 0 or y + j < 0) or (x + i >= image.shape[0] or y + j >= image.shape[1]):
                continue

            neighbours.append(((x + i), (y + j)))

    return neighbours


def myWatershed(image):
    # Initialization
    width, height = image.shape
    segmented_image = np.full((width, height), Watershed.INIT, np.int32)

    currentLabel = 0
    flag = 0
    levels = 256
    fifo = deque()

    # Get all the pixels
    intensity_list = []
    for x in range(image.shape[0]):
        for y in range(image.shape[1]):
            intensity_list.append((image[x][y], (x, y)))

    # Sort the list by intensities
    intensity_list.sort()

    # Get neighbouring pixels
    neighbours = {}
    for i in range(len(intensity_list)):
        x = intensity_list[i][1][0]
        y = intensity_list[i][1][1]

        neighbours[(x, y)] = np.array(get_neighbours(x, y, image))

    # Levels evenly spaced steps from minimum to maximum.
    levels = np.linspace(intensity_list[0][0], intensity_list[-1][0], levels)
    level_indices = []
    current_level = 0

    # Get the indices that delimit pixels with different values.
    for i in range(width * height):
        if intensity_list[i][0] > levels[current_level]:
            # Skip levels until the next highest one is reached.
            while intensity_list[i][0] > levels[current_level]:
                current_level += 1
            level_indices.append(i)

    level_indices.append(width * height)

    start_index = 0
    for stop_index in level_indices:
        # Mask all pixels at the current level.
        for p in intensity_list[start_index:stop_index]:
            p = p[1]
            segmented_image[p[0], p[1]] = Watershed.MASK
            # Initialize queue with neighbours of existing basins at the current level.
            for q in neighbours[p[0], p[1]]:
                if segmented_image[q[0], q[1]] >= Watershed.WSHD:
                    segmented_image[p[0], p[1]] = Watershed.INQE
                    fifo.append(p)
                    break

        while fifo:
            p = fifo.popleft()

            # Label the pixel by inspecting neighbours
            for q in neighbours[(p[0], p[1])]:
                lab_p = segmented_image[p[0], p[1]]  # pixel
                lab_q = segmented_image[q[0], q[1]]  # neighbour pixel

                # Pixel already belongs to basin
                if lab_q > 0:
                    if lab_p == Watershed.INQE or (lab_p == Watershed.WSHD and flag):
                        segmented_image[p[0], p[1]] = lab_q
                    elif lab_p > 0 and lab_p != lab_q:
                        segmented_image[p[0], p[1]] = Watershed.WSHD
                        flag = False

                elif lab_q == Watershed.WSHD:
                    if lab_p == Watershed.INQE:
                        segmented_image[p[0], p[1]] = Watershed.WSHD
                        flag = True

                elif lab_q == Watershed.MASK:
                    segmented_image[q[0], q[1]] = Watershed.INQE
                    fifo.append(q)

        # Detect and process new minima at the current level.
        for p in intensity_list[start_index:stop_index]:
            p = p[1]
            # p is inside a new minimum. Create a new label.
            if segmented_image[p[0], p[1]] == Watershed.MASK:
                currentLabel += 1
                fifo.append(p)
                segmented_image[p[0], p[1]] = currentLabel

                while fifo:
                    q = fifo.popleft()
                    for r in neighbours[q[0], q[1]]:
                        if segmented_image[r[0], r[1]] == Watershed.MASK:
                            fifo.append(r)
                            segmented_image[r[0], r[1]] = currentLabel

        start_index = stop_index

    return segmented_image


def imagePreprocessing(image):
    image = cv2.cvtColor(np.array(image), cv2.COLOR_RGB2BGR)
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)

    # Normalize the image
    gray -= gray.min()
    gray = gray / gray.max()

    # Smooth the image
    img_smoothed = gaussian_filter(gray, 2)

    # Perform Otsu's thresholding
    otsu_threshold = threshold_otsu(img_smoothed)

    if SEARCH_LIGHT_PARTICLES:
        image_otsu = img_smoothed > otsu_threshold
    else:
        image_otsu = img_smoothed < otsu_threshold

    # Erode the semantic segmentation result
    image_eroded = binary_erosion(image_otsu)

    # Apply the distance transform
    distance_transform = distance_transform_edt(image_eroded)
    distance_transform = np.max(distance_transform) - distance_transform
    distance_transform = gaussian_filter(distance_transform, 2)

    return img_smoothed, image_otsu, distance_transform


def main():
    image = PIL.Image.open(IMAGE_PATH)
    width, height = image.size

    if not AUTOMATIC_MARKERS:
        global markersArray
        markersArray = []
        createCanvas(image, width, height)

    img_smoothed, image_otsu, distance_transform = imagePreprocessing(image)

    # Apply the Watershed transform to the inverted image
    if USE_LIBRARY:
        ws_dist = watershed(distance_transform)
    else:
        ws_dist = myWatershed(distance_transform)

    # Label individual instances
    instance_dist = label(ws_dist * image_otsu)

    # Keep only marker boundaries
    if not AUTOMATIC_MARKERS:
        segments = []
        for marker in markersArray:
            segment = instance_dist[marker[1], marker[0]]

            if segment not in segments:
                segments.append(segment)

        for x in range(instance_dist.shape[0]):
            for y in range(instance_dist.shape[1]):
                if instance_dist[x, y] not in segments:
                    instance_dist[x, y] = 0

    # Obtain boundaries of each instance
    boundary_dist = find_boundaries(instance_dist)

    # Plot the results
    fig, axes = plt.subplots(2, 2, figsize=[20, 20])
    axes[0, 0].imshow(distance_transform, cmap='gray')
    axes[0, 0].set_title('Distance Map')

    axes[0, 1].imshow(ws_dist, cmap='coolwarm')
    axes[0, 1].set_title('Watershed result')

    axes[1, 0].imshow(instance_dist, cmap='coolwarm')
    axes[1, 0].set_title('Instance Segmentation')

    axes[1, 1].imshow(image)
    axes[1, 1].imshow(boundary_dist, alpha=0.35, cmap='coolwarm')
    axes[1, 1].set_title('Boundary Overlay')
    plt.show()

    plt.imsave("segmented.jpg", boundary_dist)


if __name__ == "__main__":
    start = time.time()

    main()

    end = time.time()
    print("Time [s]: ", end - start)
