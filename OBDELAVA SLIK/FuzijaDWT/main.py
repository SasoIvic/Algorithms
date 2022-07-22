import cv2
import os
from numpy.lib.type_check import imag
import pywt
import scipy.ndimage as ndimage
import scipy.sparse as sps
import numpy as np
from matplotlib import pyplot

np.seterr(divide='ignore', invalid='ignore')

FILE = "Primer4"
FUSION_METHOD = "Advanced" # Advanced
IS_WINDOW = True
IS_WEIGHTED_FUSION = True
RESULTS = False

# region windows

window_1 = np.array([[0.1, 0.1, 0.1],
                        [0.1, 0.2, 0.1],
                        [0.1, 0.1, 0.1]])

window_2 = np.array([[0.1, 0.0, 0.1],
                        [0.0, 0.8, 0.0],
                        [0.1, 0.0, 0.1]])

window_3 = np.array([[0.1, 0.1, 0.2, 0.1, 0.1],
                        [0.1, 0.2, 0.4, 0.2, 0.1],
                        [0.2, 0.4, 0.6, 0.4, 0.2],
                        [0.1, 0.2, 0.4, 0.2, 0.1],
                        [0.1, 0.1, 0.2, 0.1, 0.1]])

# endregion


def show_dwt(dwt):
    L = len(dwt)-1
    
    fig, ax = pyplot.subplots(L+1, 3, figsize=(6, 6), constrained_layout=True)
    ax[0, 0].imshow(dwt[0]/dwt[0].max())
    ax[0, 0].set_title('Slika lp (LL)')
    ax[0, 1].set_axis_off()
    ax[0, 2].set_axis_off()

    for l in range(L):
        lh, hl, hh = dwt[l+1]
        ax[l+1, 0].imshow(lh/np.abs(lh).max()/2+0.5)
        ax[l+1, 1].imshow(hl/np.abs(hl).max()/2+0.5)
        ax[l+1, 2].imshow(hh/np.abs(hh).max()/2+0.5)

    ax[1, 0].set_title('horz (LH)')
    ax[1, 1].set_title('vert (HL)')
    ax[1, 2].set_title('diag (HH)')

    for l in range(L):
        ax[l+1, 0].set_ylabel(f'nivo {L-l}')

    return fig

def show_results(dwt_fused, ind_fused, img_fused, dwts, image_avg):
    
    if RESULTS:
        for i in range(len(dwts)):
            show_dwt(dwts[i])
            pyplot.suptitle('DWT slike ' + str(i+1))

        show_dwt(ind_fused)
        pyplot.suptitle('Izbrani indeksi zlivanja')
        
        show_dwt(dwt_fused)
        pyplot.suptitle('DWT zlite slike')

    fig, ax = pyplot.subplots(1, 2)
    ax = ax.ravel()
    ax[0].imshow(image_avg)
    ax[0].axis('off')
    ax[0].set_title('povprečena slika')
    ax[1].imshow(img_fused)
    ax[1].axis('off')
    ax[1].set_title('zlita slika')

    pyplot.show()

def get_coefficients(dwts, L):
    approximation = []
    details_arr = []

    for dwt in dwts:
        approximation.append(dwt[0])
        details = []
        for i in range(L):
            details.append([dwt[i][0], dwt[i][1], dwt[i][2]])

        details_arr.append(details)

    return approximation, details_arr

def faktor_aktivnosti(coeff):
    return np.abs(coeff)


def weighted_average(data):
    # izračun aktivnosti z oknom - Točka 4
    S = window_3
    return ndimage.convolve(np.array(data), S, mode='reflect')

