import unittest
import numpy as np
from matplotlib.pyplot import imread
from skimage.draw import polygon

import segmentacija

def imread_uint8_gray(ime):
    slika = imread(ime)
    if slika.dtype != np.uint8:
        slika = np.uint8(slika*255)
    if slika.ndim == 3:
        slika = slika[:, :, 1]
    return slika

class TestPoisciKonture(unittest.TestCase):
    def test_1_poisci_konture_primer3(self):
        slika = imread_uint8_gray('tests/primer3.png')
        maska_ref = imread_uint8_gray('tests/primer3_ref.png')

        konture = segmentacija.poisci_konture(slika, 1000)

        self.assertIsInstance(konture, list)
        self.assertEqual(len(konture), 4)

        maska = np.zeros(slika.shape, dtype=np.bool)
        for k in konture:
            self.assertIsInstance(k, np.ndarray)
            self.assertEqual(k.ndim, 2)

            k_rr, k_cc = polygon(k[:, 0], k[:, 1])
            maska[k_rr, k_cc] = True

        presek = (maska*maska_ref).sum()
        unija = (maska+maska_ref).sum()
        
        self.assertGreaterEqual(presek/unija, 0.8)

    def test_2_poisci_konture_primer5(self):
        slika = imread_uint8_gray('tests/primer5.png')
        maska_ref = imread_uint8_gray('tests/primer5_ref.png')

        konture = segmentacija.poisci_konture(slika, 1000)

        self.assertIsInstance(konture, list)
        self.assertEqual(len(konture), 4)

        maska = np.zeros(slika.shape, dtype=np.bool)
        for k in konture:
            self.assertIsInstance(k, np.ndarray)
            self.assertEqual(k.ndim, 2)

            k_rr, k_cc = polygon(k[:, 0], k[:, 1])
            maska[k_rr, k_cc] = True

        presek = (maska*maska_ref).sum()
        unija = (maska+maska_ref).sum()
        
        self.assertGreaterEqual(presek/unija, 0.8)

    def test_3_analiziraj_konture_primer3(self):
        slika = imread_uint8_gray('tests/primer3.png')
        analiza_ref = np.array([[105.42, 508.71,  80.05,  19.95, 233.18],
                                [342.98, 138.55,  80.24,  19.76, 126.95],
                                [138.47, 187.69,  66.37,  33.63, 334.65],
                                [340.21, 461.9 ,  88.54,  11.46, 153.42]])

        konture = segmentacija.poisci_konture(slika, 1000)
        self.assertEqual(len(konture), 4,
            msg='stevilo najdenih kontur se ne ujema s predvidenim')

        analiza = segmentacija.analiziraj_konture(konture)

        self.assertEqual(analiza.shape[0], len(konture),
            msg='vrstice tabele analize se ne ujemamo s stevilom kontur')
        self.assertEqual(analiza.shape[1], 5, 
            msg='stevilo stolpcev tabele analize se ne ujema s predvidenim')

        cent = analiza[:, :2]
        cent_ref = analiza_ref[:, :2]

        cent_diff = cent.reshape(-1, 1, 2)-cent_ref.reshape(1, -1, 2)
        cent_dist = (cent_diff**2).sum(2)**0.5

        ref_sort_ind = np.argmin(cent_dist, 1)

        np.testing.assert_array_equal(np.sort(ref_sort_ind),
            np.arange(4),
            err_msg='centri najdenih elementov se ne ujemajo s predvidenimi')

        analiza_ref = analiza_ref[ref_sort_ind, :]

        np.testing.assert_array_less(
            np.abs(analiza_ref[:, :2] - analiza[:, :2]),
            10, err_msg='napaka centroidov je vecja od 10 pikslov')

        np.testing.assert_array_less(
            np.abs(analiza_ref[:, 2:4] - analiza[:, 2:4]),
            10, err_msg='napaka dolzin osi je vecja od 10')

        np.testing.assert_array_less(
            np.abs(analiza_ref[:, 4] - analiza[:, 4]),
            3, err_msg='napaka kotov orientacij je vecja od 3 stopinj')

    def test_4_analiziraj_konture_primer6_1(self):
        slika = imread_uint8_gray('tests/primer6_1.png')
        analiza_ref = np.array([[45.05, 38.39, 65.03, 34.97, 26.88]])

        konture = segmentacija.poisci_konture(slika, 1000)
        self.assertIsInstance(konture, list)
        self.assertEqual(len(konture), 1, 
                        msg='stevilo najdenih kontur se ne ujema s predvidenim')

        analiza = segmentacija.analiziraj_konture(konture)

        np.testing.assert_array_less(
            np.abs(analiza_ref[:, :2] - analiza[:, :2]),
            5, err_msg='napaka centroidov je vecja od 5 pikslov')

        np.testing.assert_array_less(
            np.abs(analiza_ref[:, 2:4] - analiza[:, 2:4]),
            10, err_msg='napaka dolzin osi je vecja od 10')

        np.testing.assert_array_less(
            np.abs(analiza_ref[:, 4] - analiza[:, 4]),
            3, err_msg='napaka kotov orientacij je vecja od 3 stopinj')

    def test_5_analiziraj_konture_primer6_2(self):
        slika = imread_uint8_gray('tests/primer6_2.png')
        analiza_ref = np.array([[ 55.89, 50.7, 65.42, 34.58, 131.27]])

        konture = segmentacija.poisci_konture(slika, 1000)
        self.assertIsInstance(konture, list)
        self.assertEqual(len(konture), 1, 
                        msg='stevilo najdenih kontur se ne ujema s predvidenim')

        analiza = segmentacija.analiziraj_konture(konture)

        np.testing.assert_array_less(
            np.abs(analiza_ref[:, :2] - analiza[:, :2]),
            5, err_msg='napaka centroidov je vecja od 5 pikslov')

        np.testing.assert_array_less(
            np.abs(analiza_ref[:, 2:4] - analiza[:, 2:4]),
            10, err_msg='napaka dolzin osi je vecja od 10')

        np.testing.assert_array_less(
            np.abs(analiza_ref[:, 4] - analiza[:, 4]),
            3, err_msg='napaka kotov orientacij je vecja od 3 stopinj')

    def test_6_analiziraj_konture_primer6_3(self):
        slika = imread_uint8_gray('tests/primer6_3.png')
        analiza_ref = np.array([[ 42.29, 49.7, 65.13, 34.87, 236.36]])

        konture = segmentacija.poisci_konture(slika, 1000)
        self.assertIsInstance(konture, list)
        self.assertEqual(len(konture), 1, 
                        msg='stevilo najdenih kontur se ne ujema s predvidenim')

        analiza = segmentacija.analiziraj_konture(konture)

        np.testing.assert_array_less(
            np.abs(analiza_ref[:, :2] - analiza[:, :2]),
            5, err_msg='napaka centroidov je vecja od 5 pikslov')

        np.testing.assert_array_less(
            np.abs(analiza_ref[:, 2:4] - analiza[:, 2:4]),
            10, err_msg='napaka dolzin osi je vecja od 10')

        np.testing.assert_array_less(
            np.abs(analiza_ref[:, 4] - analiza[:, 4]),
            3, err_msg='napaka kotov orientacij je vecja od 3 stopinj')

    def test_7_analiziraj_konture_primer6_4(self):
        slika = imread_uint8_gray('tests/primer6_4.png')
        analiza_ref = np.array([[ 49.29, 42.29, 65.12, 34.88, 326.45]])

        konture = segmentacija.poisci_konture(slika, 1000)
        self.assertIsInstance(konture, list)
        self.assertEqual(len(konture), 1, 
                        msg='stevilo najdenih kontur se ne ujema s predvidenim')

        analiza = segmentacija.analiziraj_konture(konture)

        np.testing.assert_array_less(
            np.abs(analiza_ref[:, :2] - analiza[:, :2]),
            5, err_msg='napaka centroidov je vecja od 5 pikslov')

        np.testing.assert_array_less(
            np.abs(analiza_ref[:, 2:4] - analiza[:, 2:4]),
            10, err_msg='napaka dolzin osi je vecja od 10')

        np.testing.assert_array_less(
            np.abs(analiza_ref[:, 4] - analiza[:, 4]),
            3, err_msg='napaka kotov orientacij je vecja od 3 stopinj')



if __name__ == '__main__':
    unittest.main(verbosity=2)