def get_weights(images, isDetails=False):
    contrasts = []
    saturations = []
    exposures = []

    e = pow(10, -12)
    factors = []
    factorsSum = 0

    counter = 0

    if RESULTS:
        fig, ax = pyplot.subplots(len(images), 4, figsize=(10, 10), constrained_layout=True)
        fig.suptitle("Kontrast, Saturacija in Izpostavljenost")

    for image in images:

        # Calculate contrast - laplacian filter * gray_image
        coeffs = np.array([0.114, 0.587, 0.229])
        gray = np.dot(image, coeffs)
        laplacian = cv2.Laplacian(gray, cv2.CV_64F)

        if IS_WINDOW:
            contrasts.append(weighted_average(np.abs(laplacian * gray) + e))
        else:
            contrasts.append(np.abs(laplacian * gray) + e)

        # Calculate saturation - stdandard deviations of r,g,b components
        if IS_WINDOW:
            saturations.append(weighted_average(np.std([image[:,:,0], image[:,:,1], image[:,:,2]], axis=0) + e))
        else:
            saturations.append(np.std([image[:,:,0], image[:,:,1], image[:,:,2]], axis=0) + e)

        # Calculate exposure
        if not isDetails:
            if IS_WINDOW:
                exposures.append(weighted_average(np.exp(-pow((image[:,:,0] - 0.5*64), 2) / (2 * pow(0.2, 2))) + np.exp(-pow((image[:,:,1] - 0.5*64), 2) / (2 * pow(0.2, 2))) + np.exp(-pow((image[:,:,2] - 0.5*64), 2) / (2 * pow(0.2, 2))) + e))
                print(image.max())
            else:
                exposures.append(np.exp(-pow((image[:,:,0] - 0.5), 2) / (2 * pow(0.2, 2))) + np.exp(-pow((image[:,:,1] - 0.5), 2) / (2 * pow(0.2, 2))) + np.exp(-pow((image[:,:,2] - 0.5), 2) / (2 * pow(0.2, 2))) + e)

        if RESULTS:
            ax[counter][0].imshow(image, cmap='gray')
            ax[counter][1].imshow(contrasts[len(contrasts)-1], cmap='gray')
            ax[counter][2].imshow(saturations[len(saturations)-1], cmap='gray')

            if not isDetails:
                ax[counter][3].imshow(exposures[len(exposures)-1], cmap='gray')

        counter += 1

    for i in range(len(images)):
        factorsSum += contrasts[i] * saturations[i] * (exposures[i] if not isDetails else 1)

    # Avoid zero division
    factorsSum[factorsSum == 0] = 1

    for i in range(len(images)):
        factors.append((contrasts[i] * saturations[i] * (exposures[i] if not isDetails else 1)) / (factorsSum))

    return np.array(factors)

def weight_activity(coeff, type):

    if type == "approximation":
        W = get_weights(coeff)
    elif type == "details":
        W = get_weights(coeff, isDetails=True)

    return np.array(W)

def dwt_fusion(dwts, level, wavelet, weights):
    
    # Approximation coefficient and details coefficients
    cA, cD = get_coefficients(dwts, level)
    
    # Izračunamo aktivnosti pribljižkov (approximation) - Točka 2
    if FUSION_METHOD == "Advanced":
        approximated_act = np.array(weight_activity(cA, 'approximation'))
    else:
        approximated_act = np.array(cA)

    # poiščemo maksimalno aktivnost
    approximated_ind = np.argmax(approximated_act, axis=0)

    approximated_fused = np.zeros(np.array(cA[0]).shape)
    
    # obteženo zlivanje koeficientov - Točka 5
    if IS_WEIGHTED_FUSION:
        for d in range(len(dwts)):
            for i in range(approximated_act.shape[1]):
                for j in range(approximated_act.shape[2]):
                    approximated_fused[i][j] += approximated_act[d][i][j] * cA[d][i][j]
    else:
        for i in range(len(cA)):
            approximated_fused[approximated_ind==i] = cA[i][approximated_ind==i]

    dwt_fused = [approximated_fused]
    ind_fused = [approximated_ind]

    # Zlivanje za vsak nivo posebej
    for l in range(level):

        horz, vert, diag = [], [], []

        # pridobimo robove slik (detajli)
        for dwt in dwts:
            h, v, d = dwt[l+1]
            horz.append(h)
            vert.append(v)
            diag.append(d)
        
        # izračunamo aktivnosti podrobnosti (detail) - Točka 3
        if FUSION_METHOD == "Advanced":
            horz_act = weight_activity(horz, "details")
            vert_act = weight_activity(vert, "details")
            diag_act = weight_activity(diag, "details")
        else:
            horz_act = np.array([faktor_aktivnosti(h) for h in horz])
            vert_act = np.array([faktor_aktivnosti(v) for v in vert])
            diag_act = np.array([faktor_aktivnosti(d) for d in diag])

        # poiščemo sliko z maksimalno aktivnostjo v vsakem nivoju
        # single scale grouping (povprečje) - Točka 6
        if FUSION_METHOD == "Advanced":
            detail_acts = np.mean(np.array([horz_act, vert_act, diag_act]), axis=0)
            hv_ind = np.argmax(detail_acts, axis=0)
            vv_ind = np.argmax(detail_acts, axis=0)
            dv_ind = np.argmax(detail_acts, axis=0)
        else:
            hv_ind = np.argmax(horz_act, axis=0)
            vv_ind = np.argmax(vert_act, axis=0)
            dv_ind = np.argmax(diag_act, axis=0)
        
        # sestavimo zlite slike robov
        horz_zlita = np.zeros(np.array(horz[0]).shape)
        vert_zlita = np.zeros(np.array(vert[0]).shape)
        diag_zlita = np.zeros(np.array(diag[0]).shape)

        # obteženo zlivanje koeficientov - Točka 5
        if IS_WEIGHTED_FUSION:
            for d in range(len(dwts)):
                for i in range(horz_act.shape[1]):
                    for j in range(horz_act.shape[2]):
                        horz_zlita[i][j] += horz_act[d][i][j] * horz[d][i][j]
                        vert_zlita[i][j] += vert_act[d][i][j] * vert[d][i][j]
                        diag_zlita[i][j] += diag_act[d][i][j] * diag[d][i][j]
        else:
            for i in range(len(dwts)):
                horz_zlita[hv_ind==i] = np.array(horz)[i][hv_ind==i] # nastavi max       
                vert_zlita[vv_ind==i] = np.array(vert)[i][vv_ind==i] # nastavi max
                diag_zlita[dv_ind==i] = np.array(diag)[i][dv_ind==i] # nastavi max
        
        # dodamo na konec seznama
        dwt_fused.append([horz_zlita, vert_zlita, diag_zlita])
        ind_fused.append([hv_ind, vv_ind, dv_ind])
    
    img_fused = pywt.waverec2(dwt_fused, wavelet, axes=(0, 1))
    img_fused = np.maximum(np.minimum(img_fused, 1), 0)

    return dwt_fused, ind_fused, img_fused
        



def dwt_images(images, wavelet, level, axes):
    dwts = []
    for image in images:
        dwt = pywt.wavedec2(image, wavelet, level=level, axes=axes)  
        dwts.append(dwt)

    return np.array(dwts)

def read_images():
    images = []
    for root, dirs, files in os.walk(FILE):
        for name in files:
            image = pyplot.imread(FILE + "/" + name)

            # Check image shape
            if images and image.shape != images[0].shape:
                image = cv2.resize(image, images[0].shape, interpolation = cv2.INTER_AREA)
                
            images.append(image/255.)

    image_avg = np.mean(images, 0)
    
    return images, image_avg


def main():
    dwts=[]; wavelet='haar'; level=6 #sym3, haar

    # Read images
    images, image_avg = read_images()

    # Calculate weights W
    weights = get_weights(images)

    # Discrete wavelet transform
    dwts = dwt_images(images, wavelet, level, axes=(0,1))

    # Discrete wavelet transform fusion
    dwt_fused, ind_fused, img_fused = dwt_fusion(dwts, level, wavelet, weights)

    show_results(dwt_fused, ind_fused, img_fused, dwts, image_avg)


if __name__ == "__main__":
    main()